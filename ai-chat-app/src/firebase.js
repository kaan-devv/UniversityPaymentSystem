import { initializeApp } from "firebase/app";
import { getFirestore } from "firebase/firestore";


const firebaseConfig = {
  apiKey: "AIzaSyCoMKe_hcyjVbEGcyw1y35lcuMn7JQnSM0",
  authDomain: "aichatproject-f22d6.firebaseapp.com",
  projectId: "aichatproject-f22d6",
  storageBucket: "aichatproject-f22d6.firebasestorage.app",
  messagingSenderId: "593146087012",
  appId: "1:593146087012:web:5231e0f6e1f16a98d04171",
  measurementId: "G-1K60QJFD5N"
};

const app = initializeApp(firebaseConfig);
export const db = getFirestore(app);