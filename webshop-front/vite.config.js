import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default ({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return defineConfig({
    plugins: [react()],
    server: {
      host: env.VITE_HOST || "localhost",
      port: Number(env.VITE_PORT) || 5173,
      proxy: {
        "/api": {
          target: env.VITE_API_BASE || "http://localhost:7171",
          changeOrigin: true,
          secure: false,
        },
      },
    },
  });
};
