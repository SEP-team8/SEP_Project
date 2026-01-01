import { useState } from "react";
import API from "../api";
import { useNavigate } from "react-router-dom";

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
      setError("Email i lozinka su obavezni.");
      return;
    }

    setLoading(true);
    try {
      const resp = await API.post("/auth/login", { email, password });
      const { token, user } = resp.data || {};
      if (!token) {
        setError("Neispravan odgovor servera - nedostaje token.");
        setLoading(false);
        return;
      }
      localStorage.setItem("token", token);
      if (user) localStorage.setItem("user", JSON.stringify(user));
      navigate("/");
    } catch (err) {
      console.error(err);
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        "Prijava nije uspela. Proverite podatke.";
      setError(String(msg));
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="max-w-md mx-auto p-8">
      <div className="card">
        <h2 className="text-2xl font-semibold mb-4">Prijava</h2>
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
            <label className="text-sm text-gray-700">Lozinka</label>
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
              {loading ? "Prijava..." : "Prijavi se"}
            </button>

            <button
              type="button"
              onClick={() => navigate("/register")}
              disabled={loading}
              className="inline-flex items-center px-4 py-2 rounded-lg border"
            >
              Registracija
            </button>
          </div>
        </form>
      </div>
    </main>
  );
}
