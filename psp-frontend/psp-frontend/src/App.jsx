import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import PaymentPage from "./pages/PaymentPage.jsx";
import PaymentMethodsPage from "./components/PaymentMethods.jsx";
import LoginPage from "./components/Login.jsx";
import AddPaymentMethods from "./components/AddPaymentMethods.jsx";
import './App.css'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/psp" element={<PaymentPage />} />
        <Route path="/pay" element={<PaymentPage />} />
        <Route path="/paymentMethods" element={<PaymentMethodsPage />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/addPaymentMethods" element={<AddPaymentMethods />} />
      </Routes>
    </BrowserRouter>
  );
}
