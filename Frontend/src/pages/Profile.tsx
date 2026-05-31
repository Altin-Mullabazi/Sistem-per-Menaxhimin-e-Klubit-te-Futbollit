import React from 'react';
import { useAuth } from '../context/AuthContext';
import '../styles/ComingSoon.css';

const Profile: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="coming-soon-container">
      <div className="coming-soon-content">
        <h1>My Profile</h1>
        <p>Welcome, {user?.username}!</p>
        <p>Profile management coming soon...</p>
      </div>
    </div>
  );
};

export default Profile;
