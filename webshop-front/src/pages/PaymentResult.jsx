import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import API from "../api";

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

export default function PaymentResult() {
  const query = useQuery();
  const navigate = useNavigate();
  const [statusText, setStatusText] = useState("Processing...");
  const [done, setDone] = useState(false);

  useEffect(() => {
    async function handle() {
      const paymentId =
        query.get("paymentId") || query.get("paymentid") || null;
      const global =
        query.get("global") || query.get("globalTransactionId") || null;
      const status = query.get("status") || query.get("result") || "UNKNOWN";

      const raw = sessionStorage.getItem("pendingPayment");
      let pending = null;
      try {
        pending = raw ? JSON.parse(raw) : null;
      } catch {
        pending = null;
      }

      if (!pending || !pending.merchantOrderId) {
        setStatusText(
          `Nema podataka o porudžbini (merchantOrderId). Status: ${status}`
        );
        setDone(true);
        return;
      }

      try {
        setStatusText("Verifikujem plaćanje na serveru...");
        const orderId = encodeURIComponent(pending.merchantOrderId);
        const st = encodeURIComponent(status);
        const gt = encodeURIComponent(global || "");
        await API.post(
          `/payments/callback?orderId=${orderId}&status=${st}&globalTransactionId=${gt}`
        );
        setStatusText(`Plaćanje ažurirano: ${status}`);
        if (status === "OK" || status === "SUCCESS" || status === "OK") {
          sessionStorage.removeItem("cart");
          sessionStorage.removeItem("pendingPayment");
          sessionStorage.removeItem("qrData");
          window.dispatchEvent(new CustomEvent("cartUpdated"));
        } else {
        }
      } catch (err) {
        console.error("Callback error", err);
        setStatusText("Greška pri ažuriranju porudžbine na serveru.");
      } finally {
        setDone(true);
      }
    }

    handle();
  }, []);

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="card p-6 bg-white rounded-2xl shadow-sm">
        <h2 className="text-xl font-semibold mb-4">Payment result</h2>
        <div className="mb-4">{statusText}</div>

        {done && (
          <div className="flex gap-2">
            <button
              onClick={() => navigate("/")}
              className="px-3 py-2 bg-sky-700 text-white rounded"
            >
              Home
            </button>
            <button
              onClick={() => navigate("/orders")}
              className="px-3 py-2 border rounded"
            >
              My orders
            </button>
          </div>
        )}
      </div>
    </main>
  );
}
