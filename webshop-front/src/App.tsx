import React, { useEffect, useState } from "react";
import Vehicles from "./pages/Vehicles";
import Checkout from "./pages/Checkout";
import Orders from "./pages/Orders";

export default function App() {
  const [page, setPage] = useState<"vehicles" | "checkout" | "orders">(
    "vehicles"
  );
  const [cart, setCart] = useState<any[]>([]);
  const [user] = useState({ id: "demo-user", name: "Demo User" });

  useEffect(() => {
    const qp = new URLSearchParams(window.location.search);
    if (qp.get("orderId")) setPage("orders");
  }, []);

  return (
    <div className="max-w-3xl mx-auto p-6">
      <header className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-semibold">RentACar - Demo</h1>
        <nav>
          <button onClick={() => setPage("vehicles")} className="mr-2">
            Vehicles
          </button>
          <button onClick={() => setPage("checkout")} className="mr-2">
            Checkout ({cart.length})
          </button>
          <button onClick={() => setPage("orders")}>Orders</button>
        </nav>
      </header>

      {page === "vehicles" && (
        <Vehicles onAdd={(v) => setCart((c) => [...c, v])} />
      )}
      {page === "checkout" && (
        <Checkout
          cart={cart}
          user={user}
          onPlaced={() => {
            setCart([]);
            setPage("orders");
          }}
        />
      )}
      {page === "orders" && <Orders />}
    </div>
  );
}
