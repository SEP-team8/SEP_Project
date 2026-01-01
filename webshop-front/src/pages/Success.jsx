import { Link } from "react-router-dom";

export default function Success() {
  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="card text-center py-12">
        <div className="text-4xl font-bold text-emerald-600 mb-4">
          Plaćanje uspešno
        </div>
        <p className="text-gray-600 mb-6">
          Hvala vam — vaša uplata je obrađena i rezervacija je potvrđena.
        </p>
        <div className="flex justify-center gap-3">
          <Link
            to="/vehicles"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Pogledaj vozila
          </Link>
          <Link to="/" className="inline-flex px-4 py-2 border rounded-lg">
            Početna
          </Link>
        </div>
      </div>
    </main>
  );
}
