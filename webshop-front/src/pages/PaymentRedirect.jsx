import QRCode from "react-qr-code";
import { Link } from "react-router-dom";

export default function PaymentRedirect() {
  const data = JSON.parse(sessionStorage.getItem("qrData") || "null");

  if (!data)
    return (
      <main className="max-w-md mx-auto p-8">
        <div className="card text-center py-8 bg-white rounded-2xl shadow-sm">
          <p className="text-gray-600 mb-4">Nema QR podataka</p>
          <Link
            to="/checkout"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Nazad
          </Link>
        </div>
      </main>
    );

  return (
    <main className="max-w-md mx-auto p-8">
      <div className="card text-center bg-white rounded-2xl shadow-sm p-6">
        <h2 className="text-2xl font-semibold mb-4">
          Skenirajte QR kod da platite
        </h2>
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
          Koristite vašu mBanking aplikaciju da skenirate i platite.
        </p>
        <div className="flex justify-center gap-3">
          <Link
            to="/checkout"
            className="inline-flex px-4 py-2 border rounded-lg"
          >
            Nazad
          </Link>
          <Link
            to="/success"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Označi kao plaćeno (test)
          </Link>
        </div>
      </div>
    </main>
  );
}
