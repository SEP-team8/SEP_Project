import { useEffect, useState } from "react";
import API from "../api";
import VehicleCard from "../components/VehicleCard";

export default function Vehicles() {
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState("");

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    API.get("/vehicles")
      .then((r) => {
        if (mounted) setVehicles(r.data || []);
      })
      .catch((err) => console.error(err))
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => (mounted = false);
  }, []);

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
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-semibold">Vehicles</h2>
        <div className="w-80">
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search..."
            className="w-full p-2 border rounded-md"
          />
        </div>
      </div>

      {loading ? (
        <div className="card p-6 bg-white rounded-2xl shadow-sm">
          Loading...
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {filtered.map((v) => (
            <VehicleCard key={v.id} v={v} />
          ))}
        </div>
      )}

      {!loading && filtered.length === 0 && (
        <div className="card mt-6 p-6 text-center bg-white rounded-2xl shadow-sm text-gray-600">
          No vehicles to display.
        </div>
      )}
    </main>
  );
}
