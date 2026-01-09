import axios from "axios";

const baseURL = import.meta.env.VITE_API_BASE;
if (!baseURL) {
  console.warn("VITE_API_BASE nije postavljen! Pokušavam sa defaultom...");
}

const API = axios.create({
  baseURL: baseURL || "https://api.shop1.localhost:7171/api",
  withCredentials: false,
  timeout: 20000,
});

API.interceptors.request.use((cfg) => {
  const token = sessionStorage.getItem("token");
  if (token) {
    cfg.headers = {
      ...cfg.headers,
      Authorization: `Bearer ${token}`,
    };
  }

  // Pošalji X-Merchant-Id header ako postoji u sessionStorage ili env var
  const merchantId =
    sessionStorage.getItem("merchantId") ||
    import.meta.env.VITE_MERCHANT_ID ||
    null;
  if (merchantId) {
    cfg.headers = {
      ...cfg.headers,
      "X-Merchant-Id": merchantId,
    };
  }

  return cfg;
});

API.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err?.response?.status === 401) {
      console.warn("401 Unauthorized");
    }
    return Promise.reject(err);
  }
);

export default API;
