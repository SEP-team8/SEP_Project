import { Link } from "react-router-dom";

export default function Failed() {
  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="card text-center py-12">
        <div className="text-4xl font-bold text-red-600 mb-4">
          Plaćanje neuspešno
        </div>
        <p className="text-gray-600 mb-6">
          Došlo je do greške prilikom obrade plaćanja. Proverite podatke ili
          pokušajte ponovo.
        </p>
        <div className="flex justify-center gap-3">
          <Link
            to="/checkout"
            className="inline-flex px-4 py-2 bg-sky-700 text-white rounded-lg"
          >
            Pokušaj ponovo
          </Link>
          <Link to="/" className="inline-flex px-4 py-2 border rounded-lg">
            Nazad na početnu
          </Link>
        </div>
      </div>
    </main>
  );
}
