import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./AddPaymentMethods.css";

import logo from "../assets/psp_logo.png";
import safeIcon from "../assets/shield.png";

// FEJK PODACI (tabela: postojeće metode plaćanja u PSP sistemu)
const FAKE_PAYMENT_METHODS = [
  { id: "pm1", name: "Card", description: "Redirect na banku za unos kartice" },
  { id: "pm2", name: "QrCode", description: "IPS / QR instant plaćanje" },
  { id: "pm3", name: "PayPal", description: "PayPal checkout" },
  { id: "pm4", name: "Crypto", description: "Plaćanje kriptovalutama" },
];

function normalizeName(name) {
  return (name ?? "").trim();
}

export default function AddPaymentMethods() {
  const navigate = useNavigate();

  // forma
  const [name, setName] = useState("");
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  // tabela
  const [methods, setMethods] = useState(FAKE_PAYMENT_METHODS);

  const totalCount = useMemo(() => methods.length, [methods]);

  function handleLogout() {
    navigate("/login");
  }

  function handleBackToMerchantMethods() {
    navigate("/paymentMethods");
  }

  function handleAdd(e) {
    e.preventDefault();
    setError("");
    setInfo("");

    const n = normalizeName(name);

    if (!n) {
      setError("Name je obavezan.");
      return;
    }

    // zabrani duplikate (case-insensitive)
    const exists = methods.some((m) => m.name.toLowerCase() === n.toLowerCase());
    if (exists) {
      setError("Ova payment metoda već postoji.");
      return;
    }

    // FEJK ADD (u realnom slučaju zamenjuješ fetch pozivom ka backend-u)
    const newItem = {
      id: crypto.randomUUID(),
      name: n,
      description: "New payment method (not configured yet)",
    };

    setMethods((prev) => [newItem, ...prev]);
    setName("");
    setInfo("Uspešno dodato (fejk).");
  }

  function handleDelete(id) {
    setError("");
    setInfo("");

    setMethods((prev) => prev.filter((m) => m.id !== id));
    setInfo("Obrisano (fejk).");
  }

  return (
    <div className="apm-page">
      <header className="apm-header">
        <div className="apm-brand">
          <img src={logo} alt="PSP logo" className="apm-logo" />
          <div className="apm-title">
            <div className="apm-name">PSP Admin</div>
            <div className="apm-sub">Add payment methods</div>
          </div>
        </div>

        <div className="apm-right">
          <div className="apm-safe">
            <img src={safeIcon} alt="Safe" className="safe-icon" />
            <p>
              System-level settings • <strong>Payment methods</strong>
            </p>
          </div>

          <div className="apm-actions">
            <button className="btn-secondary" onClick={handleBackToMerchantMethods}>
              Back
            </button>
            <button className="btn-secondary apm-logout" onClick={handleLogout}>
              Logout
            </button>
          </div>
        </div>
      </header>

      <main className="apm-container">
        {/* ADD CARD */}
        <section className="apm-card">
          <h2>Add new payment method</h2>
          <p className="apm-hint">
            Unesi <strong>Name</strong> i klikni <strong>Add</strong>.
          </p>

          <form className="apm-form" onSubmit={handleAdd}>
            <div className="form-row">
              <label className="form-label">Name</label>
              <input
                className="form-input"
                placeholder="npr. ApplePay"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </div>

            <div className="form-actions">
              <button className="btn-primary" type="submit">
                Add
              </button>
            </div>

            {!!error && <div className="form-error">{error}</div>}
            {!!info && <div className="form-info">{info}</div>}
          </form>
        </section>

        {/* TABLE CARD */}
        <section className="apm-card">
          <div className="table-header">
            <h2>Existing payment methods</h2>
            <div className="table-meta">
              Total: <strong>{totalCount}</strong>
            </div>
          </div>

          <div className="table-wrap">
            <table className="apm-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th className="col-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {methods.map((m) => (
                  <tr key={m.id}>
                    <td className="mono">{m.name}</td>
                    <td className="muted">{m.description}</td>
                    <td className="col-actions">
                      <button className="btn-danger btn-sm" onClick={() => handleDelete(m.id)}>
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}

                {methods.length === 0 && (
                  <tr>
                    <td colSpan={3} className="empty">
                      Nema podataka.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <p className="apm-note muted">
            Napomena: Mora da postoji barem jedna payment metoda konfigurisana.
          </p>
        </section>
      </main>
    </div>
  );
}
