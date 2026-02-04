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
      if (e?.message.includes("insufficient funds")) {
        setError("Nemate dovoljno Ethereum za ovu transakciju.");
      } else {
        setError(e?.message ?? e);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="pay-crypto-container">
      <h2>Crypto placanje</h2>
      {error && <div className="pay-crypto-error">{error}</div>}
      {payment ? (
        <div className="pay-crypto-info">
          <div className="pay-crypto-card">
            <p className="label">Iznos za slanje:</p>
            <p className="value">{payment.ethAmount} ETH</p>
          </div>
          <div className="pay-crypto-card">
            <p className="label">Adresa primaoca:</p>
            <p className="value">{payment.ethAddress}</p>
          </div>
          <button
            className="pay-crypto-button"
            onClick={payWithMetaMask}
            disabled={loading}
          >
            {loading ? "Saljem transakciju..." : "Plati sa MetaMask"}
          </button>
          <p style={{ marginTop: "1rem", fontSize: "0.9rem", color: "#555" }}>
            Napomena: transakcija ce biti verifikovana na backendu nakon što se
            pošalje.
          </p>
        </div>
      ) : (
        <p className="pay-crypto-loading">Učitavanje...</p>
      )}
    </div>
  );
}
