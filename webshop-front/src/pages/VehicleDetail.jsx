import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import API from "../api";

export default function VehicleDetail() {
  const { id } = useParams();
  const [v, setV] = useState(null);
  const [days, setDays] = useState(1);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    API.get(`/vehicles/${id}`)
      .then((r) => {
        if (mounted) setV(r.data);
      })
      .catch((err) => {
        console.error(err);
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => (mounted = false);
  }, [id]);

  function addToCart() {
    if (!v) return;
    const cart = JSON.parse(localStorage.getItem("cart") || "[]");
    cart.push({
      vehicleId: v.id,
      price: v.price,
      days: Math.max(1, Number(days)),
    });
    localStorage.setItem("cart", JSON.stringify(cart));
    window.dispatchEvent(new CustomEvent("cartUpdated"));
    navigate("/cart");
  }

  if (loading) {
    return (
      <main className="max-w-4xl mx-auto p-8">
        <div className="card">
          <div className="text-gray-600">Učitavanje vozila...</div>
        </div>
      </main>
    );
  }

  if (!v) {
    return (
      <main className="max-w-4xl mx-auto p-8">
        <div className="card text-center">
          <div className="text-gray-600">Vozilo nije pronađeno.</div>
        </div>
      </main>
    );
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2 card">
          <img
            src={v.image || "/placeholder.png"}
            alt={`${v.make} ${v.model}`}
            className="w-full h-80 object-cover rounded-md"
          />
          <div className="mt-4">
            <h2 className="text-2xl font-bold">
              {v.make} {v.model}
            </h2>
            <p className="text-gray-600 mt-2">{v.description}</p>
          </div>
        </div>

        <aside className="card flex flex-col gap-4">
          <div>
            <div className="text-sm text-gray-500">Cena</div>
            <div className="text-2xl font-bold text-sky-700">
              {v.price} € / dan
            </div>
          </div>

          <div>
            <label className="block text-sm text-gray-600 mb-1">Dani</label>
            <input
              type="number"
              min="1"
              value={days}
              onChange={(e) => setDays(e.target.value)}
              className="w-full p-3 border rounded-md"
            />
          </div>

          <button
            onClick={addToCart}
            className="w-full inline-flex justify-center px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Rezerviši i dodaj u korpu
          </button>

          <button
            onClick={() => navigate(-1)}
            className="w-full inline-flex justify-center px-4 py-2 border rounded-lg"
          >
            Nazad
          </button>
        </aside>
      </div>
    </main>
  );
}
