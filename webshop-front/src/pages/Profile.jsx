import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api";
import { useAuth } from "../components/AuthContext";

export default function Profile() {
  const { user: authUser, token, login, logout } = useAuth();
  const [profile, setProfile] = useState(authUser);
  const [editing, setEditing] = useState(false);

  const [name, setName] = useState(authUser?.name || "");
  const [email, setEmail] = useState(authUser?.email || "");
  const [previewSrc, setPreviewSrc] = useState(
    authUser?.profilePictureBase64
      ? `data:image/*;base64,${authUser.profilePictureBase64}`
      : null
  );
  const [file, setFile] = useState(null);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");

  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState(null);
  const fileInputRef = useRef(null);
  const navigate = useNavigate();

  useEffect(() => {
    // Refresh profile on mount
    async function load() {
      try {
        const res = await API.get("/users/me");
        setProfile(res.data);
        // keep auth context in sync (preserve token)
        if (token) login({ token, user: res.data });
      } catch (err) {
        // if unauthorized or other error, logout
        console.error("Failed to load profile:", err);
        logout();
      }
    }
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // run once

  useEffect(() => {
    // when authUser changes (login), sync local state
    setProfile(authUser);
    setName(authUser?.name || "");
    setEmail(authUser?.email || "");
    setPreviewSrc(
      authUser?.profilePictureBase64
        ? `data:image/*;base64,${authUser.profilePictureBase64}`
        : null
    );
  }, [authUser]);

  function onFileChange(e) {
    const f = e.target.files?.[0];
    if (!f) {
      setFile(null);
      setPreviewSrc(
        profile?.profilePictureBase64
          ? `data:image/*;base64,${profile.profilePictureBase64}`
          : null
      );
      return;
    }

    // limit file size client-side (5MB)
    const maxBytes = 5 * 1024 * 1024;
    if (f.size > maxBytes) {
      setMessage("Image is too large. Max 5 MB.");
      fileInputRef.current.value = null;
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      setPreviewSrc(reader.result);
    };
    reader.readAsDataURL(f);
    setFile(f);
    setMessage(null);
  }

  async function saveProfile() {
    setMessage(null);
    setBusy(true);

    try {
      // 1) update basic profile (name/email)
      const updateBody = { name };
      // if you allow email change, add email: email
      const res = await API.put("/users", updateBody);
      let updated = res.data;

      // 2) if there is a file -> upload it (multipart)
      if (file) {
        const fd = new FormData();
        fd.append("file", file);
        const up = await API.post("/users/picture", fd, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        updated = up.data;
      }

      // 3) update local state + auth context + sessionStorage
      setProfile(updated);
      if (token) login({ token, user: updated });
      setEditing(false);
      setMessage("Profile saved successfully.");
      setFile(null);
      if (fileInputRef.current) fileInputRef.current.value = null;
    } catch (err) {
      console.error("Error saving profile:", err);
      setMessage(err?.response?.data?.error || "Error saving profile.");
    } finally {
      setBusy(false);
    }
  }

  async function changePassword() {
    setMessage(null);
    if (!currentPassword || !newPassword) {
      setMessage("Fill in both password fields.");
      return;
    }
    setBusy(true);
    try {
      const res = await API.put("/users/password", {
        currentPassword,
        newPassword,
      });
      setCurrentPassword("");
      setNewPassword("");
      setMessage("Password successfully changed.");
    } catch (err) {
      console.error("Error changing password:", err);
      setMessage(err?.response?.data?.error || "Error changing password.");
    } finally {
      setBusy(false);
    }
  }

  function handleLogout() {
    logout();
    navigate("/login");
  }

  if (!profile) {
    return (
      <main className="max-w-4xl mx-auto p-8">
        <div className="card text-center py-12">
          <div className="text-gray-600">You are not logged in.</div>
        </div>
      </main>
    );
  }

  return (
    <main className="max-w-4xl mx-auto p-8">
      <div className="card p-6 bg-white rounded-2xl shadow-sm">
        <div className="flex items-center gap-6">
          <div className="w-24 h-24 rounded-full overflow-hidden bg-sky-700 text-white flex items-center justify-center text-3xl font-semibold">
            {previewSrc ? (
              <img
                src={previewSrc}
                alt="profile"
                className="w-full h-full object-cover"
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center">
                {profile.name
                  ? profile.name.charAt(0).toUpperCase()
                  : (profile.email || "U").charAt(0).toUpperCase()}
              </div>
            )}
          </div>

          <div className="flex-1">
            <h2 className="text-2xl font-bold">
              {profile.name || profile.email}
            </h2>
            <div className="text-sm text-gray-600">{profile.email}</div>
            <div className="text-xs text-gray-500 mt-1">
              Role: {profile.role || "User"}
            </div>
          </div>
        </div>

        <div className="mt-6 flex gap-3">
          <button
            onClick={handleLogout}
            className="inline-flex px-4 py-2 bg-red-600 text-white rounded-lg"
          >
            Log out
          </button>
          <button
            onClick={() => setEditing(true)}
            className="inline-flex px-4 py-2 border rounded-lg"
          >
            Edit profile
          </button>
        </div>

        {editing && (
          <div className="mt-6 p-4 border rounded">
            <div className="grid grid-cols-1 gap-3">
              <label className="text-sm">Name</label>
              <input
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="border p-2 rounded"
              />

              <label className="text-sm">Profile picture</label>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={onFileChange}
              />

              <div className="text-sm font-semibold mb-2">Change password</div>
              <div className="grid grid-cols-1 gap-2">
                <input
                  type="password"
                  placeholder="Current password"
                  value={currentPassword}
                  onChange={(e) => setCurrentPassword(e.target.value)}
                  className="border p-2 rounded"
                />
                <input
                  type="password"
                  placeholder="New password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="border p-2 rounded"
                />
              </div>

              <div className="flex gap-2 mt-2">
                <button
                  disabled={busy}
                  onClick={saveProfile}
                  className="px-4 py-2 bg-blue-600 text-white rounded"
                >
                  Save
                </button>
                <button
                  onClick={() => {
                    setEditing(false);
                    setFile(null);
                    setName(profile.name || "");
                    if (fileInputRef.current) fileInputRef.current.value = null;
                    setPreviewSrc(
                      profile.profilePictureBase64
                        ? `data:image/*;base64,${profile.profilePictureBase64}`
                        : null
                    );
                  }}
                  className="px-4 py-2 border rounded"
                >
                  Cancel
                </button>
                <div className="flex gap-2">
                  <button
                    disabled={busy}
                    onClick={changePassword}
                    className="px-4 py-2 bg-yellow-400 text-white rounded"
                  >
                    Change password
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {message && <div className="mt-4 text-sm text-red-600">{message}</div>}
      </div>
    </main>
  );
}
