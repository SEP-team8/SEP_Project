import { Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Vehicles from "./pages/Vehicles";
import VehicleDetail from "./pages/VehicleDetail";
import Cart from "./pages/Cart";
import Checkout from "./pages/Checkout";
import PaymentRedirect from "./pages/PaymentRedirect";
import Success from "./pages/Success";
import Failed from "./pages/Failed";
import Login from "./pages/Login";
import Register from "./pages/Register";
import NavBar from "./components/NavBar";
import Profile from "./pages/Profile";
import AdminVehicles from "./pages/AdminVehicles";
import AddUpdateVehicle from "./pages/AddUpdateVehicle";
import PaymentResult from "./pages/PaymentResult";

export default function App() {
  return (
    <>
      <NavBar />
      <main className="bg-gray-50 min-h-screen">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/vehicles" element={<Vehicles />} />
          <Route path="/vehicle/:id" element={<VehicleDetail />} />
          <Route path="/cart" element={<Cart />} />
          <Route path="/checkout" element={<Checkout />} />
          <Route path="/payment-redirect" element={<PaymentRedirect />} />
          <Route path="/payment-result" element={<PaymentResult />} />
          <Route path="/success" element={<Success />} />
          <Route path="/failed" element={<Failed />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/admin/vehicles" element={<AdminVehicles />} />
          <Route path="/admin/vehicle/:id?" element={<AddUpdateVehicle />} />
        </Routes>
      </main>
    </>
  );
}
