// src/main.jsx  (ili src/index.jsx ako koristiš taj naziv)
import React from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import { AuthProvider } from "./components/AuthContext";
import "./index.css";

const merchantId = import.meta.env.VITE_MERCHANT_ID;
if (!merchantId) {
  throw new Error("VITE_MERCHANT_ID is missing");
}
sessionStorage.setItem("merchantId", merchantId);

const root = createRoot(document.getElementById("root"));

root.render(
  <React.StrictMode>
    {/* BrowserRouter mora biti iznad svega što koristi react-router hookove */}
    <BrowserRouter>
      {/* AuthProvider može biti unutar Router-a i tada može koristiti navigaciju u budućnosti */}
      <AuthProvider>
        <App />
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
