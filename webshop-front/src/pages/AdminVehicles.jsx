import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";

export default function AdminVehicles() {
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    setLoading(true);
    API.get("/vehicles")
      .then((res) => setVehicles(res.data))
      .catch((err) => console.error(err))
      .finally(() => setLoading(false));
  }, []);

  async function deleteVehicle(id) {
    if (!window.confirm("Da li ste sigurni da želite da obrišete ovo vozilo?"))
      return;
    try {
      await API.delete(`/vehicles/${id}`);
      setVehicles((prev) => prev.filter((v) => v.id !== id));
    } catch (err) {
      console.error(err);
      alert("Greška pri brisanju vozila.");
    }
  }

  // filtriramo vozila prema search query
  const filtered = vehicles.filter((v) => {
    if (!query) return true;
    const q = query.toLowerCase();
    return (
      (v.make && v.make.toLowerCase().includes(q)) ||
      (v.model && v.model.toLowerCase().includes(q)) ||
      (v.description && v.description.toLowerCase().includes(q))
    );
  });

  if (loading)
    return (
      <main className="max-w-6xl mx-auto p-8">
        <div className="text-gray-600">Loading vehicles...</div>
      </main>
    );

  return (
    <main className="max-w-6xl mx-auto p-8">
      {/* Search bar */}
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-semibold">Vehicles</h2>
        <div className="w-80">
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search..."
            className="w-full p-2 border rounded-md"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Add new card (uvek prva) */}
        <div
          onClick={() => navigate("/admin/vehicle")}
          className="flex flex-col items-center justify-center border-2 border-dashed rounded-2xl p-6 cursor-pointer hover:bg-gray-50"
        >
          <div className="text-3xl font-bold text-gray-400">+</div>
          <div className="text-gray-600 mt-2">Add new vehicle</div>
        </div>

        {/* Filtered vehicles (ako ih nema, Add New ostaje u grid) */}
        {filtered.length > 0 ? (
          filtered.map((v) => (
            <div
              key={v.id}
              className="bg-white rounded-2xl shadow-md overflow-hidden flex flex-col"
            >
              <div className="h-44 md:h-48 bg-gray-100">
                {v.image ? (
                  <img
                    src={`data:image/*;base64,${v.image}`}
                    alt={`${v.make} ${v.model}`}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-gray-400">
                    No picture
                  </div>
                )}
              </div>
              <div className="p-4 flex-1 flex flex-col">
                <h3 className="text-lg font-semibold">
                  {v.make} {v.model}
                </h3>
                <p className="text-sm text-gray-500 mt-1 line-clamp-2">
                  {v.description}
                </p>
                <div className="text-xl font-bold text-sky-700 mt-2">
                  {v.price} €/day
                </div>
                <div className="mt-4 flex gap-2">
                  <button
                    onClick={() => navigate(`/admin/vehicle/${v.id}`)}
                    className="flex-1 px-3 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-500"
                  >
                    Update
                  </button>
                  <button
                    onClick={() => deleteVehicle(v.id)}
                    className="flex-1 px-3 py-2 bg-red-600 text-white rounded-lg hover:bg-red-500"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))
        ) : (
          <div className="md:col-span-2 text-center text-gray-600 mt-4">
            There are no vehicles matching your search.
          </div>
        )}
      </div>
    </main>
  );
}
