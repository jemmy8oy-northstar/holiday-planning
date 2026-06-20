import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import ThemeToggle from './ThemeToggle';
import config from '../data/config.json';

const Navbar: React.FC = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const location = useLocation();

  const getLinkStyle = (path: string) => {
    const active = location.pathname === path;
    return {
      color: active ? 'var(--accent-primary)' : 'var(--text-secondary)',
      fontWeight: active ? 700 : 500,
      transition: 'all 0.3s ease',
      position: 'relative' as const,
      display: 'flex',
      flexDirection: 'column' as const,
      alignItems: 'center'
    };
  };

  const isActive = (path: string) => location.pathname === path;

  const ActiveDot = () => (
    <div style={{
      position: 'absolute',
      bottom: '-8px',
      width: '4px',
      height: '4px',
      borderRadius: '50%',
      background: 'var(--accent-primary)',
      boxShadow: '0 0 8px var(--accent-primary)'
    }} />
  );

  return (
    <nav className="glass" style={{
      position: 'fixed',
      top: '24px',
      left: '50%',
      transform: 'translateX(-50%)',
      width: 'max-content',
      padding: '12px 32px',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 1000,
      backdropFilter: 'blur(20px)',
      background: 'var(--bg-card)',
      borderColor: 'var(--glass-border)',
      flexDirection: 'column' as const
    }}>
      <div style={{ display: 'flex', width: '100%', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ fontWeight: 800, fontSize: '1.2rem', color: 'var(--text-primary)', display: 'none' }} className="mobile-logo">App Name</div>
        <button
          onClick={() => setIsMenuOpen(!isMenuOpen)}
          style={{ display: 'none', color: 'var(--text-primary)', fontSize: '1.5rem', padding: '4px' }}
          className="mobile-toggle"
        >
          {isMenuOpen ? '✕' : '☰'}
        </button>
      </div>

      <div className={`nav-links ${isMenuOpen ? 'open' : ''}`} style={{ display: 'flex', gap: '24px', fontSize: '0.85rem', alignItems: 'center' }}>
        <Link to={config.navigation.home} onClick={() => setIsMenuOpen(false)} style={getLinkStyle(config.navigation.home)}>
          Home
          {isActive(config.navigation.home) && <ActiveDot />}
        </Link>
        {/* Add more nav links here as your project grows */}
        <ThemeToggle />
      </div>
    </nav>
  );
};

export default Navbar;
