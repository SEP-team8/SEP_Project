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
  const [txSent, setTxSent] = useState(false);
  const [payment, setPayment] = useState(null);
  const [error, setError] = useState("");
  const [statusText, setStatusText] = useState("");

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

    // traži pristup nalogu pre bilo kakvih RPC poziva
    const provider = new ethers.BrowserProvider(window.ethereum, "any");
    await provider.send("eth_requestAccounts", []);

    // proveri mrežu
    const network = await provider.getNetwork();
    const currentChainId = Number(network.chainId);

    if (payment.chainId && currentChainId !== payment.chainId) {
      const targetChainHex = "0x" + Number(payment.chainId).toString(16);

      try {
        await window.ethereum.request({
          method: "wallet_switchEthereumChain",
          params: [{ chainId: targetChainHex }],
        });
      } catch {
        throw new Error(
          `Prebacite MetaMask na mrezu sa chainId ${payment.chainId}.`,
        );
      }
    }

    const signer = await provider.getSigner();
    const from = await signer.getAddress();

    const tx = {
      to: payment.ethAddress,
      value: payment.amountWei
        ? BigInt(payment.amountWei)
        : ethers.parseEther(String(payment.ethAmount)),
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
      throw new Error(`Neuspelo obavestavanje servera: ${submitRes.status} ${txt}`);
    }

    setTxSent(true);
  } catch (e) {
    const message = e?.message ?? String(e);
    const isRejected =
      e?.code === 4001 ||
      e?.info?.error?.code === 4001 ||
      message.includes("ACTION_REJECTED") ||
      message.includes("user rejected") ||
      message.includes("User denied");

    if (isRejected) {
      await fetch(`/api/psp/crypto/cancel?paymentId=${paymentId}`, { method: "POST" }).catch(() => {});
      if (payment?.failedUrl) {
        window.location.href = payment.failedUrl;
        return;
      }
    } else if (message.includes("insufficient funds")) {
      setError("Nemate dovoljno Ethereum za ovu transakciju.");
    } else if (
      message.includes("Unauthorized") ||
      message.includes("could not coalesce") ||
      e?.code === -32006 ||
      e?.data?.httpStatus === 401
    ) {
      setError(
        "MetaMask nije dao potrebna odobrenja ili je zahtev odbijen. Otvori MetaMask i ponovo odobri sajt.",
      );
    } else {
      setError(message);
    }
  } finally {
    setLoading(false);
  }
}

  if (txSent) {
    return (
      <div className="pay-crypto-container">
        <h2>Crypto plaćanje</h2>
        <div className="pay-crypto-waiting">
          <div className="pay-crypto-spinner" />
          <p className="pay-crypto-waiting-title">Transakcija je poslata!</p>
          <p className="pay-crypto-waiting-sub">
            Čekam potvrdu na blockchain mreži.<br />
            Bićete automatski preusmereni na stranicu prodavca.
          </p>
        </div>
      </div>
    );
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
