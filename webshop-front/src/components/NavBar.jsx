import { Link } from "react-router-dom";

export default function NavBar() {
  return (
    <header className="sticky top-0 z-50 bg-white border-b">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          {/* LEFT */}
          <div className="flex items-center gap-8">
            {/* Logo */}
            <Link to="/" className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-sky-700 flex items-center justify-center text-white font-bold text-sm">
                RAC
              </div>
              <div className="hidden sm:flex flex-col leading-tight">
                <span className="font-semibold text-gray-900">Rent-A-Car</span>
                <span className="text-xs text-gray-500">
                  Reliable. Local. Ready.
                </span>
              </div>
            </Link>

            {/* Desktop navigation */}
            <nav className="hidden md:flex items-center gap-6">
              <Link
                to="/vehicles"
                className="text-sm font-medium text-gray-600 hover:text-sky-700"
              >
                Vozila
              </Link>
              <Link
                to="/about"
                className="text-sm font-medium text-gray-600 hover:text-sky-700"
              >
                Kako radi
              </Link>
              <Link
                to="/contact"
                className="text-sm font-medium text-gray-600 hover:text-sky-700"
              >
                Kontakt
              </Link>
            </nav>
          </div>

          {/* RIGHT */}
          <div className="flex items-center gap-4">
            {/* Cart */}
            <Link
              to="/cart"
              className="relative inline-flex items-center text-gray-600 hover:text-sky-700"
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
                  d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13l-1.5 6h13L17 13"
                />
              </svg>

              {/* badge */}
              <span className="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] rounded-full bg-red-500 text-white text-xs flex items-center justify-center">
                2
              </span>
            </Link>

            {/* Auth buttons */}
            <div className="hidden sm:flex items-center gap-3">
              <Link
                to="/login"
                className="text-sm font-medium text-gray-600 hover:text-sky-700"
              >
                Prijava
              </Link>
              <Link
                to="/register"
                className="inline-flex items-center rounded-lg bg-sky-700 px-4 py-2 text-sm font-semibold text-white hover:bg-sky-600"
              >
                Registracija
              </Link>
            </div>

            {/* Mobile menu button */}
            <button
              className="md:hidden inline-flex items-center justify-center rounded-lg p-2 text-gray-600 hover:bg-gray-100"
              aria-label="Open menu"
            >
              <svg
                className="h-6 w-6"
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
