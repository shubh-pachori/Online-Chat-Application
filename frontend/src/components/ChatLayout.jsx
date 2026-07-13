import React, { useState, useRef, useEffect } from 'react';
import { Send, Image, Video, Mic, Paperclip, MoreVertical, Search, LogOut, X, Edit2, Check } from 'lucide-react';

const Modal = ({ isOpen, onClose, title, children }) => {
  if (!isOpen) return null;
  return (
    <div style={{ position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50, backdropFilter: 'blur(4px)' }}>
      <div className="glass-panel animate-fade-in" style={{ width: '100%', maxWidth: '400px', padding: '1.5rem', borderRadius: '1rem', position: 'relative' }}>
        <button onClick={onClose} style={{ position: 'absolute', top: '1rem', right: '1rem', background: 'transparent', border: 'none', cursor: 'pointer', color: 'var(--text-secondary)' }}>
          <X size={20} />
        </button>
        <h3 style={{ marginBottom: '1.5rem', fontSize: '1.25rem', fontWeight: 'bold' }}>{title}</h3>
        {children}
      </div>
    </div>
  );
};

const ChatLayout = ({ user, setUser }) => {
  const [messages, setMessages] = useState([
    { id: 1, content: "Hey, how are you?", senderId: '2', time: "10:30 AM" },
    { id: 2, content: "I'm good, working on the new app!", senderId: user.id, time: "10:32 AM" }
  ]);
  const [inputText, setInputText] = useState('');
  const [searchEmail, setSearchEmail] = useState('');
  const [searchResults, setSearchResults] = useState(null);
  const [showUserProfile, setShowUserProfile] = useState(false);
  const [showGroupProfile, setShowGroupProfile] = useState(false);
  const fileInputRef = useRef(null);

  // Profile Edit State
  const [editProfile, setEditProfile] = useState({ username: user.username, phone: user.phoneNumber || '', dob: user.dateOfBirth ? user.dateOfBirth.split('T')[0] : '' });
  
  // Dummy Group State
  const [groupDetails, setGroupDetails] = useState({ name: 'Design Team', description: 'UI/UX squad', members: 3 });
  
  const handleLogout = () => {
    localStorage.removeItem('user');
    setUser(null);
  };

  const handleSend = (e) => {
    e.preventDefault();
    if (!inputText.trim()) return;
    
    setMessages([...messages, {
      id: Date.now(),
      content: inputText,
      senderId: user.id,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    }]);
    setInputText('');
  };

  const handleSearch = (e) => {
    e.preventDefault();
    if (!searchEmail) {
        setSearchResults(null);
        return;
    }
    // Simulate search logic
    if (searchEmail.includes('admin') || searchEmail.includes('dummy')) {
        setSearchResults({ id: '99', username: searchEmail.split('@')[0], email: searchEmail });
    } else {
        setSearchResults(null);
        alert('User not found.');
    }
  };

  const handleFileUpload = (e) => {
      const file = e.target.files[0];
      if (!file) return;
      // Simulate successful upload and message send
      const url = URL.createObjectURL(file);
      const isVideo = file.type.startsWith('video/');
      setMessages([...messages, {
          id: Date.now(),
          mediaUrl: url,
          mediaType: isVideo ? 'Video' : 'Image',
          senderId: user.id,
          time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      }]);
  };

  const saveUserProfile = () => {
      // Simulate API Call
      const updatedUser = { ...user, username: editProfile.username, phoneNumber: editProfile.phone, dateOfBirth: editProfile.dob };
      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
      setShowUserProfile(false);
  };

  const saveGroupProfile = () => {
      // Simulate API call
      setShowGroupProfile(false);
  };

  return (
    <div style={{ display: 'flex', height: '100vh', padding: '1rem', gap: '1rem' }}>
      {/* Sidebar */}
      <div className="glass-panel" style={{ width: '350px', borderRadius: '1rem', display: 'flex', flexDirection: 'column' }}>
        <div style={{ padding: '1.5rem', borderBottom: '1px solid var(--border-color)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', cursor: 'pointer' }} onClick={() => setShowUserProfile(true)}>
            <div style={{ width: '40px', height: '40px', borderRadius: '50%', backgroundColor: 'var(--primary-color)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold' }}>
              {user.username.charAt(0).toUpperCase()}
            </div>
            <div>
              <h3 style={{ fontSize: '1rem' }}>{user.username}</h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>My Profile</p>
            </div>
          </div>
          <button className="btn" style={{ background: 'transparent', padding: '0.5rem' }} onClick={handleLogout}>
            <LogOut size={20} color="var(--text-secondary)" />
          </button>
        </div>
        
        <div style={{ padding: '1rem' }}>
          <form onSubmit={handleSearch} style={{ position: 'relative' }}>
            <button type="submit" style={{ position: 'absolute', left: '1rem', top: '50%', transform: 'translateY(-50%)', background: 'transparent', border: 'none', cursor: 'pointer', color: 'var(--text-secondary)' }}>
                <Search size={18} />
            </button>
            <input 
                type="email" 
                placeholder="Search user by email..." 
                className="input-field" 
                style={{ paddingLeft: '2.5rem' }} 
                value={searchEmail}
                onChange={e => setSearchEmail(e.target.value)}
            />
          </form>
        </div>

        <div style={{ flex: 1, overflowY: 'auto', padding: '0 1rem' }}>
          {searchResults && (
            <div style={{ display: 'flex', gap: '1rem', padding: '1rem', borderRadius: '0.5rem', background: 'rgba(59,130,246,0.1)', cursor: 'pointer', marginBottom: '1rem', border: '1px solid var(--primary-color)' }}>
                <div style={{ width: '48px', height: '48px', borderRadius: '50%', background: 'var(--primary-color)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>{searchResults.username.charAt(0).toUpperCase()}</div>
                <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <h4 style={{ fontWeight: '500' }}>{searchResults.username}</h4>
                    </div>
                    <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Click to chat</p>
                </div>
            </div>
          )}

          {/* Dummy chat list item */}
          <div style={{ display: 'flex', gap: '1rem', padding: '1rem', borderRadius: '0.5rem', background: 'rgba(255,255,255,0.05)', cursor: 'pointer' }}>
             <div style={{ width: '48px', height: '48px', borderRadius: '50%', background: '#475569', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>DT</div>
             <div style={{ flex: 1 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                   <h4 style={{ fontWeight: '500' }}>{groupDetails.name}</h4>
                   <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>10:35 AM</span>
                </div>
                <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', marginTop: '0.25rem', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>Tap to view details</p>
             </div>
          </div>
        </div>
      </div>

      {/* Main Chat Area */}
      <div className="glass-panel" style={{ flex: 1, borderRadius: '1rem', display: 'flex', flexDirection: 'column' }}>
        <div style={{ padding: '1.5rem', borderBottom: '1px solid var(--border-color)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', cursor: 'pointer' }} onClick={() => setShowGroupProfile(true)}>
            <div style={{ width: '40px', height: '40px', borderRadius: '50%', backgroundColor: '#475569', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>DT</div>
            <div>
              <h3 style={{ fontSize: '1.1rem' }}>{groupDetails.name}</h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{groupDetails.members} members (Click to edit)</p>
            </div>
          </div>
          <button className="btn" style={{ background: 'transparent', padding: '0.5rem' }}>
            <MoreVertical size={20} color="var(--text-secondary)" />
          </button>
        </div>

        <div style={{ flex: 1, padding: '1.5rem', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {messages.map(msg => (
            <div key={msg.id} style={{ alignSelf: msg.senderId === user.id ? 'flex-end' : 'flex-start', maxWidth: '70%' }}>
              <div style={{ 
                background: msg.senderId === user.id ? 'var(--primary-color)' : 'rgba(255,255,255,0.1)', 
                padding: msg.mediaUrl ? '0.5rem' : '1rem', 
                borderRadius: '1rem',
                borderBottomRightRadius: msg.senderId === user.id ? '0' : '1rem',
                borderBottomLeftRadius: msg.senderId === user.id ? '1rem' : '0',
              }}>
                {msg.mediaUrl && msg.mediaType === 'Image' && (
                  <img src={msg.mediaUrl} alt="Media" style={{ width: '100%', borderRadius: '0.5rem', marginBottom: msg.content ? '0.5rem' : '0' }} />
                )}
                {msg.mediaUrl && msg.mediaType === 'Video' && (
                  <video src={msg.mediaUrl} controls style={{ width: '100%', borderRadius: '0.5rem', marginBottom: msg.content ? '0.5rem' : '0' }} />
                )}
                {msg.content && <p>{msg.content}</p>}
              </div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.25rem', textAlign: msg.senderId === user.id ? 'right' : 'left' }}>
                {msg.time}
              </div>
            </div>
          ))}
        </div>

        <div style={{ padding: '1.5rem', borderTop: '1px solid var(--border-color)' }}>
          <form onSubmit={handleSend} style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
            <button type="button" className="btn" style={{ background: 'transparent', padding: '0.5rem', color: 'var(--text-secondary)' }} onClick={() => fileInputRef.current.click()}>
              <Paperclip size={20} />
            </button>
            <input type="file" ref={fileInputRef} style={{ display: 'none' }} accept="image/*,video/*" onChange={handleFileUpload} />
            
            <input 
              type="text" 
              placeholder="Type a message..." 
              className="input-field" 
              style={{ flex: 1, borderRadius: '2rem' }}
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
            />
            <button type="button" className="btn" style={{ background: 'transparent', padding: '0.5rem', color: 'var(--text-secondary)' }}>
              <Mic size={20} />
            </button>
            <button type="submit" className="btn btn-primary" style={{ borderRadius: '50%', width: '48px', height: '48px', padding: 0 }}>
              <Send size={18} />
            </button>
          </form>
        </div>
      </div>

      {/* User Profile Modal */}
      <Modal isOpen={showUserProfile} onClose={() => setShowUserProfile(false)} title="Edit Profile">
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div style={{ alignSelf: 'center', width: '80px', height: '80px', borderRadius: '50%', backgroundColor: 'var(--primary-color)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2rem', fontWeight: 'bold', cursor: 'pointer', position: 'relative' }}>
             {editProfile.username.charAt(0).toUpperCase()}
             <div style={{ position: 'absolute', bottom: 0, right: 0, background: 'rgba(0,0,0,0.6)', borderRadius: '50%', padding: '0.25rem' }}><Edit2 size={14} /></div>
          </div>
          <div>
            <label style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Username</label>
            <input type="text" className="input-field" value={editProfile.username} onChange={e => setEditProfile({...editProfile, username: e.target.value})} style={{ marginTop: '0.25rem' }} />
          </div>
          <div>
            <label style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Phone Number</label>
            <input type="tel" className="input-field" value={editProfile.phone} onChange={e => setEditProfile({...editProfile, phone: e.target.value})} style={{ marginTop: '0.25rem' }} />
          </div>
          <div>
            <label style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Date of Birth</label>
            <input type="date" className="input-field" value={editProfile.dob} onChange={e => setEditProfile({...editProfile, dob: e.target.value})} style={{ marginTop: '0.25rem' }} />
          </div>
          <button className="btn btn-primary" style={{ marginTop: '1rem' }} onClick={saveUserProfile}>Save Changes</button>
        </div>
      </Modal>

      {/* Group Profile Modal */}
      <Modal isOpen={showGroupProfile} onClose={() => setShowGroupProfile(false)} title="Group Details">
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div style={{ alignSelf: 'center', width: '80px', height: '80px', borderRadius: '50%', backgroundColor: '#475569', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2rem', fontWeight: 'bold', cursor: 'pointer', position: 'relative' }}>
             {groupDetails.name.charAt(0).toUpperCase()}
             <div style={{ position: 'absolute', bottom: 0, right: 0, background: 'rgba(0,0,0,0.6)', borderRadius: '50%', padding: '0.25rem' }}><Edit2 size={14} /></div>
          </div>
          <div>
            <label style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Group Name</label>
            <input type="text" className="input-field" value={groupDetails.name} onChange={e => setGroupDetails({...groupDetails, name: e.target.value})} style={{ marginTop: '0.25rem' }} />
          </div>
          <div>
            <label style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Description</label>
            <textarea className="input-field" value={groupDetails.description} onChange={e => setGroupDetails({...groupDetails, description: e.target.value})} style={{ marginTop: '0.25rem', resize: 'none', height: '80px' }} />
          </div>
          <button className="btn btn-primary" style={{ marginTop: '1rem' }} onClick={saveGroupProfile}>Save Changes</button>
        </div>
      </Modal>

    </div>
  );
};

export default ChatLayout;
