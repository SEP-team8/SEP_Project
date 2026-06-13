import { useEffect, useState } from "react";
import { ethers } from "ethers";
import "./PayCrypto.css";

function parseQuery() {
  const sp = new URLSearchParams(window.location.search);
  return {
    paymentId: sp.get("paymentId"),
  };
}

export default function PayCrypto() {
  const { paymentId } = parseQuery();
  const [loading, setLoading] = useState(false);
  const [payment, setPayment] = useState(null);
  const [error, setError] = useState("");
  const [statusText, setStatusText] = useState(""); // shows polling status

  useEffect(() => {
    async function load() {
      setError("");
      if (!paymentId) {
        setError("paymentId missing");
        return;
      }
      try {
        const res = await fetch(
          `/api/psp/crypto/paymentInfo?paymentId=${paymentId}`,
        );
        if (!res.ok)
          throw new Error("Ne mogu da dobijem podatke o crypto placanju");
        const data = await res.json();
        setPayment(data);
      } catch (e) {
        setError(e.message || e);
      }
    }
    load();
  }, [paymentId]);

  // POLLING: check status every 5s until finished
  useEffect(() => {
    if (!paymentId) return;
    let stopped = false;
    let intervalId = null;

    async function checkStatus() {
      try {
        setStatusText("Čekam potvrdu transakcije...");
        const resp = await fetch(
          `/api/psp/crypto/status?paymentId=${paymentId}`,
        );
        if (!resp.ok) {
          // not fatal: show message and continue polling
          setStatusText("Greška pri proveri statusa (server).");
          return;
        }

        const body = await resp.json();
        if (body.finished) {
          // redirect user to merchant URL
          window.location.href = body.redirectUrl;
          stopped = true;
          if (intervalId) clearInterval(intervalId);
        } else {
          // show intermediate status
          setStatusText(`Status: ${body.status ?? "waiting"}`);
        }
      } catch (err) {
        // network error -> show but keep polling
        setStatusText("Ne mogu da proverim status (mreža).");
      }
    }

    // first immediate check, then interval
    checkStatus();
    intervalId = setInterval(() => {
      if (!stopped) checkStatus();
    }, 5000);

    return () => {
      stopped = true;
      if (intervalId) clearInterval(intervalId);
    };
  }, [paymentId]);

  async function payWithMetaMask() {
    setError("");
    if (!window.ethereum) {
      setError("MetaMask nije dostupan");
      return;
    }
    if (!payment) {
      setError("Podaci o placanju nisu ucitani");
      return;
    }

    try {
      setLoading(true);
      const accounts = await window.ethereum.request({
        method: "eth_requestAccounts",
      });
      const from = accounts[0];

      const chainHex = await window.ethereum.request({ method: "eth_chainId" });
      const currentChainId = parseInt(chainHex, 16);
      if (payment.chainId && currentChainId !== payment.chainId) {
        throw new Error(
          `Molimo prebacite se na odgovarajucu mrezu (chainId ${payment.chainId}).`,
        );
      }

      const provider = new ethers.BrowserProvider(window.ethereum);
      const signer = await provider.getSigner();

      const tx = {
        to: payment.ethAddress,
        value: ethers.parseEther(payment.ethAmount.toString()),
      };

      const txResponse = await signer.sendTransaction(tx);

      const submitRes = await fetch("/api/psp/crypto/submitTx", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          paymentId: paymentId,
          txHash: txResponse.hash,
          fromAddress: from,
        }),
      });

      if (!submitRes.ok) {
        const txt = await submitRes.text().catch(() => "");
        if (txt.includes("insufficient funds")) {
          throw new Error("Nemate dovoljno Ethereum za ovu transakciju.");
        }
        throw new Error(
          `Neuspelo obavestavanje servera: ${submitRes.status} ${txt}`,
        );
      }

      alert(
        `Transakcija poslana: ${txResponse.hash}. Status ce biti azuriran.`,
      );
    } catch (e) {
      if (e?.message && e.message.includes("insufficient funds")) {
        setError("Nemate dovoljno Ethereum za ovu transakciju.");
      } else {
        setError(e?.message ?? String(e));
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="pay-crypto-container">
      <h2>Crypto plaćanje</h2>

      {error && <div className="pay-crypto-error">{error}</div>}

      {payment ? (
        <div className="pay-crypto-info">
          <div className="pay-crypto-row">
            <div className="pay-crypto-card">
              <p className="label">Iznos za slanje</p>
              <p className="value">{payment.ethAmount} ETH</p>
            </div>

            <div className="pay-crypto-card">
              <p className="label">Adresa primaoca</p>
              <p className="value mono">{payment.ethAddress}</p>
            </div>
          </div>

          <div className="pay-crypto-actions">
            <button
              className="pay-crypto-button"
              onClick={payWithMetaMask}
              disabled={loading}
            >
              {loading ? "Saljem transakciju..." : "Plati sa MetaMask"}
            </button>
            <div className="status-text">
              {statusText ? statusText : "Čekam na akciju..."}
            </div>
          </div>

          <p style={{ marginTop: "1rem", fontSize: "0.9rem", color: "#555" }}>
            Napomena: nakon slanja tx, status će biti ažuriran i bićete
            automatski vraćeni na stranicu trgovca.
          </p>
        </div>
      ) : (
        <p className="pay-crypto-loading">Učitavanje...</p>
      )}
    </div>
  );
}
