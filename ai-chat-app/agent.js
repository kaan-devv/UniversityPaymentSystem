require("dotenv").config();
const { initializeApp } = require("firebase/app");
const { getFirestore, collection, query, onSnapshot, addDoc, updateDoc } = require("firebase/firestore");
const OpenAI = require("openai");
const axios = require("axios");

// -----------------------------------------------------------
// 1. CONFIGURATION
// -----------------------------------------------------------

// A) Your OpenAI API Key
const openai = new OpenAI({
  apiKey: process.env.OPENAI_API_KEY,
});

// B) Your Firebase Config
const firebaseConfig = {
  apiKey: "AIzaSyCoMKe_hcyjVbEGcyw1y35lcuMn7JQnSM0",
  authDomain: "aichatproject-f22d6.firebaseapp.com",
  projectId: "aichatproject-f22d6",
  storageBucket: "aichatproject-f22d6.firebasestorage.app",
  messagingSenderId: "593146087012",
  appId: "1:593146087012:web:5231e0f6e1f16a98d04171",
  measurementId: "G-1K60QJFD5N"
};

// C) REAL API GATEWAY URL
const GATEWAY_URL = "http://localhost:5088"; 

// -----------------------------------------------------------

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

console.log("---------------------------------------");
console.log("🚀 AI Agent Connecting to Real API...");
console.log(`🔗 Target Gateway: ${GATEWAY_URL}`);

// Listen to Database
const q = query(collection(db, "chats"));

onSnapshot(q, (snapshot) => {
  snapshot.docChanges().forEach(async (change) => {
    if (change.type === "added") {
      const data = change.doc.data();
      
      // Reply only to 'user' messages that are not processed
      if (data.sender === "user" && !data.processed) {
        console.log(`📩 Processing Message: ${data.text}`);
        await updateDoc(change.doc.ref, { processed: true });
        await processMessage(data.text);
      }
    }
  });
});

// OpenAI Function Definitions (The "Tools" needed for Assignment 3)
const tools = [
  {
    type: "function",
    function: {
      name: "get_tuition_info",
      description: "Queries the student's tuition fee/debt information.",
      parameters: {
        type: "object",
        properties: {
          studentNumber: { type: "string", description: "The student ID number (e.g., '101')" },
        },
        required: ["studentNumber"],
      },
    },
  },
  {
    type: "function",
    function: {
      name: "pay_tuition",
      description: "Processes a tuition payment for a student.",
      parameters: {
        type: "object",
        properties: {
          studentNumber: { type: "string", description: "The student ID number" },
          amount: { type: "number", description: "The amount to pay" },
          term: { type: "string", description: "The term to pay for (e.g. 'Fall 2025')" }
        },
        required: ["studentNumber", "amount"],
      },
    },
  },
  {
    type: "function",
    function: {
      name: "create_student_tuition",
      description: "ADMIN ONLY: Creates a new student record with tuition debt.",
      parameters: {
        type: "object",
        properties: {
          studentNumber: { type: "string" },
          firstName: { type: "string" },
          lastName: { type: "string" },
          term: { type: "string", description: "e.g. 'Fall 2025'" },
          amount: { type: "number" }
        },
        required: ["studentNumber", "firstName", "lastName", "term", "amount"],
      },
    },
  },
];

async function processMessage(userMessage) {
  try {
    const completion = await openai.chat.completions.create({
      model: "gpt-3.5-turbo", // or gpt-4o if available
      messages: [
        { role: "system", content: "You are a university payment assistant. You help students check debts and make payments. You also help Admins create new students. Keep answers professional and in English." },
        { role: "user", content: userMessage }
      ],
      tools: tools,
      tool_choice: "auto",
    });

    const responseMessage = completion.choices[0].message;

    // A) If OpenAI decides to call your C# API
    if (responseMessage.tool_calls) {
      const toolCall = responseMessage.tool_calls[0];
      const fnName = toolCall.function.name;
      const args = JSON.parse(toolCall.function.arguments);
      
      console.log(`⚙️ API Call Triggered: ${fnName} with args:`, args);
      
      let apiResponseText = "";

      try {
        // 1. QUERY TUITION
        if (fnName === "get_tuition_info") {
            // Gateway Route: GET /api/tuition/{id}
            const res = await axios.get(`${GATEWAY_URL}/api/tuition/${args.studentNumber}`);
            
            // Formatting response from C# DTO
            const data = res.data;
            apiResponseText = `Student: ${data.fullName}\nTotal Debt: ${data.totalAmount} TL\nPaid: ${data.paidAmount} TL\nRemaining Balance: ${data.balance} TL\nStatus: ${data.records[0]?.status || 'Unknown'}`;
        } 
        
        // 2. PAY TUITION
        else if (fnName === "pay_tuition") {
            // Gateway Route: POST /api/payment
            // Default term if AI didn't catch it
            const payload = {
                studentNumber: args.studentNumber,
                amount: args.amount,
                term: args.term || "Fall 2025" 
            };
            const res = await axios.post(`${GATEWAY_URL}/api/payment`, payload);
            apiResponseText = `Payment Successful! ✅\nNew Balance: ${res.data.balance} TL\nTransaction Ref: ${res.data.transactionReference}`;
        }

        // 3. CREATE STUDENT (ADMIN)
        else if (fnName === "create_student_tuition") {
            // Gateway Route: POST /api/admin/tuition
            const res = await axios.post(`${GATEWAY_URL}/api/admin/tuition`, args);
            apiResponseText = `Admin Action Successful: Student ${args.studentNumber} (${args.firstName} ${args.lastName}) has been created/updated with ${args.amount} TL debt.`;
        }

      } catch (error) {
        console.error("❌ API Error:", error.response ? error.response.data : error.message);
        apiResponseText = "System Error: I could not complete the operation. Please check the Student ID or try again.";
      }

      // Send the API result back to the chat
      await sendBotMessage(apiResponseText);
    } 
    // B) Normal Chat (No API needed)
    else {
      await sendBotMessage(responseMessage.content);
    }
  } catch (error) {
    console.error("OpenAI Error:", error);
    await sendBotMessage("I am currently experiencing connection issues. Please try again later.");
  }
}

async function sendBotMessage(text) {
  await addDoc(collection(db, "chats"), {
    text: text,
    sender: "bot",
    sentTime: new Date()
  });
  console.log(`📤 Response sent: ${text}`);
}