import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5172,       // fiksan port
    strictPort: true, // ne prelazi na drugi port ako je zauzet
    proxy: {
      "/api": {
        target: "https://localhost:7150", //PSP backend
        changeOrigin: true,
        secure: false,
      },
    },
  },
});