import React, { useState } from "react";
import { createOrder } from "../api";

export default function Checkout({ cart, user, onPlaced }: any) {
  const [loading, setLoading] = useState(false);

  const placeOrder = async () => {
    setLoading(true);
    const resp = await createOrder({ userId: user.id, items: cart });
    // resp.payment.paymentUrl contains the PSP URL (our stub)
    const paymentUrl = resp.payment.paymentUrl;
    window.location.href = paymentUrl;
  };

  const total = cart.reduce(
    (s: number, it: any) => s + it.price * it.quantity,
    0
  );

  return (
    <div>
      <h2 className="text-xl mb-4">Checkout</h2>
      <div className="mb-4">
        {cart.map((c: any, idx: number) => (
          <div key={idx} className="flex justify-between py-2">
            {c.name}
            <span>€{c.price}</span>
          </div>
        ))}
        <div className="border-t pt-2 mt-2 font-semibold">Total: €{total}</div>
      </div>

      <button disabled={loading || cart.length === 0} onClick={placeOrder}>
        Proceed to payment
      </button>

      <p className="mt-4 text-sm">
        Payment redirect is simulated. After you approve or fail on PSP mock
        page, you'll be redirected back to the frontend.
      </p>
    </div>
  );
}
