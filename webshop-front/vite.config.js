// vite.config.ts
import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default ({ mode }) => {
  // učitaj env var-ove za mode (bez prefiksa)
  const env = loadEnv(mode, process.cwd(), "");

  return defineConfig({
    plugins: [react()],
    server: {
      host: env.VITE_HOST || "localhost",
      port: Number(env.VITE_PORT) || 5173,
      // open: true // opcionalno
    },
  });
};
