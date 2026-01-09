import { Link } from "react-router-dom";
import { useState } from "react";

export default function VehicleCard({ v }) {
  const [days, setDays] = useState(1); // default 1 dan

  function addToCart() {
    const cart = JSON.parse(sessionStorage.getItem("cart") || "[]");
    cart.push({
      vehicleId: v.id,
      price: v.price,
      days: Math.max(1, Number(days)), // minimum 1 dan
    });
    sessionStorage.setItem("cart", JSON.stringify(cart));
    window.dispatchEvent(new CustomEvent("cartUpdated"));
  }

  return (
    <article className="bg-white rounded-2xl shadow-md overflow-hidden flex flex-col w-full md:w-100">
      {/* Slika */}
      <div className="h-44 md:h-48 bg-gray-100">
        <img
          src={v.image ? `data:image/*;base64,${v.image}` : "/placeholder.png"}
          alt={`${v.make} ${v.model}`}
          className="w-full h-full object-cover"
        />
      </div>

      {/* Opis i cena */}
      <div className="p-4 flex-1 flex flex-col justify-between">
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
            <div className="text-sm text-gray-500">per day</div>
            <div className="text-xl font-bold text-sky-700">{v.price}€</div>
          </div>
        </div>

        <div className="flex items-center justify-between mt-4 ">
          {/* Red dugmadi + input */}
          <div className="flex items-center gap-5">
            <Link
              to={`/vehicle/${v.id}`}
              className="px-3 py-2 rounded-lg border text-sky-700 hover:bg-sky-50 text-center"
            >
              Details
            </Link>

            <div className="flex items-center gap-1">
              <input
                type="number"
                min="1"
                value={days}
                onChange={(e) => setDays(e.target.value)}
                className="w-14 p-2 border rounded text-center"
              />
              <span className="text-gray-600">day(s)</span>
            </div>

            <button
              onClick={addToCart}
              className="px-4 py-2 rounded-lg bg-sky-700 text-white hover:bg-sky-600"
            >
              Add to cart
            </button>
          </div>
        </div>
      </div>
    </article>
  );
}
