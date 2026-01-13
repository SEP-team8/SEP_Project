import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import API from "../api";

export default function Success() {
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(true);
  const location = useLocation();

  useEffect(() => {
    const orderId =
      localStorage.getItem("lastOrderId") ||
      new URLSearchParams(location.search).get("orderId");
    if (!orderId) {
      setLoading(false);
      return;
    }

    let mounted = true;

    async function markSuccess() {
      try {
        const res = await API.get(`/payments/${orderId}/success`);
        if (!mounted) return;
        setStatus(res.data?.status ?? "Success");
      } catch (err) {
        console.error("Failed to mark payment as success", err);
        if (mounted) setStatus("Success");
      } finally {
        if (mounted) setLoading(false);
        localStorage.removeItem("lastOrderId");
      }
    }

    markSuccess();
    return () => {
      mounted = false;
    };
  }, [location]);

  if (loading) {
    return (
      <main className="max-w-4xl mx-auto p-8">
        <div className="card text-center py-12">Loading payment status…</div>
      </main>
    );
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="card text-center py-12 bg-white rounded-2xl shadow-sm">
        <div className="text-4xl font-bold text-emerald-600 mb-4">
          Payment Successful
        </div>
        <p className="text-gray-600 mb-6">
          Thank you — your payment has been processed and the reservation is
          confirmed.
        </p>
        <div className="flex justify-center gap-3">
          <Link
            to="/vehicles"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            View Vehicles
          </Link>
          <Link to="/" className="inline-flex px-4 py-2 border rounded-lg">
            Home
          </Link>
        </div>
      </div>
    </main>
  );
}
