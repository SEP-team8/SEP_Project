import QRCode from "react-qr-code";
import { Link } from "react-router-dom";

export default function PaymentRedirect() {
  const data = JSON.parse(sessionStorage.getItem("qrData") || "null");

  if (!data)
    return (
      <main className="max-w-md mx-auto p-8">
        <div className="card text-center py-8 bg-white rounded-2xl shadow-sm">
          <p className="text-gray-600 mb-4">No QR data</p>
          <Link
            to="/checkout"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Back
          </Link>
        </div>
      </main>
    );

  const successUrl = `${window.location.origin}/payment-result`;
  const failedUrl = `${window.location.origin}/payment-result`;

  const simulateUrl = `${
    window.location.origin
  }/psp/simulate-payment?paymentId=${encodeURIComponent(
    data.paymentId ?? ""
  )}&successUrl=${encodeURIComponent(
    successUrl
  )}&failedUrl=${encodeURIComponent(failedUrl)}`;

  return (
    <main className="max-w-md mx-auto p-8">
      <div className="card text-center bg-white rounded-2xl shadow-sm p-6">
        <h2 className="text-2xl font-semibold mb-4">Scan the QR code to pay</h2>
        <div className="flex justify-center mb-6">
          <QRCode value={data.qrPayload || ""} size={180} />
        </div>
        <div className="text-sm text-gray-600 mb-2">
          PSP PaymentId:{" "}
          <span className="font-mono text-xs text-gray-800">
            {data.paymentId}
          </span>
        </div>
        <div className="text-sm text-gray-600 mb-4">
          Amount:{" "}
          <span className="font-semibold">
            {data.amount} {data.currency}
          </span>
        </div>
        <p className="text-gray-600 mb-6">
          Use your mBanking app to scan and pay or use PSP simulator.
        </p>

        <div className="flex justify-center gap-3">
          <a
            href={simulateUrl}
            className="inline-flex px-4 py-2 border rounded-lg"
            target="_blank"
            rel="noreferrer"
          >
            Open PSP simulator (simulate)
          </a>

          {/* Test button — mark as paid (client-only) redirects to payment-result with params,
              useful for quick manual testing without opening simulator. */}
          <Link
            to={`/payment-result?paymentId=${encodeURIComponent(
              data.paymentId ?? ""
            )}&global=GTID-${Date.now()}&status=OK`}
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Mark as paid (test)
          </Link>
        </div>
      </div>
    </main>
  );
}
