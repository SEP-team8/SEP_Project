import "./PaymentCard.css";
import logo from "../assets/psp_logo.png";
import safeIcon from "../assets/shield.png";
import visa from "../assets/visa_card.png";
import mastercard from "../assets/master_card.png";
import amex from "../assets/amex_card.png";
import qr from "../assets/qr-code.png";
import paypalLogo from "../assets/pay-pal.png";
import cryptocurrency from "../assets/cryptocurrency.png";
import { useEffect, useState } from "react";


const MOCK_PURCHASE = {
  merchantId: "Rent-a-Car Agency",
  merchantOrderId: "ORDER12345",
  amount: 129.99,
  currency: "EUR",
};

export default function PaymentCard() {

  const [purchase, setPurchase] = useState(null);
  const [paymentMethod, setPaymentMethod] = useState("card"); // default payment method
  const [loading, setLoading] = useState(false);

  useEffect(() => {
       // TODO: kasnije ovo dolazi od webshop backenda
      setPurchase(MOCK_PURCHASE);
  }, []);


  async function handleContinue() {
    if (!purchase) return; // safety check

    setLoading(true);

    const payload = {
      paymentMethod,
      merchantId: purchase.merchant,
      purchase: {
        merchantOrderId: purchase.merchantOrderId,
        amount: purchase.amount,     
        currency: purchase.currency
      }
    };

    const url = "/api/psp/startPayment";

    try {
      const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const errText = await res.text().catch(() => "");
        throw new Error(`PSP payment failed: ${res.status}. ${errText}`);
      }
      const data = await res.json();

      if (!data?.redirectUrl) {
        throw new Error("Backend nije vratio redirectUrl.");
      }

      // redirect na bank front (PaymentRequestUrl koji vraća banka)
      console.log("Redirecting to:", data.redirectUrl);
      window.location.href = data.redirectUrl;
  
    } catch (e) {
      console.error(e);
      alert("Greska prilikom obrade placanja. Pokusajte ponovo.");
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
        </section>

        <section className="purchase-info">
          <h2>Podaci o kupovini</h2>

          <div className="info-row">
            <span className="label">Usluga:</span>
            <span className="value">Iznajmljivanje vozila</span>
          </div>

          <div className="total">Ukupan iznos: <strong>{purchase ? `${purchase.amount} ${purchase.currency}` : '—'}</strong></div>
          <p className="note">Podaci o kupovini dostavljeni od strane trgovca.</p>
        </section>
      </div>

      <div className="actions">
        <button className="btn-primary" onClick={handleContinue} disabled={loading || !purchase}>
          {loading ? "Učitavanje..." : "Nastavi sa plaćanjem"}
        </button>
        <button className="btn-secondary" disabled={loading}>Otkaži</button>
      </div>
    </div>
  );
}