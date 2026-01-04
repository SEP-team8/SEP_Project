import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function Checkout() {
  const navigate = useNavigate();

  const [method, setMethod] = useState("card"); // card | qr
  const [merchantId, setMerchantId] = useState("SHOP-123");
  const [loading, setLoading] = useState(false);

  const rawCartFromStorage = () =>
    JSON.parse(sessionStorage.getItem("cart") || "[]");

  const [cart, setCart] = useState([]);

  const PENDING_KEY = "pendingCheckout";

  const isLoggedIn = !!sessionStorage.getItem("token");

  useEffect(() => {
    const pendingRaw = sessionStorage.getItem(PENDING_KEY);
    if (pendingRaw) {
      try {
        const pending = JSON.parse(pendingRaw);
        if (isLoggedIn) {
          if (pending.cart && Array.isArray(pending.cart)) {
            sessionStorage.setItem("cart", JSON.stringify(pending.cart));
          }
          if (pending.merchantId) setMerchantId(pending.merchantId);
          if (pending.method) setMethod(pending.method);
          sessionStorage.removeItem(PENDING_KEY);
        } else {
          if (pending.merchantId) setMerchantId(pending.merchantId);
          if (pending.method) setMethod(pending.method);
        }
      } catch {}
    }
  }, []);

  useEffect(() => {
    let mounted = true;

    async function enrichCart() {
      const rawCart = rawCartFromStorage();
      if (!rawCart || rawCart.length === 0) {
        if (mounted) setCart([]);
        return;
      }

      const needFetchIds = rawCart
        .filter((c) => !c.name)
        .map((c) => c.vehicleId);
      const uniqIds = [...new Set(needFetchIds)];
      const nameMap = {};

      try {
        await Promise.all(
          uniqIds.map(async (id) => {
            try {
              const res = await API.get(`/vehicles/${id}`);
              const v = res.data;
              nameMap[id] = v ? `${v.make} ${v.model}` : `Vehicle ${id}`;
            } catch {
              nameMap[id] = `Vehicle ${id}`;
            }
          })
        );
      } catch (err) {
        console.error("Failed to fetch vehicle names", err);
      }

      const enriched = rawCart.map((c) => ({
        ...c,
        name: c.name ?? nameMap[c.vehicleId] ?? `Vehicle ${c.vehicleId}`,
      }));

      if (mounted) setCart(enriched);
    }

    enrichCart();

    function onUpdate() {
      enrichCart();
    }
    window.addEventListener("cartUpdated", onUpdate);

    return () => {
      mounted = false;
      window.removeEventListener("cartUpdated", onUpdate);
    };
  }, []);

  const amount = cart.reduce((sum, c) => sum + c.price * (c.days || 1), 0);

  function makeMerchantOrderId() {
    return `ORDER-${Date.now()}`;
  }

  async function startPayment() {
    if (cart.length === 0) {
      alert("The cart is empty.");
      return;
    }

    const token = sessionStorage.getItem("token");
    if (!token) {
      const currentCart = JSON.parse(sessionStorage.getItem("cart") || "[]");
      const pending = {
        cart: currentCart,
        merchantId,
        method,
        createdAt: new Date().toISOString(),
      };
      sessionStorage.setItem(PENDING_KEY, JSON.stringify(pending));
      alert(
        "You need to be logged in to proceed with payment. Your cart has been saved — please login or register and return to checkout."
      );
      navigate("/login?next=/checkout");
      return;
    }

    setLoading(true);
    try {
      const MERCHANT_ORDER_ID = makeMerchantOrderId();
      const MERCHANT_TIMESTAMP = new Date().toISOString();
      const SUCCESS_URL = `${window.location.origin}/payment-result`;
      const FAILED_URL = `${window.location.origin}/payment-result`;
      const ERROR_URL = `${window.location.origin}/payment-result`;

      const payload = {
        MERCHANT_ID: merchantId,
        AMOUNT: amount,
        CURRENCY: "EUR",
        MERCHANT_ORDER_ID,
        MERCHANT_TIMESTAMP,
        SUCCESS_URL,
        FAILED_URL,
        ERROR_URL,
        Method: method,
      };

      const resp = await API.post("/payments/init", payload);
      const data = resp.data;

      const pending = {
        merchantOrderId: MERCHANT_ORDER_ID,
        paymentId: data?.paymentId ?? null,
        cart: rawCartFromStorage(),
        merchantId,
        method,
        createdAt: new Date().toISOString(),
      };
      sessionStorage.setItem(PENDING_KEY, JSON.stringify(pending));

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

  const pendingRaw = sessionStorage.getItem(PENDING_KEY);
  const hasPending = !!pendingRaw && !isLoggedIn;

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2 card p-6 bg-white rounded-2xl shadow-sm">
          <h2 className="text-xl font-semibold mb-4">Checkout</h2>
          {hasPending && (
            <div className="mb-4 p-3 bg-yellow-50 border border-yellow-100 text-yellow-900 rounded">
              You are not logged in. If you continue you will be redirected to
              login and your cart will be preserved. After login, return to this
              page to finish checkout.
            </div>
          )}

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
