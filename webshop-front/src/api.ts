import axios from "axios";
const API = axios.create({ baseURL: "http://localhost:5204/api" });

export async function getVehicles() {
  const r = await API.get("/vehicles");
  return r.data;
}

export async function createOrder(payload: any) {
  const r = await API.post("/orders", payload);
  return r.data;
}

export async function getOrder(orderId: string) {
  const r = await API.get(`/orders/${orderId}`);
  return r.data;
}

export default API;
