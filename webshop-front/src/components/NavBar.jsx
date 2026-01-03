import { Link, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import API from "../api"; // tvoj axios instance sa tokenom

export default function NavBar() {
  const navigate = useNavigate();

  // cart count
  const [cartCount, setCartCount] = useState(() => {
    try {
      return JSON.parse(sessionStorage.getItem("cart") || "[]").length;
    } catch {
      return 0;
    }
  });

  // user state
  const [user, setUser] = useState(null);

  // funkcija za osvežavanje user-a
  async function refreshUser() {
    try {
      const res = await API.get("/users/me");
      const u = res.data;
      sessionStorage.setItem("user", JSON.stringify(u));
      setUser(u);
    } catch {
      sessionStorage.removeItem("user");
      setUser(null);
    }
  }

  // inicijalni fetch user-a
  useEffect(() => {
    const raw = sessionStorage.getItem("user");
    if (raw) {
      try {
        setUser(JSON.parse(raw));
      } catch {
        setUser(null);
      }
    }

    // odmah fetch users/me da bude aktuelno
    refreshUser();

    function onCartUpdate() {
      try {
        const cart = JSON.parse(sessionStorage.getItem("cart") || "[]");
        setCartCount(cart.length);
      } catch {
        setCartCount(0);
      }
    }

    function onAuthChange() {
      refreshUser();
    }

    function onStorageEvent(e) {
      if (!e) return;
      if (e.key === "cart") onCartUpdate();
      if (e.key === "user" || e.key === "token") onAuthChange();
    }

    window.addEventListener("cartUpdated", onCartUpdate);
    window.addEventListener("authChanged", onAuthChange);
    window.addEventListener("storage", onStorageEvent);

    return () => {
      window.removeEventListener("cartUpdated", onCartUpdate);
      window.removeEventListener("authChanged", onAuthChange);
      window.removeEventListener("storage", onStorageEvent);
    };
  }, []);

  function logout() {
    try {
      sessionStorage.removeItem("token");
      sessionStorage.removeItem("user");
    } catch {}
    window.dispatchEvent(new CustomEvent("authChanged"));
    navigate("/login");
  }

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
                Vehicles
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
              <span className="hidden sm:inline text-sm">Cart</span>
              {cartCount > 0 && (
                <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full px-1.5">
                  {cartCount}
                </span>
              )}
            </Link>

            {/* Admin button */}
            {user?.role === "Admin" && (
              <Link
                to="/admin/vehicles"
                className="inline-flex items-center px-3 py-2 rounded-lg border text-gray-700 hover:bg-gray-50"
              >
                Admin panel
              </Link>
            )}

            {/* Auth area */}
            {!user ? (
              <div className="hidden sm:flex gap-2">
                <Link
                  to="/login"
                  className="inline-flex items-center px-4 py-2 rounded-lg border border-sky-700 text-sky-700 font-semibold hover:bg-sky-50"
                >
                  Login
                </Link>
                <Link
                  to="/register"
                  className="inline-flex items-center px-4 py-2 rounded-lg bg-sky-700 text-white font-semibold hover:bg-sky-600"
                >
                  Registration
                </Link>
              </div>
            ) : (
              <div className="flex items-center gap-3">
                <Link
                  to="/profile"
                  className="hidden sm:flex items-center gap-2"
                >
                  <div className="w-8 h-8 rounded-full overflow-hidden bg-sky-700 text-white flex items-center justify-center text-sm font-semibold">
                    {user?.profilePictureBase64 ? (
                      <img
                        src={`data:image/*;base64,${user.profilePictureBase64}`}
                        alt="profile"
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <span>
                        {user?.name
                          ? user.name.charAt(0).toUpperCase()
                          : user?.email?.charAt(0).toUpperCase()}
                      </span>
                    )}
                  </div>
                  <div className="text-sm">
                    <div className="font-medium text-gray-800">
                      {user?.name || user?.email}
                    </div>
                    <div className="text-xs text-gray-500">Profile</div>
                  </div>
                </Link>

                <button
                  onClick={logout}
                  className="inline-flex items-center px-3 py-2 rounded-lg border text-gray-700 hover:bg-gray-50"
                >
                  Log out
                </button>
              </div>
            )}

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
