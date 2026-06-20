import Hero from '../components/Hero';
import { useGetStatusQuery } from '../api/generatedApi';

const Home = () => {
  const { data: status, isLoading, isError } = useGetStatusQuery();

  return (
    <>
      <Hero />
      <section className="container" id="status" style={{ paddingTop: '80px' }}>
        <div className="glass" style={{ padding: '32px', maxWidth: '480px', margin: '0 auto' }}>
          <h2 style={{ marginBottom: '16px', fontSize: '1.25rem' }}>API Status</h2>
          {isLoading && <p style={{ color: 'var(--text-secondary)' }}>Connecting...</p>}
          {isError && <p style={{ color: '#f87171' }}>Could not reach the API. Is the backend running?</p>}
          {status !== undefined && (
            <pre style={{
              color: 'var(--text-secondary)',
              fontSize: '0.85rem',
              whiteSpace: 'pre-wrap',
              fontFamily: 'monospace'
            }}>
              {JSON.stringify(status, null, 2)}
            </pre>
          )}
        </div>
      </section>
    </>
  );
};

export default Home;
