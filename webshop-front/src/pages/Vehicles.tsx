import { useEffect, useState } from "react";
import { getVehicles } from "../api";

export default function Vehicles({ onAdd }: { onAdd: (v: any) => void }) {
  const [vehicles, setVehicles] = useState<any[]>([]);
  useEffect(() => {
    getVehicles().then(setVehicles);
  }, []);

  return (
    <div>
      <h2 className="text-xl mb-4">Available vehicles</h2>
      <div className="grid grid-cols-1 gap-4">
        {vehicles.map((v) => (
          <div key={v.id} className="p-4 border rounded">
            <div className="flex justify-between">
              <div>
                <div className="font-semibold">
                  {v.make} {v.model}
                </div>
                <div className="text-sm">Class: {v.class}</div>
              </div>
              <div className="text-right">
                <div className="text-lg">€{v.pricePerDay}/day</div>
                <button
                  className="mt-2"
                  onClick={() =>
                    onAdd({
                      id: v.id,
                      name: `${v.make} ${v.model}`,
                      price: v.pricePerDay,
                      quantity: 1,
                    })
                  }
                >
                  Add to cart
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
