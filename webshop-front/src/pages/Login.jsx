import { useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  async function onSubmit(e) {
    e.preventDefault();
    setError("");

    if (!email || !password) {
      setError("Email and password are required.");
      return;
    }

    setLoading(true);
    try {
      const resp = await API.post("/auth/login", { email, password });
      // prilagodi u zavisnosti od response shape; ovde očekujemo { token, user }
      const token = resp?.data?.token;
      const user = resp?.data?.user || resp?.data?.data?.user;

      if (!token) {
        setError("Bad server response - missing token.");
        setLoading(false);
        return;
      }

      sessionStorage.setItem("token", token);
      if (user) sessionStorage.setItem("user", JSON.stringify(user));
      window.dispatchEvent(
        new CustomEvent("authChanged", { detail: { user } })
      );
      navigate("/");
    } catch (err) {
      console.error(err);
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        "Login failed. Please check your information.";
      setError(String(msg));
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="max-w-md mx-auto p-8">
      <div className="card">
        <h2 className="text-2xl font-semibold mb-4">Login</h2>
        <form onSubmit={onSubmit} className="space-y-4">
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

          {error && <div className="text-sm text-red-600">{error}</div>}

          <div className="flex gap-3">
            <button
              type="submit"
              disabled={loading}
              className={`inline-flex items-center px-4 py-2 rounded-lg text-white ${
                loading ? "bg-sky-300" : "bg-sky-700 hover:bg-sky-600"
              }`}
            >
              {loading ? "Login..." : "Sign up"}
            </button>

            <button
              type="button"
              onClick={() => navigate("/register")}
              disabled={loading}
              className="inline-flex items-center px-4 py-2 rounded-lg border"
            >
              Registration
            </button>
          </div>
        </form>
      </div>
    </main>
  );
}
