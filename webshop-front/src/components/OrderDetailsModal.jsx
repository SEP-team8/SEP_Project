export default function OrderDetailsModal({ open, data, loading, onClose }) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-2xl max-w-2xl w-full p-6">
        <div className="flex items-start justify-between">
          <h3 className="text-xl font-semibold">Order details</h3>
          <button onClick={onClose} className="text-sm text-gray-500">
            Close
          </button>
        </div>

        {loading ? (
          <div className="py-8">Loading...</div>
        ) : !data ? (
          <div className="py-8 text-gray-600">No data.</div>
        ) : (
          <>
            <div className="mt-4 text-sm text-gray-600">
              Order: <span className="font-mono">{data.orderId}</span>
            </div>
            <div className="mt-2">
              <div className="text-sm">
                Status: <strong>{data.status}</strong>
              </div>
              <div className="text-sm">
                Amount:{" "}
                <strong>
                  {data.currency} {Number(data.amount).toFixed(2)}
                </strong>
              </div>
              <div className="text-sm">
                Created:{" "}
                <strong>{new Date(data.createdAt).toLocaleString()}</strong>
              </div>
            </div>

            <div className="mt-4">
              <h4 className="font-semibold">Items</h4>
              {!data.items || data.items.length === 0 ? (
                <div className="text-gray-600 text-sm mt-2">No items.</div>
              ) : (
                <ul className="mt-2 divide-y">
                  {data.items.map((it, i) => (
                    <li
                      key={i}
                      className="py-3 flex justify-between items-center"
                    >
                      <div>
                        <div className="font-medium">{it.vehicleName}</div>
                        <div className="text-xs text-gray-500">
                          {it.days} day(s) × {it.pricePerDay.toFixed(2)} €/day
                        </div>
                      </div>
                      <div className="font-semibold">
                        {it.total.toFixed(2)} €
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
