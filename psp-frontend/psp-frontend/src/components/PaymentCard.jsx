import "./PaymentCard.css";
import logo from "../assets/psp_logo.png";
import safeIcon from "../assets/shield.png";
import visa from "../assets/visa_card.png";
import mastercard from "../assets/master_card.png";
import amex from "../assets/amex_card.png";
import qr from "../assets/qr-code.png";
import paypalLogo from "../assets/pay-pal.png";
import cryptocurrency from "../assets/cryptocurrency.png";
import { useEffect, useMemo, useState } from "react";

// enum mapping (frontend -> backend)
const PAYMENT_METHOD = {
  card: 0,
  qr: 1,
  paypal: 2,
  crypto: 3,
};

function parseQuery() {
  const sp = new URLSearchParams(window.location.search);
  return {
    merchantId: sp.get("merchantId") ?? "",
    stan: sp.get("stan") ?? "",
    pspTimestamp: sp.get("pspTimestamp") ?? "",
  };
}

function normalizePaymentMethodType(value) {
  // value može biti: 0/1/2/3 ili "Card"/"QrCode"/"PayPal"/"Crypto"
  if (typeof value === "number") return value;

  if (typeof value === "string") {
    const m = value.toLowerCase();
    if (m === "card") return 0;
    if (m === "qrcode" || m === "qr") return 1;
    if (m === "paypal") return 2;
    if (m === "crypto") return 3;
  }

  return null;
}

function normalizeMethodFromApi(method) {
  /**
   * Backend ti vraća LISTU PaymentMethod objekata:
   * { paymentMethodId: "...", paymentMethodType: 0 } ili "...Type": "Card"
   */
  if (method == null) return null;

  // slučaj: backend vraća broj direktno [0,1,2]
  if (typeof method === "number" || typeof method === "string") {
    return normalizePaymentMethodType(method);
  }

  // slučaj: backend vraća objekat PaymentMethod
  if (typeof method === "object") {
    // najčešće: { paymentMethodType: 0 } ili { paymentMethodType: "Card" }
    if ("paymentMethodType" in method) {
      return normalizePaymentMethodType(method.paymentMethodType);
    }

    // fallback ako nekad dođe { paymentMethod: { paymentMethodType: ... } }
    if (method.paymentMethod && "paymentMethodType" in method.paymentMethod) {
      return normalizePaymentMethodType(method.paymentMethod.paymentMethodType);
    }
  }

  return null;
}

export default function PaymentCard() {
  const { merchantId, stan, pspTimestamp } = useMemo(() => parseQuery(), []);

  const [loading, setLoading] = useState(false);
  const [methodsLoading, setMethodsLoading] = useState(false);
  const [error, setError] = useState("");

  const [availableMethodTypes, setAvailableMethodTypes] = useState([]);
  const [paymentMethod, setPaymentMethod] = useState("card");

  const [purchase, setPurchase] = useState(null);

  useEffect(() => {
    setPurchase({
      merchantOrderId: "ORDER12345",
      amount: 129.99,
      currency: "EUR",
    });
  }, []);

  useEffect(() => {
    async function loadMethods() {
      setError("");

      if (!merchantId || !stan || !pspTimestamp) {
        setError("Nedostaju parametri transakcije (merchantId/stan/pspTimestamp) u URL-u.");
        return;
      }

      setMethodsLoading(true);

      try {
        const res = await fetch(`/api/psp/paymentMethods/${merchantId}`, {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text().catch(() => "");
          throw new Error(`Ne mogu da učitam metode plaćanja: ${res.status}. ${txt}`);
        }

        const data = await res.json();

        const normalized = (Array.isArray(data) ? data : [])
          .map(normalizeMethodFromApi)
          .filter((x) => x !== null);

        if (!normalized.length) {
          // help za debug: da vidiš šta backend stvarno vraća
          console.log("paymentMethods raw response:", data);
          throw new Error("Merchant nema podešene metode plaćanja (frontend nije uspeo da mapira response).");
        }

        setAvailableMethodTypes(normalized);

        // default: prvi dostupni
        const first = normalized[0];
        if (first === 0) setPaymentMethod("card");
        else if (first === 1) setPaymentMethod("qr");
        else if (first === 2) setPaymentMethod("paypal");
        else if (first === 3) setPaymentMethod("crypto");
      } catch (e) {
        console.error(e);
        setError(e?.message ?? "Greška prilikom učitavanja metoda plaćanja.");
      } finally {
        setMethodsLoading(false);
      }
    }

    loadMethods();
  }, [merchantId, stan, pspTimestamp]);

  const isMethodAvailable = (methodKey) => {
    const enumValue = PAYMENT_METHOD[methodKey];
    return availableMethodTypes.includes(enumValue);
  };

  async function handleContinue() {
    setError("");

    if (!merchantId || !stan || !pspTimestamp) {
      setError("Nedostaju parametri transakcije u URL-u.");
      return;
    }

    if (!isMethodAvailable(paymentMethod)) {
      setError("Izabrani metod plaćanja nije dostupan za ovog trgovca.");
      return;
    }

    setLoading(true);

    const payload = {
      merchantId,
      stan,
      pspTimestamp,
      paymentMethod: PAYMENT_METHOD[paymentMethod],
    };

    try {
      const res = await fetch("https://localhost:7150/api/psp/selectPaymentMethod", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      const paymentUrl = await res.text();
      window.location.href = paymentUrl;
    } catch (e) {
      console.error(e);
      setError(e?.message ?? "Greška prilikom obrade plaćanja.");
      setLoading(false);
    }
  }

  return (
    <div className="payment-card">
      <header className="psp-header">
        <div className="psp-brand">
          <img src={logo} alt="PSP logo" className="psp-logo" />
          <span className="psp-text">PSP</span>
        </div>
        <div className="psp-safe">
          <img src={safeIcon} alt="Safe" className="safe-icon" />
          <p>
            Bezbedno plaćanje preko <strong>Payment Service Providera</strong>
          </p>
        </div>
      </header>

      <div className="content-wrapper">
        <section className="payment-methods">
          <h2>Izaberite način plaćanja</h2>

          {methodsLoading && <p>Učitavanje metoda plaćanja...</p>}
          {!!error && <p>{error}</p>}

          {isMethodAvailable("card") && (
            <label className={`payment-option ${paymentMethod === "card" ? "selected" : ""}`}>
              <input
                type="radio"
                name="payment"
                checked={paymentMethod === "card"}
                onChange={() => setPaymentMethod("card")}
              />
              <div className="option-content">
                <h3>Plaćanje karticom</h3>
                <p>Bićete preusmereni na sigurnu stranicu banke radi unosa podataka</p>
              </div>
              <div className="logos-right">
                <img src={visa} alt="Visa" />
                <img src={mastercard} alt="Mastercard" />
                <img src={amex} alt="Amex" />
              </div>
            </label>
          )}

          {isMethodAvailable("qr") && (
            <label className={`payment-option ${paymentMethod === "qr" ? "selected" : ""}`}>
              <input
                type="radio"
                name="payment"
                checked={paymentMethod === "qr"}
                onChange={() => setPaymentMethod("qr")}
              />
              <div className="option-content">
                <h3>Instant plaćanje (QR / IPS)</h3>
                <p>Skenirajte QR kod putem mobilne bankarske aplikacije</p>
              </div>
              <img src={qr} alt="QR" className="qr-icon" />
            </label>
          )}

          {isMethodAvailable("paypal") && (
            <label className={`payment-option ${paymentMethod === "paypal" ? "selected" : ""}`}>
              <input
                type="radio"
                name="payment"
                checked={paymentMethod === "paypal"}
                onChange={() => setPaymentMethod("paypal")}
              />
              <div className="option-content">
                <h3>PayPal</h3>
                <p>Bićete preusmereni na PayPal radi potvrde plaćanja</p>
              </div>
              <img src={paypalLogo} alt="PayPal" className="paypal-icon" />
            </label>
          )}

          {isMethodAvailable("crypto") && (
            <label className={`payment-option ${paymentMethod === "crypto" ? "selected" : ""}`}>
              <input
                type="radio"
                name="payment"
                checked={paymentMethod === "crypto"}
                onChange={() => setPaymentMethod("crypto")}
              />
              <div className="option-content">
                <h3>Crypto plaćanje</h3>
                <p>Plaćanje kriptovalutama putem jedinstvene adrese</p>
              </div>
              <div className="crypto-logos">
                <img src={cryptocurrency} alt="Cryptocurrency" />
              </div>
            </label>
          )}
        </section>

        <section className="purchase-info">
          <h2>Podaci o kupovini</h2>

          <div className="info-row">
            <span className="label">Usluga:</span>
            <span className="value">Iznajmljivanje vozila</span>
          </div>

          <div className="total">
            Ukupan iznos: <strong>{purchase ? `${purchase.amount} ${purchase.currency}` : "—"}</strong>
          </div>
          <p className="note">Podaci o kupovini dostavljeni od strane trgovca.</p>
        </section>
      </div>

      <div className="actions">
        <button
          className="btn-primary"
          onClick={handleContinue}
          disabled={loading || methodsLoading || !purchase || !availableMethodTypes.length || !!error}
        >
          {loading ? "Učitavanje..." : "Nastavi sa plaćanjem"}
        </button>
        <button className="btn-secondary" disabled={loading || methodsLoading}>
          Otkaži
        </button>
      </div>
    </div>
  );
}
