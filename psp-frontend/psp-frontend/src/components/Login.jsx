import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Login.css";
import logo from "../assets/psp_logo.png";
import safeIcon from "../assets/shield.png";

export default function LoginPage() {
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  function handleSubmit(e) {
    e.preventDefault();
    setError("");

    if (username.trim() === "admin" && password === "admin") {
      // fejk login OK -> idi na admin stranicu
      navigate("/paymentMethods");
      return;
    }

    setError("Pogrešan username ili password. (Probaj admin / admin)");
  }

  return (
    <div className="login-page">
      <header className="login-header">
        <div className="login-brand">
          <img src={logo} alt="PSP logo" className="login-logo" />
          <div className="login-title">
            <div className="login-name">PSP Admin</div>
            <div className="login-sub">Login panel</div>
          </div>
        </div>

        <div className="login-safe">
          <img src={safeIcon} alt="Safe" className="safe-icon" />
          <p>
            Super admin panel • <strong>Manage merchants</strong>
          </p>
        </div>
      </header>

      <main className="login-center">
        <section className="login-card">
          <h2>Login</h2>
          <p className="login-hint">Unesi username i password.</p>

          <form className="login-form" onSubmit={handleSubmit}>
            <div className="form-row">
              <label className="form-label">Username</label>
              <input
                className="form-input"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
            </div>

            <div className="form-row">
              <label className="form-label">Password</label>
              <input
                className="form-input"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>

            <div className="form-actions">
              <button className="btn-primary" type="submit">
                Login
              </button>
            </div>

            {!!error && <div className="form-error">{error}</div>}
          </form>
        </section>
      </main>
    </div>
  );
}
