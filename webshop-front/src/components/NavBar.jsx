import { Link } from "react-router-dom";
import { useEffect, useState } from "react";

export default function NavBar() {
  const [cartCount, setCartCount] = useState(
    JSON.parse(localStorage.getItem("cart") || "[]").length
  );

  // Osluškuj promene u korpi
  useEffect(() => {
    function onCartUpdate() {
      const cart = JSON.parse(localStorage.getItem("cart") || "[]");
      setCartCount(cart.length);
    }
    window.addEventListener("cartUpdated", onCartUpdate);
    return () => window.removeEventListener("cartUpdated", onCartUpdate);
  }, []);

  return (
    <header className="bg-white border-b shadow-sm">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo + Navigation */}
          <div className="flex items-center gap-6">
            <Link to="/" className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gradient-to-br from-sky-600 to-sky-800 rounded-md flex items-center justify-center text-white font-bold">
                RA
              </div>
              <div className="hidden sm:block">
                <div className="text-lg font-semibold">Rent-A-Car</div>
                <div className="text-xs text-gray-500">
                  Reliable. Local. Ready.
                </div>
              </div>
            </Link>

            <nav className="hidden md:flex items-center gap-4">
              <Link to="/vehicles" className="text-gray-600 hover:text-sky-700">
                Vozila
              </Link>
              <Link to="/about" className="text-gray-600 hover:text-sky-700">
                Kako radi
              </Link>
              <Link to="/contact" className="text-gray-600 hover:text-sky-700">
                Kontakt
              </Link>
            </nav>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-4">
            {/* Cart */}
            <Link
              to="/cart"
              className="relative inline-flex items-center gap-2 text-gray-600 hover:text-sky-700"
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeWidth="1.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M3 3h2l.4 2M7 13h10l4-8H5.4"
                />
              </svg>
              <span className="hidden sm:inline text-sm">Korpa</span>
              {cartCount > 0 && (
                <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full px-1.5">
                  {cartCount}
                </span>
              )}
            </Link>

            {/* Login / Register */}
            <div className="hidden sm:flex gap-2">
              <Link
                to="/login"
                className="inline-flex items-center px-4 py-2 rounded-lg border border-sky-700 text-sky-700 font-semibold hover:bg-sky-50"
              >
                Prijava
              </Link>
              <Link
                to="/register"
                className="inline-flex items-center px-4 py-2 rounded-lg bg-sky-700 text-white font-semibold hover:bg-sky-600"
              >
                Registracija
              </Link>
            </div>

            {/* Mobile menu button */}
            <button className="md:hidden p-2 rounded-md text-gray-600 hover:bg-gray-100">
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeWidth="1.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M4 6h16M4 12h16M4 18h16"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}
