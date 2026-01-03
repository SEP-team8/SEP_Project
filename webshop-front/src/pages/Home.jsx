import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import API from "../api";
import VehicleCard from "../components/VehicleCard";

export default function Home() {
  const [vehicles, setVehicles] = useState([]);
  const [query, setQuery] = useState("");

  useEffect(() => {
    let mounted = true;
    API.get("/vehicles")
      .then((r) => mounted && setVehicles(r.data || []))
      .catch((err) => console.error("Failed to load vehicles:", err));
    return () => (mounted = false);
  }, []);

  const featured = vehicles.slice(0, 3);
  const filtered = vehicles.filter((v) => {
    if (!query) return true;
    const q = query.toLowerCase();
    return (
      (v.make && v.make.toLowerCase().includes(q)) ||
      (v.model && v.model.toLowerCase().includes(q)) ||
      (v.description && v.description.toLowerCase().includes(q))
    );
  });

  return (
    <main className="max-w-8xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Hero Section */}
      <section className="bg-white rounded-2xl p-8 mb-8 shadow-sm">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6">
          <div>
            <h1 className="text-3xl font-extrabold">
              Iznajmi automobil brzo i sigurno
            </h1>
            <p className="text-gray-600 mt-2 max-w-xl">
              Najbolje cene, transparentni uslovi i instant rezervacije. Izaberi
              auto, rezerviši i plati online.
            </p>
          </div>
          <div className="w-full md:w-1/2">
            <form
              onSubmit={(e) => e.preventDefault()}
              className="flex bg-gray-50 rounded-lg p-2 gap-2 border"
            >
              <input
                className="flex-1 bg-transparent outline-none px-3 py-2"
                placeholder="Pretraži po marki, modelu, gradu..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
              />
              <button
                type="submit"
                className="btn bg-sky-700 text-white px-4 py-2 rounded-lg"
              >
                Pretraži
              </button>
            </form>
          </div>
        </div>
      </section>

      {/* Featured Vehicles */}
      <section className="mb-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold">Istaknuta vozila</h2>
          <Link to="/vehicles" className="text-sky-700">
            Prikaži sva
          </Link>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {featured.map((v) => (
            <VehicleCard key={v.id} v={v} />
          ))}
        </div>
      </section>

      {/* Why Choose Us */}
      <section className="mt-8">
        <div className="card p-6 bg-white rounded-2xl shadow-sm">
          <h3 className="text-lg font-semibold">Zašto izabrati nas</h3>
          <ul className="mt-3 grid grid-cols-1 sm:grid-cols-3 gap-4 text-sm text-gray-600">
            <li>
              <strong>24/7 podrška</strong>
              <div className="text-xs">Kontakt uvek dostupan</div>
            </li>
            <li>
              <strong>Bez skrivenih troškova</strong>
              <div className="text-xs">Transparentni uslovi</div>
            </li>
            <li>
              <strong>Brza rezervacija</strong>
              <div className="text-xs">Rezerviši u par klikova</div>
            </li>
          </ul>
        </div>
      </section>

      {/* Quick List */}
      <section className="mt-8">
        <div className="card p-6 bg-white rounded-2xl shadow-sm">
          <h3 className="text-lg font-semibold mb-3">Brza lista</h3>
          {filtered.length === 0 ? (
            <p className="text-gray-600">Nema vozila za prikaz.</p>
          ) : (
            <ul className="grid gap-3">
              {filtered.slice(0, 8).map((v) => (
                <li key={v.id} className="py-2 border-b">
                  <Link
                    to={`/vehicle/${v.id}`}
                    className="flex items-center justify-between"
                  >
                    <span>
                      {v.make} {v.model}
                    </span>
                    <span className="text-sky-700 font-semibold">
                      {v.price} €/dan
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </div>
      </section>
    </main>
  );
}
