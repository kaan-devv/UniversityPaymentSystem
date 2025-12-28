import { useState, useEffect } from 'react'
import './App.css'
import '@chatscope/chat-ui-kit-styles/dist/default/styles.min.css';
import { MainContainer, ChatContainer, MessageList, Message, MessageInput, TypingIndicator } from '@chatscope/chat-ui-kit-react';
import { db } from './firebase';
import { collection, addDoc, query, orderBy, onSnapshot } from "firebase/firestore";

function App() {
  const [messages, setMessages] = useState([]);
  const [typing, setTyping] = useState(false);

  // 1. Mesajları Firebase'den Canlı Dinle
  useEffect(() => {
    const q = query(collection(db, "chats"), orderBy("sentTime", "asc"));
    const unsubscribe = onSnapshot(q, (snapshot) => {
      const liveMessages = snapshot.docs.map(doc => ({
        message: doc.data().text,
        sender: doc.data().sender,
        direction: doc.data().sender === "user" ? "outgoing" : "incoming",
        position: "single"
      }));
      setMessages(liveMessages);
      
      // Eğer son mesajı "user" attıysa, bot düşünüyor göster
      if(liveMessages.length > 0 && liveMessages[liveMessages.length-1].sender === "user"){
        setTyping(true);
      } else {
        setTyping(false);
      }
    });

    return () => unsubscribe();
  }, []);

  // 2. Mesaj Gönderme Fonksiyonu
  const handleSend = async (messageText) => {
    // Önce mesajı Firebase'e kaydet
    await addDoc(collection(db, "chats"), {
      text: messageText,
      sender: "user",
      sentTime: new Date()
    });
  };

  return (
    <div style={{ position: "relative", height: "100vh", width: "100vw" }}>
      <MainContainer>
        <ChatContainer>
          <MessageList typingIndicator={typing ? <TypingIndicator content="Asistan yazıyor..." /> : null}>
            {messages.map((msg, i) => {
              return <Message key={i} model={msg} />
            })}
          </MessageList>
          <MessageInput placeholder="Mesajınızı yazın..." onSend={handleSend} />
        </ChatContainer>
      </MainContainer>
    </div>
  )
}

export default App;