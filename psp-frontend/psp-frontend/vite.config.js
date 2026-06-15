import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default ({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "")
  return defineConfig({
    plugins: [react()],
    server: {
      port: Number(env.VITE_PORT) || 5172,
      strictPort: true,
      proxy: {
        "/api": {
          target: env.VITE_PSP_BACKEND || "http://localhost:7150",
          changeOrigin: true,
          secure: false,
        },
      },
    },
  })
};