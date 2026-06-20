import React from 'react';

const Hero: React.FC = () => {
    return (
        <section className="container" style={{
            textAlign: 'center',
            marginTop: '60px'
        }}>
            <h1 style={{
                fontSize: 'clamp(3.5rem, 8vw, 6rem)',
                marginBottom: '24px',
                background: 'linear-gradient(to bottom right, var(--text-primary) 30%, rgba(148, 163, 184, 0.5))',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                lineHeight: 1.1
            }}>
                Your App Name
            </h1>
            <p style={{
                fontSize: 'clamp(1.1rem, 2vw, 1.4rem)',
                color: 'var(--text-secondary)',
                maxWidth: '600px',
                margin: '0 auto 48px',
                fontWeight: 400
            }}>
                A brief description of what your app does.
            </p>
            <div className="hero-actions" style={{ display: 'flex', gap: '16px', justifyContent: 'center' }}>
                <a
                    href="#status"
                    className="glass"
                    style={{
                        padding: '16px 32px',
                        background: 'var(--accent-primary)',
                        color: '#fff',
                        fontWeight: 600,
                        borderRadius: '50px',
                        border: 'none',
                        textDecoration: 'none'
                    }}
                >
                    Get Started
                </a>
            </div>
        </section>
    );
};

export default Hero;
