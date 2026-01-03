import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function Checkout() {
  const navigate = useNavigate();

  const [method, setMethod] = useState("card"); // card | qr
  const [merchantId, setMerchantId] = useState("SHOP-123");
  const [loading, setLoading] = useState(false);

  // sirova korpa iz sessionStorage
  const rawCart = JSON.parse(sessionStorage.getItem("cart") || "[]");

  // obogaćena korpa (sa imenom vozila)
  const [cart, setCart] = useState([]);

  // učitaj imena vozila sa servera
  useEffect(() => {
    let mounted = true;

    async function enrichCart() {
      if (rawCart.length === 0) {
        setCart([]);
        return;
      }

      const uniqueIds = [...new Set(rawCart.map((c) => c.vehicleId))];
      const nameMap = {};

      await Promise.all(
        uniqueIds.map(async (id) => {
          try {
            const res = await API.get(`/vehicles/${id}`);
            const v = res.data;
            nameMap[id] = `${v.make} ${v.model}`;
          } catch {
            nameMap[id] = `Vehicle ${id}`;
          }
        })
      );

      const enriched = rawCart.map((c) => ({
        ...c,
        name: nameMap[c.vehicleId],
      }));

      if (mounted) setCart(enriched);
    }

    enrichCart();
    return () => (mounted = false);
  }, []);

  // ukupan iznos
  const amount = cart.reduce((sum, c) => sum + c.price * (c.days || 1), 0);

  async function startPayment() {
    if (cart.length === 0) {
      alert("The cart is empty.");
      return;
    }

    setLoading(true);
    try {
      const payload = {
        MERCHANT_ID: merchantId,
        AMOUNT: amount,
        CURRENCY: "EUR",
        MERCHANT_ORDER_ID: `ORDER-${Date.now()}`,
        MERCHANT_TIMESTAMP: new Date().toISOString(),
        SUCCESS_URL: `${window.location.origin}/success`,
        FAILED_URL: `${window.location.origin}/failed`,
        ERROR_URL: `${window.location.origin}/failed`,
        method, // card | qr
      };

      const resp = await API.post("/payments/init", payload);
      const data = resp.data;

      if (method === "card") {
        if (!data?.paymentUrl)
          throw new Error("PaymentUrl from server is missing");
        window.location.href = data.paymentUrl;
      } else {
        sessionStorage.setItem("qrData", JSON.stringify(data));
        navigate("/payment-redirect");
      }
    } catch (err) {
      console.error(err);
      alert("Error initializing payment");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* LEFT */}
        <div className="md:col-span-2 card p-6 bg-white rounded-2xl shadow-sm">
          <h2 className="text-xl font-semibold mb-4">Checkout</h2>

          <div className="mb-4">
            <label className="block text-sm text-gray-700 mb-2">
              Merchant ID
            </label>
            <input
              value={merchantId}
              onChange={(e) => setMerchantId(e.target.value)}
              className="w-full p-3 border rounded-md"
            />
          </div>

          <div className="mb-4">
            <div className="text-sm text-gray-600">Amount</div>
            <div className="text-2xl font-bold text-sky-700">
              {amount.toFixed(2)} €
            </div>
          </div>

          <div className="mb-4 flex gap-4">
            <label className="inline-flex items-center gap-2">
              <input
                type="radio"
                name="method"
                checked={method === "card"}
                onChange={() => setMethod("card")}
              />
              <span>Card</span>
            </label>
            <label className="inline-flex items-center gap-2">
              <input
                type="radio"
                name="method"
                checked={method === "qr"}
                onChange={() => setMethod("qr")}
              />
              <span>QR code</span>
            </label>
          </div>

          <div className="flex gap-3">
            <button
              onClick={startPayment}
              disabled={loading}
              className={`inline-flex items-center px-4 py-2 rounded-lg text-white ${
                loading ? "bg-sky-300" : "bg-sky-700 hover:bg-sky-600"
              }`}
            >
              {loading ? "Initialization..." : "Pay"}
            </button>

            <button
              onClick={() => navigate("/cart")}
              className="inline-flex items-center px-4 py-2 rounded-lg border"
            >
              Back
            </button>
          </div>
        </div>

        {/* RIGHT */}
        <aside className="card p-6 bg-white rounded-2xl shadow-sm">
          <h3 className="font-semibold mb-3">Cart view</h3>

          <ul className="space-y-3">
            {cart.map((c, i) => (
              <li key={i} className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium">{c.name}</div>
                  <div className="text-xs text-gray-500">
                    {c.days || 1} day(s)
                  </div>
                </div>
                <div className="font-semibold">
                  {(c.price * (c.days || 1)).toFixed(2)} €
                </div>
              </li>
            ))}
          </ul>

          <div className="mt-4 border-t pt-4 text-right">
            <div className="text-lg font-bold">{amount.toFixed(2)} €</div>
          </div>
        </aside>
      </div>
    </main>
  );
}
