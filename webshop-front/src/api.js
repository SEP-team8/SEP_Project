import axios from "axios";

const API = axios.create({
  baseURL: "https://localhost:7171/api",
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

  /**
   * DEV / TEST ONLY
   * omogućava multi-tenant bez domena
   */
  const merchantId = sessionStorage.getItem("merchantId");
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
