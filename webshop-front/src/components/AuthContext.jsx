import { createContext, useContext, useEffect, useState } from "react";
import API from "../api";

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(true);

  async function refreshUser() {
    const t = sessionStorage.getItem("token");
    if (!t) {
      setToken(null);
      setUser(null);
      setLoading(false);
      return;
    }
    try {
      setToken(t);
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

  const login = async ({ token: newToken, user: maybeUser }) => {
    sessionStorage.setItem("token", newToken);
    setToken(newToken);

    try {
      const res = await API.get("/users/me");
      const fresh = res.data;
      sessionStorage.setItem("user", JSON.stringify(fresh));
      setUser(fresh);
      window.dispatchEvent(
        new CustomEvent("authChanged", { detail: { user: fresh } })
      );
    } catch (err) {
      if (maybeUser) {
        sessionStorage.setItem("user", JSON.stringify(maybeUser));
        setUser(maybeUser);
        window.dispatchEvent(
          new CustomEvent("authChanged", { detail: { user: maybeUser } })
        );
      } else {
        sessionStorage.removeItem("token");
        setToken(null);
        setUser(null);
      }
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
