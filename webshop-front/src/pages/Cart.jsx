import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

export default function Cart() {
  const navigate = useNavigate();
  const [cart, setCart] = useState(() =>
    JSON.parse(localStorage.getItem("cart") || "[]")
  );

  useEffect(() => {
    const onUpdate = () =>
      setCart(JSON.parse(localStorage.getItem("cart") || "[]"));
    window.addEventListener("cartUpdated", onUpdate);
    return () => window.removeEventListener("cartUpdated", onUpdate);
  }, []);

  const total = cart.reduce((s, c) => s + c.price * (c.days || 1), 0);

  function removeItem(index) {
    const copy = [...cart];
    copy.splice(index, 1);
    setCart(copy);
    localStorage.setItem("cart", JSON.stringify(copy));
    window.dispatchEvent(new CustomEvent("cartUpdated"));
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <h2 className="text-2xl font-semibold mb-4">Korpa</h2>
      {cart.length === 0 ? (
        <div className="card text-center py-12">
          <p className="text-gray-600">Korpa je prazna</p>
          <button
            onClick={() => navigate("/vehicles")}
            className="mt-4 inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Pretraži vozila
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="md:col-span-2 card">
            <ul className="space-y-4">
              {cart.map((c, i) => (
                <li key={i} className="flex items-center justify-between">
                  <div>
                    <div className="font-semibold text-sm">
                      Vehicle {c.vehicleId}
                    </div>
                    <div className="text-xs text-gray-500">
                      {c.days || 1} dan(a)
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="font-semibold">
                      {(c.price * (c.days || 1)).toFixed(2)} €
                    </div>
                    <button
                      onClick={() => removeItem(i)}
                      className="text-sm text-red-600 hover:underline"
                    >
                      Ukloni
                    </button>
                  </div>
                </li>
              ))}
            </ul>
            <div className="mt-6 flex justify-between items-center">
              <div className="text-sm text-gray-600">
                Ukupno artikala: {cart.length}
              </div>
              <div className="text-lg font-bold">{total.toFixed(2)} €</div>
            </div>
          </div>
          <aside className="card">
            <h3 className="font-semibold mb-3">Sažetak</h3>
            <div className="text-sm text-gray-600">Ukupan iznos:</div>
            <div className="text-2xl font-bold text-sky-700 mt-2">
              {total.toFixed(2)} €
            </div>
            <button
              onClick={() => navigate("/checkout")}
              className="mt-6 w-full inline-flex justify-center px-4 py-2 bg-sky-700 text-white rounded-lg"
            >
              Checkout
            </button>
          </aside>
        </div>
      )}
    </main>
  );
}
