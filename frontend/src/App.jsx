import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import Login from './components/Login';
import ChatLayout from './components/ChatLayout';

function App() {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
  }, []);

  return (
    <Routes>
      <Route path="/" element={user ? <Navigate to="/chat" /> : <Navigate to="/login" />} />
      <Route path="/login" element={!user ? <Login setUser={setUser} /> : <Navigate to="/chat" />} />
      <Route path="/chat" element={user ? <ChatLayout user={user} setUser={setUser} /> : <Navigate to="/login" />} />
    </Routes>
  );
}

export default App;
