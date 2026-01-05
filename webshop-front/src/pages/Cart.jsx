import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function Cart() {
  const navigate = useNavigate();

  const [rawCart, setRawCart] = useState(() =>
    JSON.parse(sessionStorage.getItem("cart") || "[]")
  );
  const [cart, setCart] = useState([]);

  useEffect(() => {
    const onUpdate = () => {
      const arr = JSON.parse(sessionStorage.getItem("cart") || "[]");
      setRawCart(arr);
    };
    window.addEventListener("cartUpdated", onUpdate);
    return () => window.removeEventListener("cartUpdated", onUpdate);
  }, []);

  useEffect(() => {
    let mounted = true;

    async function enrich() {
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

    enrich();
    return () => (mounted = false);
  }, [rawCart]);

  const total = cart.reduce((s, c) => s + c.price * (c.days || 1), 0);

  function removeItem(index) {
    const copy = [...cart];
    copy.splice(index, 1);
    setCart(copy);

    const storageCopy = copy.map(({ name, ...rest }) => rest);
    sessionStorage.setItem("cart", JSON.stringify(storageCopy));
    window.dispatchEvent(new CustomEvent("cartUpdated"));
  }

  function removeAll() {
    setCart([]);
    setRawCart([]);
    sessionStorage.removeItem("cart");
    window.dispatchEvent(new CustomEvent("cartUpdated"));
  }

  async function pay() {
    if (cart.length === 0) return;

    const token = sessionStorage.getItem("token");
    if (!token) {
      alert("You must be logged in to continue.");
      navigate("/login?next=/cart");
      return;
    }

    try {
      const orderResp = await API.post("/orders", {
        items: cart.map((c) => ({
          vehicleId: c.vehicleId,
          days: c.days || 1,
        })),
      });

      const orderId = orderResp.data.orderId;
      if (!orderId) throw new Error("OrderId missing from backend");

      //STA SE SALJE PSP-U I MOZDA IZMENUTI I BACK STA PRIMA
      const payResp = await API.post("/payments/init", {
        MERCHANT_ORDER_ID: orderId,
        AMOUNT: total,
      });

      const paymentUrl = payResp.data?.paymentUrl;
      if (!paymentUrl) throw new Error("PaymentUrl from server is missing");

      window.location.href = paymentUrl;
    } catch (err) {
      console.error(err);
      alert("Payment initialization failed.");
    }
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-semibold">Cart</h2>

        {cart.length > 0 && (
          <button
            onClick={removeAll}
            className="text-sm text-red-600 hover:underline"
          >
            Remove all
          </button>
        )}
      </div>

      {cart.length === 0 ? (
        <div className="card text-center py-12">
          <p className="text-gray-600">Cart is empty.</p>
          <button
            onClick={() => navigate("/vehicles")}
            className="mt-4 inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Search vehicles
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="md:col-span-2 card">
            <ul className="space-y-4">
              {cart.map((c, i) => (
                <li key={i} className="flex items-center justify-between">
                  <div>
                    <div className="font-semibold text-sm">{c.name}</div>
                    <div className="text-xs text-gray-500">
                      {c.days || 1} day(s)
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
                      Remove
                    </button>
                  </div>
                </li>
              ))}
            </ul>

            <div className="mt-6 flex justify-between items-center">
              <div className="text-sm text-gray-600">
                Total items: {cart.length}
              </div>
              <div className="text-lg font-bold">{total.toFixed(2)} €</div>
            </div>
          </div>

          <aside className="card">
            <h3 className="font-semibold mb-3">Summary</h3>
            <div className="text-sm text-gray-600">Total amount:</div>
            <div className="text-2xl font-bold text-sky-700 mt-2">
              {total.toFixed(2)} €
            </div>
            <button
              onClick={pay}
              className="mt-6 w-full inline-flex justify-center px-4 py-2 bg-sky-700 text-white rounded-lg"
            >
              Pay
            </button>
          </aside>
        </div>
      )}
    </main>
  );
}
