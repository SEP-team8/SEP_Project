import { useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import API from "../api";

export default function Register() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const location = useLocation();

  const params = new URLSearchParams(location.search);
  const next = params.get("next") || "/";

  async function onSubmit(e) {
    e.preventDefault();
    setError("");

    if (!name || !email || !password) {
      setError("Fill in all required fields.");
      return;
    }
    if (password !== confirm) {
      setError("The passwords don't match.");
      return;
    }
    if (password.length < 6) {
      setError("Password must have at least 6 characters.");
      return;
    }

    setLoading(true);
    try {
      const resp = await API.post("/auth/register", { name, email, password });
      const token = resp?.data?.token || resp?.data?.data?.token;
      const user = resp?.data?.user || resp?.data?.data?.user;

      if (token) {
        await login({ token, user });
        const pendingRaw = sessionStorage.getItem("pendingCheckout");
        if (pendingRaw) {
          try {
            const pending = JSON.parse(pendingRaw);
            if (Array.isArray(pending.cart)) {
              sessionStorage.setItem("cart", JSON.stringify(pending.cart));
            }
          } catch {}
          sessionStorage.removeItem("pendingCheckout");
        }
        navigate("/");
        return;
      }

      setError("");
      alert("Registration successful. Please login.");
      navigate(`/login?next=${encodeURIComponent(next)}`);
    } catch (err) {
      console.error(err);
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        "Registration failed.";
      setError(String(msg));
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="max-w-md mx-auto p-8">
      <div className="card">
        <h2 className="text-2xl font-semibold mb-4">Register</h2>
        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <label className="text-sm text-gray-700">First and last name</label>
            <input
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              className="w-full p-3 border rounded-md mt-1"
            />
          </div>

          <div>
            <label className="text-sm text-gray-700">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full p-3 border rounded-md mt-1"
            />
          </div>

          <div>
            <label className="text-sm text-gray-700">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full p-3 border rounded-md mt-1"
            />
          </div>

          <div>
            <label className="text-sm text-gray-700">Repeat password</label>
            <input
              type="password"
              value={confirm}
              onChange={(e) => setConfirm(e.target.value)}
              required
              className="w-full p-3 border rounded-md mt-1"
            />
          </div>

          {error && <div className="text-sm text-red-600">{error}</div>}

          <div className="flex gap-3">
            <button
              type="submit"
              disabled={loading}
              className={`inline-flex items-center px-4 py-2 rounded-lg text-white ${
                loading ? "bg-sky-300" : "bg-sky-700 hover:bg-sky-600"
              }`}
            >
              {loading ? "Registering..." : "Register"}
            </button>

            <button
              type="button"
              onClick={() => navigate(`/login${location.search}`)}
              disabled={loading}
              className="inline-flex items-center px-4 py-2 rounded-lg border"
            >
              Login
            </button>
          </div>
        </form>
      </div>
    </main>
  );
}
