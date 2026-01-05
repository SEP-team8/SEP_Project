import { useEffect, useState } from "react";
import API from "../api";
import OrderDetailsModal from "../components/OrderDetailsModal";

export default function OrdersHistory() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState(null);
  const [details, setDetails] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    API.get("/orders")
      .then((r) => {
        if (!mounted) return;
        setOrders(r.data || []);
      })
      .catch((err) => {
        console.error("Failed to load orders:", err);
      })
      .finally(() => mounted && setLoading(false));
    return () => (mounted = false);
  }, []);

  function openDetails(orderId) {
    setSelected(orderId);
    setDetailsLoading(true);
    API.get(`/orders/${encodeURIComponent(orderId)}`)
      .then((r) => setDetails(r.data))
      .catch((err) => {
        console.error("Failed to load order details:", err);
        setDetails(null);
      })
      .finally(() => setDetailsLoading(false));
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <h2 className="text-2xl font-semibold mb-4">Order history</h2>

      {loading ? (
        <div className="card p-6">Loading...</div>
      ) : orders.length === 0 ? (
        <div className="card p-6 text-gray-600">No orders.</div>
      ) : (
        <div className="space-y-3">
          {orders.map((o) => (
            <div
              key={o.orderId}
              className="card p-4 flex items-center justify-between"
            >
              <div>
                <div className="font-medium">{o.orderId}</div>
                <div className="text-sm text-gray-600">
                  {new Date(o.createdAt).toLocaleString()} — {o.currency}{" "}
                  {o.amount.toFixed(2)}
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="text-sm px-3 py-1 rounded-full bg-gray-100">
                  {o.status}
                </div>
                <button
                  onClick={() => openDetails(o.orderId)}
                  className="px-3 py-2 rounded-lg border"
                >
                  Details
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {selected && (
        <OrderDetailsModal
          orderId={selected}
          open={!!selected}
          loading={detailsLoading}
          data={details}
          onClose={() => {
            setSelected(null);
            setDetails(null);
          }}
        />
      )}
    </main>
  );
}
