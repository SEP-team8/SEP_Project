import { useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function Register() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const navigate = useNavigate();

  async function onSubmit(e) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!name || !email || !password) {
      setError("Popunite sva obavezna polja.");
      return;
    }
    if (password !== confirm) {
      setError("Lozinke se ne poklapaju.");
      return;
    }
    if (password.length < 6) {
      setError("Lozinka mora imati najmanje 6 karaktera.");
      return;
    }

    setLoading(true);
    try {
      const resp = await API.post("/auth/register", { name, email, password });
      if (resp.data?.token) {
        localStorage.setItem("token", resp.data.token);
        if (resp.data.user)
          localStorage.setItem("user", JSON.stringify(resp.data.user));
        navigate("/");
        return;
      }
      setSuccess("Registracija uspela. Možete se prijaviti.");
      setTimeout(() => navigate("/login"), 1200);
    } catch (err) {
      console.error(err);
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        "Registracija neuspešna.";
      setError(String(msg));
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="max-w-md mx-auto p-8">
      <div className="card">
        <h2 className="text-2xl font-semibold mb-4">Registracija</h2>
        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <label className="text-sm text-gray-700">Ime i prezime</label>
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
            <label className="text-sm text-gray-700">Lozinka</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full p-3 border rounded-md mt-1"
            />
          </div>

          <div>
            <label className="text-sm text-gray-700">Ponovite lozinku</label>
            <input
              type="password"
              value={confirm}
              onChange={(e) => setConfirm(e.target.value)}
              required
              className="w-full p-3 border rounded-md mt-1"
            />
          </div>

          {error && <div className="text-sm text-red-600">{error}</div>}
          {success && <div className="text-sm text-green-600">{success}</div>}

          <div className="flex gap-3">
            <button
              type="submit"
              disabled={loading}
              className={`inline-flex items-center px-4 py-2 rounded-lg text-white ${
                loading ? "bg-sky-300" : "bg-sky-700 hover:bg-sky-600"
              }`}
            >
              {loading ? "Registrujem..." : "Registruj se"}
            </button>

            <button
              type="button"
              onClick={() => navigate("/login")}
              disabled={loading}
              className="inline-flex items-center px-4 py-2 rounded-lg border"
            >
              Prijava
            </button>
          </div>
        </form>
      </div>
    </main>
  );
}
