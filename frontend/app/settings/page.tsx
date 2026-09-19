"use client";

import { useState } from "react";
import { useAuth } from "../../context/AuthContext";


export default function SettingsPage() {
  const [revoked, setRevoked] = useState(false);
  const [matchVisibility, setMatchVisibility] = useState(true);
  const { token, apiUrl, logout } = useAuth();

  const handleRevoke = async () => {
    if (!token) return;

    try {
      await fetch(`${apiUrl}/auth/revoke`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });
    } catch {
      // ignore
    }

    setRevoked(true);
    setTimeout(() => {
      logout();
    }, 1500);
  };


  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 py-12">
      <h1 className="text-3xl font-bold text-white mb-2">Ayarlar</h1>
      <p className="text-zinc-400 text-sm mb-8">
        Hesap bağlantılarınızı ve gizlilik tercihlerinizi yönetin.
      </p>

      <div className="space-y-6">
        {/* Spotify Account Card */}
        <div className="p-6 rounded-3xl bg-zinc-900/40 border border-zinc-800">
          <h2 className="text-lg font-bold text-white mb-4">Spotify Bağlantısı</h2>
          <div className="flex items-center justify-between gap-4 py-3 border-b border-zinc-800">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-[#1DB954]/10 text-[#1DB954] flex items-center justify-center">
                <svg className="w-6 h-6" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
                </svg>
              </div>
              <div>
                <div className="text-sm font-semibold text-white">
                  {revoked ? "Bağlantı Kesildi" : "Spotify Hesabı Bağlı"}
                </div>
                <div className="text-xs text-zinc-400">
                  {revoked
                    ? "Eşleşme özelliklerini kullanmak için tekrar bağlanın."
                    : "Dinleme geçmişi senkronizasyonu aktif"}
                </div>
              </div>
            </div>

            {!revoked ? (
              <button
                onClick={handleRevoke}
                className="px-4 py-2 rounded-xl text-xs font-semibold bg-red-950/40 text-red-400 hover:bg-red-900/60 border border-red-800/60 transition cursor-pointer"
              >
                Bağlantıyı Kes
              </button>
            ) : (
              <a
                href={`${apiUrl}/auth/spotify`}
                className="px-4 py-2 rounded-xl text-xs font-semibold bg-[#1DB954] text-black hover:bg-[#1ed760] transition"
              >
                Tekrar Bağlan
              </a>
            )}
          </div>
        </div>

        {/* Privacy Card */}
        <div className="p-6 rounded-3xl bg-zinc-900/40 border border-zinc-800">
          <h2 className="text-lg font-bold text-white mb-4">Gizlilik & Görünürlük</h2>

          <div className="flex items-center justify-between py-3 border-b border-zinc-800">
            <div>
              <div className="text-sm font-semibold text-white">Eşleşmelerde Görün</div>
              <div className="text-xs text-zinc-400">
                Diğer kullanıcıların müzik eşleşmelerinde profilinizin listelenmesine izin verin.
              </div>
            </div>
            <button
              onClick={() => setMatchVisibility(!matchVisibility)}
              className={`w-12 h-6 flex items-center rounded-full p-1 transition-colors cursor-pointer ${
                matchVisibility ? "bg-[#1DB954]" : "bg-zinc-700"
              }`}
            >
              <div
                className={`bg-black w-4 h-4 rounded-full shadow-md transform transition-transform ${
                  matchVisibility ? "translate-x-6" : "translate-x-0"
                }`}
              />
            </button>
          </div>

          <p className="text-xs text-zinc-500 mt-4 leading-relaxed">
            SpoMusic, Spotify şifrenizi asla kaydetmez. Yalnızca Spotify OAuth2 API yetkilendirmesi ile müzik dinleme geçmişinizi okur.
          </p>
        </div>
      </div>
    </div>
  );
}
