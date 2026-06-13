import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import API from "../api";

export default function Failed() {
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

    async function markFailed() {
      try {
        const res = await API.get(`/payments/${orderId}/failed`);
        if (!mounted) return;
        setStatus(res.data?.status ?? "Failed");
      } catch (err) {
        console.error("Failed to mark payment as failed", err);
        if (mounted) setStatus("Failed");
      } finally {
        if (mounted) setLoading(false);
        localStorage.removeItem("lastOrderId");
      }
    }

    markFailed();
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
        <div className="text-4xl font-bold text-red-600 mb-4">
          Payment Failed
        </div>
        <p className="text-gray-600 mb-6">
          Please check your details or try again.
        </p>
        <div className="flex justify-center gap-3">
          <Link
            to="/cart"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Try Again
          </Link>
          <Link to="/" className="inline-flex px-4 py-2 border rounded-lg">
            Back to Home
          </Link>
        </div>
      </div>
    </main>
  );
}
