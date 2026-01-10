import { useEffect, useMemo, useState } from "react";
import "./PaymentMethods.css";
import logo from "../assets/psp_logo.png";
import safeIcon from "../assets/shield.png";

const PAYMENT_METHODS = [
  { value: "Card", label: "Card" },
  { value: "QrCode", label: "QrCode" },
  { value: "PayPal", label: "PayPal" },
  { value: "Crypto", label: "Crypto" },
];

function normalizeApiErrorText(txt) {
  if (!txt) return "Došlo je do greške.";
  return txt;
}

export default function PaymentMethodsPage() {
  // forma
  const [merchantId, setMerchantId] = useState("");
  const [selectedMethod, setSelectedMethod] = useState("Card");
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  // tabela
  const [rows, setRows] = useState([]);
  const [loadingRows, setLoadingRows] = useState(false);
  const [saving, setSaving] = useState(false);

  const uniqueMerchantsCount = useMemo(() => {
    return new Set(rows.map((r) => r.merchantId)).size;
  }, [rows]);

  function handleLogout() {
    console.log("Logout clicked");
  }

  async function loadRows() {
    setError("");
    setInfo("");
    setLoadingRows(true);

    try {
      const res = await fetch("https://localhost:7150/api/admin/paymentMethods/getAllrows", {
        method: "GET",
        headers: { "Content-Type": "application/json" },
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(normalizeApiErrorText(txt || `Failed to load. (${res.status})`));
      }

      const data = await res.json();
      // očekujemo [{ merchantId: "...", paymentMethodType: "Card" }]
      setRows(Array.isArray(data) ? data : []);
    } catch (e) {
      console.error(e);
      setError(e?.message ?? "Greška prilikom učitavanja.");
    } finally {
      setLoadingRows(false);
    }
  }

  useEffect(() => {
    loadRows();
  }, []);

  async function handleAdd(e) {
    e.preventDefault();
    setError("");
    setInfo("");

    if (!merchantId.trim()) {
      setError("MerchantId je obavezan.");
      return;
    }

    const guidRegex =
      /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

    if (!guidRegex.test(merchantId.trim())) {
      setError("MerchantId nije validan GUID format.");
      return;
    }

    setSaving(true);
    try {
      const payload = {
        merchantId: merchantId.trim(),
        paymentMethodType: selectedMethod, // string: "Card" | "QrCode" ...
      };

      const res = await fetch("https://localhost:7150/api/admin/paymentMethods/add", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(normalizeApiErrorText(txt || `Add failed. (${res.status})`));
      }

      setInfo("Uspešno dodato.");
      setMerchantId("");
      setSelectedMethod("Card");

      // najjednostavnije i najrobustnije: refresh tabele
      await loadRows();
    } catch (e) {
      console.error(e);
      setError(e?.message ?? "Greška prilikom dodavanja.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(merchantIdToDelete, methodTypeToDelete) {
    setError("");
    setInfo("");

    setSaving(true);
    try {
      const url =
        `https://localhost:7150/api/admin/paymentMethods/delete` +
        `?merchantId=${merchantIdToDelete}` +
        `&paymentMethodType=${methodTypeToDelete}`;

      const res = await fetch(url, {
        method: "DELETE",
        headers: { "Content-Type": "application/json" },
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(normalizeApiErrorText(txt || `Delete failed. (${res.status})`));
      }

      setInfo("Uspešno obrisano.");
      await loadRows();
    } catch (e) {
      console.error(e);
      setError(e?.message ?? "Greška prilikom brisanja.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="admin-page">
      <header className="admin-header">
        <div className="admin-brand">
          <img src={logo} alt="PSP logo" className="admin-logo" />
          <div className="admin-title">
            <div className="admin-name">PSP Admin</div>
            <div className="admin-sub">Payment methods management</div>
          </div>
        </div>

        <div className="admin-right">
          <div className="admin-safe">
            <img src={safeIcon} alt="Safe" className="safe-icon" />
            <p>
              Super admin panel • <strong>Manage merchants</strong>
            </p>
          </div>

          <button className="btn-secondary admin-logout" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </header>

      <main className="admin-container">
        <section className="admin-card">
          <h2>Add payment method to merchant</h2>
          <p className="admin-hint">
            Unesi MerchantId, izaberi metodu i klikni <strong>Add</strong>.
          </p>

          <form className="admin-form" onSubmit={handleAdd}>
            <div className="form-row">
              <label className="form-label">MerchantId</label>
              <input
                className="form-input"
                placeholder="Unesi MerchantId (GUID format)"
                value={merchantId}
                onChange={(e) => setMerchantId(e.target.value)}
                disabled={saving}
              />
            </div>

            <div className="form-row">
              <label className="form-label">Payment method</label>
              <select
                className="form-select"
                value={selectedMethod}
                onChange={(e) => setSelectedMethod(e.target.value)}
                disabled={saving}
              >
                {PAYMENT_METHODS.map((m) => (
                  <option key={m.value} value={m.value}>
                    {m.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-actions">
              <button className="btn-primary" type="submit" disabled={saving}>
                {saving ? "Saving..." : "Add"}
              </button>
            </div>

            {!!error && <div className="form-error">{error}</div>}
            {!!info && <div className="form-info">{info}</div>}
          </form>
        </section>

        <section className="admin-card">
          <div className="table-header">
            <h2>Merchant payment methods</h2>
            <div className="table-meta">
              Merchants: <strong>{uniqueMerchantsCount}</strong> • Rows: <strong>{rows.length}</strong>
            </div>
          </div>

          {loadingRows && <p style={{ textAlign: "center", padding: "10px 0", color: "#6f8796" }}>Loading...</p>}

          <div className="table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>MerchantId</th>
                  <th>Payment method</th>
                  <th className="col-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r, idx) => (
                  <tr key={`${r.merchantId}-${r.paymentMethodType}-${idx}`}>
                    <td className="mono">{r.merchantId}</td>
                    <td>{r.paymentMethodType}</td>
                    <td className="col-actions">
                      <button
                        className="btn-danger"
                        disabled={saving}
                        onClick={() => handleDelete(r.merchantId, r.paymentMethodType)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}

                {!loadingRows && rows.length === 0 && (
                  <tr>
                    <td colSpan={3} className="empty">
                      Nema podataka.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <p className="admin-note">Napomena: Nije dozvoljeno brisati poslednju metodu plaćanja za jednog merchant-a.</p>
        </section>
      </main>
    </div>
  );
}
