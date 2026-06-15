import axios from "axios";

const baseURL = import.meta.env.VITE_API_BASE || "/api";

const API = axios.create({
  baseURL,
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

  const merchantId =
    sessionStorage.getItem("merchantId") || import.meta.env.VITE_MERCHANT_ID;

  console.debug("API: sending X-Merchant-Id header ->", merchantId); // <--- debug

  if (merchantId) {
    cfg.headers = {
      ...cfg.headers,
      "X-Merchant-Id": merchantId,
    };
  } else {
    console.warn(
      "There is no merchantId to send in the header (X-Merchant-Id)"
    );
  }

  return cfg;
});

API.interceptors.response.use(
  (response) => response,
  (err) => {
    if (err?.response?.status === 401) {
      console.warn("401 Unauthorized – the token may have expired");
    }
    return Promise.reject(err);
  }
);

export default API;
