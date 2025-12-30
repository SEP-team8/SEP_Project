import React, { useEffect, useState } from "react";
import { getOrder } from "../api";

export default function Orders() {
  const [order, setOrder] = useState<any | null>(null);

  useEffect(() => {
    const qp = new URLSearchParams(window.location.search);
    const orderId = qp.get("orderId");
    if (orderId) getOrder(orderId).then((d) => setOrder(d.order));
  }, []);

  if (!order)
    return (
      <div>
        <h2>Orders</h2>
        <p>
          No order selected. If you just made a payment you will be redirected
          here with ?orderId=...
        </p>
      </div>
    );

  return (
    <div>
      <h2 className="text-xl mb-4">Order {order.merchantOrderId}</h2>
      <div>Status: {order.status}</div>
      <div>Amount: €{order.amount}</div>
      <pre className="mt-2">{JSON.stringify(order.items, null, 2)}</pre>
    </div>
  );
}
