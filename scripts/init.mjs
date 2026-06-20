import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import crypto from 'crypto';

const PROJECT_ROOT = process.cwd();
const BACKEND_DIR = path.join(PROJECT_ROOT, 'backend');
const FRONTEND_DIR = path.join(PROJECT_ROOT, 'frontend');

function log(msg) {
  console.log(`[INIT] ${msg}`);
}

async function run() {
  log('Starting Indie Dev Home initialization...');

  // 1. Generate GUIDs and Secrets
  const jwtSecret = crypto.randomBytes(32).toString('hex');
  const projectId = crypto.randomUUID();

  // 2. Setup Backend appsettings.Development.json
  const webApiProject = fs.readdirSync(BACKEND_DIR).find(d => d.endsWith('.WebApi'));
  if (webApiProject) {
    const appSettingsPath = path.join(BACKEND_DIR, webApiProject, 'appsettings.Development.json');
    const settings = {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=indie_dev_home;Username=postgres;Password=postgres"
      },
      "Jwt": {
        "Secret": jwtSecret
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      }
    };
    fs.writeFileSync(appSettingsPath, JSON.stringify(settings, null, 2));
    log(`Generated ${appSettingsPath}`);
  }

  // 3. Setup Frontend .env
  const frontendEnvPath = path.join(FRONTEND_DIR, '.env');
  const frontendEnv = `VITE_API_URL=http://localhost:5000\nVITE_PROJECT_ID=${projectId}\n`;
  fs.writeFileSync(frontendEnvPath, frontendEnv);
  log(`Generated ${frontendEnvPath}`);

  // 4. Install Dependencies
  log('Restoring .NET tools and dependencies...');
  try {
    execSync('dotnet restore', { cwd: BACKEND_DIR, stdio: 'inherit' });
    log('.NET Restore complete.');
  } catch (e) {
    log('Failed to restore .NET dependencies. Ensure dotnet is installed.');
  }

  log('Installing frontend dependencies...');
  try {
    execSync('npm install', { cwd: FRONTEND_DIR, stdio: 'inherit' });
    log('Frontend installation complete.');
  } catch (e) {
    log('Failed to install frontend dependencies. Ensure node/npm is installed.');
  }

  log('Initialization complete! You can now run the app.');
}

run().catch(err => {
  console.error(err);
  process.exit(1);
});
