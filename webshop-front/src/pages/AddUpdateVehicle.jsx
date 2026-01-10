import { useEffect, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import API from "../api";

export default function AddUpdateVehicle() {
  const { id } = useParams(); // ako postoji, radi update
  const [vehicle, setVehicle] = useState(null);
  const [make, setMake] = useState("");
  const [model, setModel] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [file, setFile] = useState(null);
  const [previewSrc, setPreviewSrc] = useState(null);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState(null);
  const fileInputRef = useRef(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return; // add mode
    API.get(`/vehicles/${id}`)
      .then((res) => {
        setVehicle(res.data);
        setMake(res.data.make);
        setModel(res.data.model);
        setDescription(res.data.description);
        setPrice(res.data.price);
        if (res.data.image)
          setPreviewSrc(`data:image/*;base64,${res.data.image}`);
      })
      .catch((err) => console.error(err));
  }, [id]);

  function onFileChange(e) {
    const f = e.target.files?.[0];
    if (!f) {
      setFile(null);
      setPreviewSrc(
        vehicle?.image ? `data:image/*;base64,${vehicle.image}` : null
      );
      return;
    }
    const reader = new FileReader();
    reader.onload = () => setPreviewSrc(reader.result);
    reader.readAsDataURL(f);
    setFile(f);
  }

  async function saveVehicle() {
    setBusy(true);
    setMessage(null);

    try {
      const formData = new FormData();
      formData.append("make", make);
      formData.append("model", model);
      formData.append("description", description);
      formData.append("price", price);
      if (file) formData.append("image", file);

      if (id) {
        // update
        await API.put(`/vehicles/${id}`, formData, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        setMessage("Vozilo ažurirano.");
      } else {
        // add
        await API.post("/vehicles", formData, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        setMessage("Vozilo dodato.");
      }
      navigate("/admin/vehicles");
    } catch (err) {
      console.error(err);
      setMessage("Greška pri čuvanju vozila.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="card bg-white p-6 rounded-2xl shadow-sm">
        <h2 className="text-2xl font-bold mb-4">
          {id ? "Update Vozilo" : "Dodaj Vozilo"}
        </h2>

        <div className="grid grid-cols-1 gap-4">
          <label className="text-sm">Marka</label>
          <input
            type="text"
            value={make}
            onChange={(e) => setMake(e.target.value)}
            className="border p-2 rounded"
          />

          <label className="text-sm">Model</label>
          <input
            type="text"
            value={model}
            onChange={(e) => setModel(e.target.value)}
            className="border p-2 rounded"
          />

          <label className="text-sm">Opis</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="border p-2 rounded"
          />

          <label className="text-sm">Cena po danu (RSD)</label>
          <input
            type="number"
            min="0"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            className="border p-2 rounded"
          />

          <label className="text-sm">Slika vozila</label>
          <input
            type="file"
            accept="image/*"
            ref={fileInputRef}
            onChange={onFileChange}
          />
          {previewSrc && (
            <div className="w-48 h-48 mt-2 border rounded overflow-hidden">
              <img
                src={previewSrc}
                alt="preview"
                className="w-full h-full object-cover"
              />
            </div>
          )}

          {message && <div className="text-red-600">{message}</div>}

          <div className="flex gap-2 mt-4">
            <button
              onClick={saveVehicle}
              disabled={busy}
              className="px-4 py-2 bg-sky-700 text-white rounded hover:bg-sky-600"
            >
              Sačuvaj
            </button>
            <button
              onClick={() => navigate(-1)}
              className="px-4 py-2 border rounded hover:bg-gray-50"
            >
              Otkaži
            </button>
          </div>
        </div>
      </div>
    </main>
  );
}
