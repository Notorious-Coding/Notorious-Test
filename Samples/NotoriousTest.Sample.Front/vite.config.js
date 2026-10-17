import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The API url is given by the test environment (see FrontInfrastructure in NotoriousTest.Sample.Playwright).
// Calls to /api/* are proxied to the API, so the browser never needs CORS.
const apiUrl = process.env.API_URL ?? 'http://localhost:5000'

export default defineConfig({
  plugins: [react()],
  server: {
    port: Number(process.env.PORT ?? 5173),
    strictPort: true,
    proxy: {
      '/api': {
        target: apiUrl,
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api/, '')
      }
    }
  }
})
