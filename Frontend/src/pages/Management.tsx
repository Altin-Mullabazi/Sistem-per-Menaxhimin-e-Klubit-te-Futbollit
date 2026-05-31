import React from 'react';
import TransfersTab from '../components/management/TransfersTab';
import ContractsTab from '../components/management/ContractsTab';
import InjuriesTab from '../components/management/InjuriesTab';
import '../styles/Management.css';

const Management: React.FC = () => {
  const [activeTab, setActiveTab] = React.useState<'transfers' | 'contracts' | 'injuries'>('transfers');

  return (
    <div className="management-page">
      <div className="management-header">
        <h1>Management</h1>
        <div className="tabs">
          <button className={`tab ${activeTab === 'transfers' ? 'active' : ''}`} onClick={() => setActiveTab('transfers')}>Transfers</button>
          <button className={`tab ${activeTab === 'contracts' ? 'active' : ''}`} onClick={() => setActiveTab('contracts')}>Contracts</button>
          <button className={`tab ${activeTab === 'injuries' ? 'active' : ''}`} onClick={() => setActiveTab('injuries')}>Injuries</button>
        </div>
      </div>

      <div className="management-content">
        {activeTab === 'transfers' && <TransfersTab />}
        {activeTab === 'contracts' && <ContractsTab />}
        {activeTab === 'injuries' && <InjuriesTab />}
      </div>
    </div>
  );
};

export default Management;
