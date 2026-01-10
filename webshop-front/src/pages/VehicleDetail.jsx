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
      .catch((err) => console.error(err))
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => (mounted = false);
  }, [id]);

  function addToCart() {
    if (!v) return;
    const cart = JSON.parse(sessionStorage.getItem("cart") || "[]");
    cart.push({
      vehicleId: v.id,
      price: v.price,
      days: Math.max(1, Number(days)),
    });
    sessionStorage.setItem("cart", JSON.stringify(cart));
    window.dispatchEvent(new CustomEvent("cartUpdated"));
    navigate("/cart");
  }

  if (loading)
    return (
      <main className="max-w-4xl mx-auto p-8">
        <div className="card p-6 bg-white rounded-2xl shadow-sm">
          <div className="text-gray-600">Loading...</div>
        </div>
      </main>
    );

  if (!v)
    return (
      <main className="max-w-4xl mx-auto p-8">
        <div className="card p-6 text-center bg-white rounded-2xl shadow-sm">
          <div className="text-gray-600">Vehicle not found.</div>
        </div>
      </main>
    );

  return (
    <main className="max-w-5xl mx-auto p-8">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2 card bg-white rounded-2xl shadow-sm p-4">
          <img
            src={
              v.image ? `data:image/*;base64,${v.image}` : "/placeholder.png"
            }
            alt={`${v.make} ${v.model}`}
            className="w-full h-full object-cover"
          />
          <div className="mt-4">
            <h2 className="text-2xl font-bold">
              {v.make} {v.model}
            </h2>
            <p className="text-gray-600 mt-2">{v.description}</p>
          </div>
        </div>

        <aside className="card bg-white rounded-2xl shadow-sm p-4 flex flex-col gap-4">
          <div>
            <div className="text-sm text-gray-500">Price</div>
            <div className="text-2xl font-bold text-sky-700">
              {v.price} RSD / day
            </div>
          </div>

          <div>
            <label className="block text-sm text-gray-600 mb-1">Days</label>
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
            className="w-full inline-flex justify-center px-4 py-2 bg-sky-700 text-white rounded-lg hover:bg-sky-600"
          >
            Add to cart
          </button>

          <button
            onClick={() => navigate(-1)}
            className="w-full inline-flex justify-center px-4 py-2 border rounded-lg hover:bg-gray-50"
          >
            Back
          </button>
        </aside>
      </div>
    </main>
  );
}
