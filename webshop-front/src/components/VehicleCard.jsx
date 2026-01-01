import { Link } from "react-router-dom";

export default function VehicleCard({ v }) {
  return (
    <article className="bg-white rounded-2xl shadow-md overflow-hidden flex flex-col">
      <div className="h-44 md:h-48 bg-gray-100">
        <img
          src={v.image || "/placeholder.png"}
          alt={v.model}
          className="w-full h-full object-cover"
        />
      </div>

      <div className="p-4 flex-1 flex flex-col">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h3 className="text-lg font-semibold">
              {v.make} {v.model}
            </h3>
            <p className="text-sm text-gray-500 mt-1 line-clamp-2">
              {v.description}
            </p>
          </div>

          <div className="text-right">
            <div className="text-sm text-gray-500">po danu</div>
            <div className="text-xl font-bold text-sky-700">{v.price}€</div>
          </div>
        </div>

        <div className="mt-4 flex items-center justify-between">
          <Link
            to={`/vehicle/${v.id}`}
            className="inline-flex items-center px-3 py-2 rounded-lg border text-sky-700 hover:bg-sky-50"
          >
            Detalji
          </Link>
          <button
            className="inline-flex items-center px-4 py-2 rounded-lg bg-sky-700 text-white hover:bg-sky-600"
            onClick={() => {
              const cart = JSON.parse(localStorage.getItem("cart") || "[]");
              cart.push({ vehicleId: v.id, price: v.price, days: 1 });
              localStorage.setItem("cart", JSON.stringify(cart));
              // optional: small UX hint
              const evt = new CustomEvent("cartUpdated");
              window.dispatchEvent(evt);
            }}
          >
            Dodaj u korpu
          </button>
        </div>
      </div>
    </article>
  );
}
