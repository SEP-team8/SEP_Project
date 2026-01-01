import { useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function Checkout() {
  const [method, setMethod] = useState("card"); // card | qr
  const [merchantId, setMerchantId] = useState("SHOP-123");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const cart = JSON.parse(localStorage.getItem("cart") || "[]");
  const amount = cart.reduce((s, c) => s + c.price * (c.days || 1), 0);

  async function startPayment() {
    if (cart.length === 0) {
      alert("Korpa je prazna.");
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
          throw new Error("Nedostaje paymentUrl od servera");
        window.location.href = data.paymentUrl;
      } else {
        sessionStorage.setItem("qrData", JSON.stringify(data));
        navigate("/payment-redirect");
      }
    } catch (err) {
      console.error(err);
      alert("Greška pri inicijalizaciji plaćanja");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
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
              <span>Kartica</span>
            </label>
            <label className="inline-flex items-center gap-2">
              <input
                type="radio"
                name="method"
                checked={method === "qr"}
                onChange={() => setMethod("qr")}
              />
              <span>QR kod</span>
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
              {loading ? "Inicijalizacija..." : "Plati"}
            </button>

            <button
              onClick={() => navigate("/cart")}
              className="inline-flex items-center px-4 py-2 rounded-lg border"
            >
              Nazad
            </button>
          </div>
        </div>

        <aside className="card p-6 bg-white rounded-2xl shadow-sm">
          <h3 className="font-semibold mb-3">Pregled korpe</h3>
          <ul className="space-y-3">
            {cart.map((c, i) => (
              <li key={i} className="flex items-center justify-between">
                <div>
                  <div className="text-sm">Vehicle {c.vehicleId}</div>
                  <div className="text-xs text-gray-500">
                    {c.days || 1} dan(a)
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
