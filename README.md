# University Payment System API (Group 2)

This project is a cloud-based **University Tuition Payment System API** developed for the SE 4458 Software Architecture course. The system handles student debt queries and payment processing via external clients.

 🚀 **MAJOR UPDATE (Assignment 3):** The system has evolved into an **AI-Powered Full Stack Application**. It now features an **OpenAI Chatbot**, a **React Frontend**, and has migrated from SQL Server to **Google Firestore (NoSQL)** for real-time capabilities.

## 🚀 Key Features and Hosting

* **Platform:** .NET 8.0 (Core Web API) & Node.js (AI Agent).
* **Database:** **Google Firestore (NoSQL)** (Migrated from AWS RDS for Assignment 3).
* **AI Engine:** **OpenAI GPT-3.5** with Function Calling (Tools).
* **Frontend:** **React + Vite** (Chat Interface).
* **API Gateway:** Ocelot (Routing traffic to Backend).
* **Hosting:** The API Core is compatible with Azure App Service (Linux).

---

## 🏗️ Architecture & Design Decisions

1.  **Deployment Strategy (Resource Efficiency):**
    * The **API** and the **Gateway** share a single **Free (F1) App Service Plan** in Azure to host both applications, demonstrating resource efficiency and meeting cloud hosting requirements.
    * The Gateway's routing rules were updated from `localhost` to the live Azure HTTPS URL.

2.  **Security & Compliance:**
    * **Rate Limiting:** Mobile tuition queries are strictly limited to **3 requests per day** at the Gateway level, as required.
    * **Custom Logging:** The Gateway includes custom middleware to log critical Request/Response details (IP, Latency, Status Code) for monitoring purposes.
    * **JWT Auth:** Banking App and Admin endpoints require a valid JWT Token.

    **Project Structure (Monorepo)**
The solution now follows a microservice-like structure with three main components:
```text
UniversityPaymentSystem/
│
├── UniversityPaymentApi/    # .NET 8 Backend (Business Logic & Firestore Connection)
├── UniversityGateway/       # Ocelot API Gateway (Routes traffic to API)
└── ai-chat-app/             # React Frontend + Node.js AI Agent (The Brain)

---

## 🗂️ Data Model (ER Diagram)

The database schema and key relationships are defined as follows:

![Database ER Diagram](ERDiagram.png)

---

## ⚠️ Challenges and Solutions

This project involved several non-trivial issues solved during development, ranging from Cloud Deployment to AI Integration:

### Assignment 3: AI & Database Challenges
* **Database Migration (SQL to NoSQL):**
    * *Problem:* Converting the existing Entity Framework (Relational) logic to match Firestore's Document-based structure.
    * *Solution:* The Repository layers were rewritten to handle **Firestore Collections and Documents** asynchronously, effectively decoupling the system from SQL tables.

* **AI Function Calling Precision:**
    * *Problem:* Making the Large Language Model (LLM) reliably understand *when* to call an API endpoint and with *what* specific parameters.
    * *Solution:* Defined strict **JSON schemas for tools** within the OpenAI configuration, ensuring the Agent accurately extracts parameters like `studentNumber` and `amount` from natural language.

### Assignment 2: Cloud & Deployment Challenges
* **Quota Exceeded (App Service Plan Creation):**
    * *Problem:* Attempting to create a second Free (F1) App Service Plan resulted in a "Quota Exceeded" error.
    * *Solution:* The Gateway was successfully deployed by configuring it to **reuse the existing App Service Plan** that was created for the API, circumventing the plan creation limit.

* **Port Binding Conflict in Azure:**
    * *Problem:* The Gateway failed to start because its code hardcoded the local development port 5200, preventing Azure from communicating on port 8080.
    * *Solution:* The hardcoded `UseUrls` line was removed from the Gateway's `Program.cs`, allowing the application to properly bind to the port provided by the Azure environment.

* **Silent Deployment Failure (Missing Config):**
    * *Problem:* The initial deployment method failed to copy configuration files (`ocelot.json`) and core DLLs.
    * *Solution:* The deployment strategy was changed to use the **`dotnet publish`** command to create a complete package, and the Gateway's `.csproj` file was updated to explicitly force copy `ocelot.json`.

* **Project Structure Conflict:**
    * *Problem:* Moving the Gateway project inside the API project folder caused compilation errors (`CS8802`).
    * *Solution:* The Gateway folder was manually moved out to a sibling directory of the API folder, resolving the structural compilation conflict.

* **JWT Key Size Error:**
    * *Problem:* The original JWT secret key was too short (184 bits), causing a runtime failure.
    * *Solution:* The secret key value was increased in length to meet the security algorithm's requirement (256 bits).

---

## ✅ Live Capabilities Verification

The system capabilities have been verified across two stages: the new **AI-Powered Setup** (Assignment 3) and the live **Azure Cloud Deployment** (Assignment 2).

### 🟢 Assignment 3 Features (AI Agent & Firestore)
*These features function via the Chat Interface and Google Firestore:*

* **Admin (Data Seeding via AI):**
    * *Command:* "Create a new student named **Ahmet Yilmaz** with ID **101**, Term **Fall 2025**, Amount **15000**."
    * *Verification:* Agent triggers `create_student_tuition` tool → Gateway → AdminController → Document created in Firestore.

* **Student (Query via AI):**
    * *Command:* "How much debt does student **101** have?"
    * *Verification:* Agent triggers `get_tuition_info` tool → Gateway → StudentController → Real-time balance fetched from Firestore.

* **Payment (Transaction via AI):**
    * *Command:* "I want to pay **5000** TL for student **101**."
    * *Verification:* Agent triggers `pay_tuition` tool → Gateway → PaymentController → Balance updated in Firestore.

### 🔵 Assignment 2 Features (Azure Deployment)
*The following features are functional and verifiable on the live Azure deployment:*

* **End-to-End Connectivity:** Accessing the mobile endpoint successfully returns a JSON response (or "Not Found" if the student ID is invalid), confirming the path from **Browser → Gateway → API → AWS RDS** is fully operational.

* **Rate Limiting:** The Mobile App query is successfully blocked. Making the same request **4 times consecutively** results in a **`429 Too Many Requests`** error.

* **Authentication (JWT):** Accessing the Banking App endpoint without a token correctly returns a **`401 Unauthorized`** error.

* **Custom Logging:** The Azure Log Stream displays unique, color-coded log entries (including **IP**, **Latency**, and **HTTP Status Code**) for every transaction processed by the Gateway.

---

## 🛠️ Final Deliverables

**1. Live Deployment URL (Assignment 2 - Azure):**
*The legacy cloud deployment for the API and Gateway:*

* **Gateway Entry Point:** `https://uni-gateway-api-[your-suffix].azurewebsites.net/gateway/mobile/tuition/{studentNumber}`
* **Swagger UI (Backend):** `https://uni-payment-api-decgg9f8bhdtgfd8.westeurope-01.azurewebsites.net/swagger/index.html`

**2. Local Setup (Assignment 3 - Full AI System):**
*To run the complete system (Chatbot + Gateway + API + Firestore), open 3 separate terminals:*

* **Step 1 (Backend):** `cd UniversityPaymentApi` then `dotnet run` (Port: 5074)

* **Step 2 (Gateway):** `cd UniversityGateway` then `dotnet run` (Port: 5088)

* **Step 3 (Frontend & Agent):** `cd ai-chat-app`
  * Install libs: `npm install`
  * Run Agent: `node agent.js` (In one terminal)
  * Run UI: `npm run dev` (In a separate terminal)