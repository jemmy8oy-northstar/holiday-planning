import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css'
import Navbar from './components/Navbar'
import Home from './pages/Home'
import ScrollToTop from './components/ScrollToTop'
import { ThemeProvider } from './context/ThemeContext'

function App() {
  return (
    <ThemeProvider>
      <Router>
        <ScrollToTop />
        <div className="app-container">
          <Navbar />
          <main>
            <Routes>
              <Route path="/" element={<Home />} />
            </Routes>
          </main>
          <footer className="container" style={{
            padding: '64px 0',
            borderTop: '1px solid var(--glass-border)',
            marginTop: '120px',
            textAlign: 'center',
            color: 'var(--text-secondary)',
            fontSize: '0.9rem'
          }}>
            <p>© {new Date().getFullYear()} Your Name Here.</p>
          </footer>
        </div>
      </Router>
    </ThemeProvider>
  )
}

export default App
