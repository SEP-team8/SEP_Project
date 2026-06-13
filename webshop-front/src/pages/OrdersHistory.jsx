import { useEffect, useState } from "react";
import API from "../api";
import OrderDetailsModal from "../components/OrderDetailsModal";

const STATUS_MAP = {
  Initialized: "Initialized",
  Pending: "Pending",
  Authorized: "Authorized",
  Success: "Success",
  Failed: "Failed",
  Expired: "Expired",
  Cancelled: "Cancelled",
  Refunded: "Refunded",

  Initialized_display: "Initialized",
  Pending_display: "Pending (awaiting payment)",
  Authorized_display: "Authorized",
  Success_display: "Payment successful",
  Failed_display: "Payment failed",
  Expired_display: "Expired",
  Cancelled_display: "Cancelled",
  Refunded_display: "Refunded",
};

const STATUS_BY_INT = {
  0: "Initialized",
  1: "Pending",
  2: "Authorized",
  3: "Success",
  4: "Failed",
  5: "Expired",
  6: "Cancelled",
  7: "Refunded",
};

function formatStatus(raw) {
  if (raw === null || raw === undefined) return "Unknown";
  const maybeInt = Number(raw);
  if (!Number.isNaN(maybeInt) && String(raw).trim() !== "") {
    const s = STATUS_BY_INT[maybeInt];
    return s ? s : `Status(${raw})`;
  }
  const str = String(raw);
  if (/^\d+$/.test(str)) {
    const s = STATUS_BY_INT[parseInt(str, 10)];
    return s ? s : `Status(${str})`;
  }
  const normalized = str.trim();
  if (STATUS_MAP[normalized + "_display"])
    return STATUS_MAP[normalized + "_display"];
  if (STATUS_MAP[normalized]) return STATUS_MAP[normalized];
  return normalized;
}

function canonicalStatus(raw) {
  if (raw === null || raw === undefined) return "Unknown";
  const maybeInt = Number(raw);
  if (!Number.isNaN(maybeInt) && String(raw).trim() !== "") {
    const s = STATUS_BY_INT[maybeInt];
    return s ? s : String(raw);
  }
  return String(raw).trim();
}

export default function OrdersHistory() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState(null);
  const [details, setDetails] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);

  const [autoCancelRunning, setAutoCancelRunning] = useState(false);
  const [autoCancelResult, setAutoCancelResult] = useState(null);

  useEffect(() => {
    let mounted = true;
    setLoading(true);

    async function loadOrdersAndMaybeAutoCancel() {
      try {
        const res = await API.get("/orders");
        if (!mounted) return;
        const fetched = res.data || [];
        setOrders(fetched);
      } catch (err) {
        console.error("Failed to load orders:", err);
      } finally {
        if (!mounted) return;
        setLoading(false);
      }

      try {
        setAutoCancelRunning(true);
        setAutoCancelResult(null);
        const cancelResp = await API.post("/payments/auto-cancel-pending");
        if (!mounted) return;
        setAutoCancelResult(cancelResp.data || { cancelled: 0 });
        if (cancelResp.data && cancelResp.data.cancelled > 0) {
          setLoading(true);
          try {
            const refreshed = await API.get("/orders");
            if (mounted) setOrders(refreshed.data || []);
          } catch (err) {
            console.error("Failed to refresh orders after auto-cancel:", err);
          } finally {
            if (mounted) setLoading(false);
          }
        }
      } catch (err) {
        // Could be 401 if not authenticated or other error — surface to user lightly
        console.warn("Auto-cancel request failed or not authorized:", err);
        if (mounted)
          setAutoCancelResult({
            error: true,
            message: err?.message || String(err),
          });
      } finally {
        if (mounted) setAutoCancelRunning(false);
      }
    }

    loadOrdersAndMaybeAutoCancel();

    return () => {
      mounted = false;
    };
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

  function minutesSince(updatedAt) {
    try {
      const ms = Date.now() - new Date(updatedAt).getTime();
      return Math.floor(ms / 60000);
    } catch {
      return null;
    }
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <h2 className="text-2xl font-semibold mb-4">Order history</h2>

      {/* Auto-cancel feedback */}
      {autoCancelRunning && (
        <div className="card p-3 mb-4 bg-yellow-50 text-sm">
          Checking pending orders and attempting auto-cancel if older than 15
          minutes...
        </div>
      )}
      {autoCancelResult && autoCancelResult.error && (
        <div className="card p-3 mb-4 bg-red-50 text-sm text-red-700">
          Auto-cancel attempt failed:{" "}
          {autoCancelResult.message || "unknown error"}
        </div>
      )}
      {autoCancelResult &&
        !autoCancelResult.error &&
        autoCancelResult.cancelled > 0 && (
          <div className="card p-3 mb-4 bg-green-50 text-sm text-green-700">
            Auto-cancelled {autoCancelResult.cancelled} order(s).
            {autoCancelResult.cancelledIds &&
              autoCancelResult.cancelledIds.length > 0 && (
                <div className="text-xs text-gray-600 mt-1">
                  Cancelled IDs: {autoCancelResult.cancelledIds.join(", ")}
                </div>
              )}
          </div>
        )}

      {loading ? (
        <div className="card p-6">Loading...</div>
      ) : orders.length === 0 ? (
        <div className="card p-6 text-gray-600">No orders.</div>
      ) : (
        <div className="space-y-3">
          {orders.map((o) => {
            const canon = canonicalStatus(o.status);
            const mins = o.updatedAt ? minutesSince(o.updatedAt) : null;
            const willAutoCancelIn =
              mins !== null ? Math.max(0, 15 - mins) : null;
            const expiredForCancel =
              mins !== null && mins >= 15 && canon === "Pending";

            return (
              <div
                key={o.orderId}
                className="card p-4 flex items-center justify-between"
              >
                <div>
                  <div className="font-medium">{o.orderId}</div>
                  <div className="text-sm text-gray-600">
                    {new Date(o.createdAt).toLocaleString()} — {o.currency}{" "}
                    {Number(o.amount).toFixed(2)}
                    {o.updatedAt && (
                      <div className="text-xs text-gray-500 mt-1">
                        Updated: {new Date(o.updatedAt).toLocaleString()}{" "}
                        {mins !== null && <span>({mins} min ago)</span>}
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <div className="text-sm px-3 py-1 rounded-full bg-gray-100">
                    {formatStatus(o.status)}
                  </div>

                  {canon === "Pending" && mins !== null && (
                    <div className="text-xs text-gray-500">
                      {expiredForCancel ? (
                        <span className="text-red-600">
                          Pending &gt;= 15 min — will be auto-cancelled
                        </span>
                      ) : (
                        <span>
                          Pending — {mins} min passed (auto-cancel in{" "}
                          {willAutoCancelIn} min)
                        </span>
                      )}
                    </div>
                  )}

                  <button
                    onClick={() => openDetails(o.orderId)}
                    className="px-3 py-2 rounded-lg border"
                  >
                    Details
                  </button>
                </div>
              </div>
            );
          })}
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
