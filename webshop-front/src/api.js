import axios from "axios";

const API = axios.create({
  baseURL: import.meta.env.VITE_API_BASE || "http://localhost:5210/api",
  withCredentials: true,
});

// Attach JWT from sessionStorage to every request if present
API.interceptors.request.use(
  (config) => {
    try {
      const token = sessionStorage.getItem("token");
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch (e) {
      // ignore
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default API;
