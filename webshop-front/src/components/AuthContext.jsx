import { createContext, useContext, useEffect, useState } from "react";
import API from "../api";

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(true);

  async function refreshUser() {
    const storedToken = sessionStorage.getItem("token");
    const storedUser = sessionStorage.getItem("user");

    if (!storedToken) {
      setToken(null);
      setUser(null);
      setLoading(false);
      return;
    }

    setToken(storedToken);

    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch {}
    }

    try {
      const res = await API.get("/users/me");
      setUser(res.data);
      sessionStorage.setItem("user", JSON.stringify(res.data));
    } catch (err) {
      console.warn("refreshUser failed:", err);
      sessionStorage.removeItem("token");
      sessionStorage.removeItem("user");
      setToken(null);
      setUser(null);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refreshUser();
  }, []);
  const login = async ({ token: newToken, user: backendUser }) => {
    if (!newToken) {
      throw new Error("login() called without token");
    }

    sessionStorage.setItem("token", newToken);
    setToken(newToken);

    if (backendUser) {
      sessionStorage.setItem("user", JSON.stringify(backendUser));
      setUser(backendUser);

      window.dispatchEvent(
        new CustomEvent("authChanged", { detail: { user: backendUser } })
      );
      return;
    }

    try {
      const res = await API.get("/users/me");
      sessionStorage.setItem("user", JSON.stringify(res.data));
      setUser(res.data);

      window.dispatchEvent(
        new CustomEvent("authChanged", { detail: { user: res.data } })
      );
    } catch (err) {
      console.error("login fallback failed:", err);
      logout();
    }
  };

  const logout = () => {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("user");
    setToken(null);
    setUser(null);
    window.dispatchEvent(new CustomEvent("authChanged"));
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!token,
        loading,
        login,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
