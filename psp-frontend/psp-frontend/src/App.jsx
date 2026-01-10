import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import PaymentPage from "./pages/PaymentPage.jsx";
import PaymentMethodsPage from "./components/PaymentMethods.jsx";

import './App.css'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/psp" replace />} />
        <Route path="/psp" element={<PaymentPage />} />
        <Route path="/pay" element={<PaymentPage />} />
        <Route path="/paymentMethods" element={<PaymentMethodsPage />} />
      </Routes>
    </BrowserRouter>
  );
}
