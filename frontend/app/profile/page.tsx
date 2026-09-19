"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "../../context/AuthContext";
import TrackPreviewPlayer, { type PreviewTrack } from "../../components/TrackPreviewPlayer";

interface TopTrackItem {
  id: string;
  rank: number;
  track: {
    spotifyId: string;
    name: string;
    artists: string[];
    album: string;
    imageUrl?: string | null;
    previewUrl?: string | null;
    url: string;
  };
}

interface HistoryItem {
  id: string;
  playedAt: string;
  track: {
    spotifyId: string;
    name: string;
    artists: string[];
    album: string;
    imageUrl?: string | null;
    previewUrl?: string | null;
    url: string;
  };
}

export default function ProfilePage() {
  const { user, token, apiUrl } = useAuth();
  const [syncing, setSyncing] = useState(false);
  const [syncMessage, setSyncMessage] = useState<string | null>(null);
  const [topTracks, setTopTracks] = useState<TopTrackItem[]>([]);
  const [recentHistory, setRecentHistory] = useState<HistoryItem[]>([]);
  const [activePreview, setActivePreview] = useState<PreviewTrack | null>(null);

  useEffect(() => {
    if (!token) return;

    // Fetch user top tracks
    fetch(`${apiUrl}/spotify/top`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((res) => (res.ok ? res.json() : []))
      .then((data) => {
        if (Array.isArray(data)) setTopTracks(data);
      })
      .catch(() => {});

    // Fetch user listening history
    fetch(`${apiUrl}/spotify/history`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((res) => (res.ok ? res.json() : []))
      .then((data) => {
        if (Array.isArray(data)) setRecentHistory(data);
      })
      .catch(() => {});
  }, [token, apiUrl]);

  const handleSync = async () => {
    if (!token) return;
    setSyncing(true);
    setSyncMessage(null);
    try {
      const res = await fetch(`${apiUrl}/spotify/sync`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });
      const data = await res.json();
      if (data.success) {
        setSyncMessage(`Senkronizasyon tamamlandı! ${data.syncedTracks} parça güncellendi.`);
        // Refresh local data
        const [topRes, histRes] = await Promise.all([
          fetch(`${apiUrl}/spotify/top`, { headers: { Authorization: `Bearer ${token}` } }),
          fetch(`${apiUrl}/spotify/history`, { headers: { Authorization: `Bearer ${token}` } }),
        ]);
        if (topRes.ok) setTopTracks(await topRes.json());
        if (histRes.ok) setRecentHistory(await histRes.json());
      } else if (data.spotifyError) {
        setSyncMessage(`Spotify hatası [${data.spotifyStatus}]: ${data.spotifyError}`);
      } else {
        setSyncMessage(data.message || "Senkronizasyon başarısız. Lütfen tekrar giriş yapın.");
      }
    } catch {
      setSyncMessage("Bağlantı hatası. API'ye ulaşılamıyor.");
    } finally {
      setSyncing(false);
    }
  };

  const topArtistName =
    topTracks.length > 0 && topTracks[0].track?.artists?.length > 0
      ? topTracks[0].track.artists[0]
      : "-";

  const topTrackName =
    topTracks.length > 0 && topTracks[0].track?.name
      ? topTracks[0].track.name
      : "-";

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 py-12">
      {/* Profile Card */}
      <div className="p-8 rounded-3xl bg-zinc-900/50 border border-zinc-800/80 backdrop-blur-xl mb-8 relative overflow-hidden">
        <div className="absolute -top-16 -right-16 w-40 h-40 bg-[#1DB954]/15 rounded-full blur-2xl pointer-events-none" />

        <div className="flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
          {user?.image ? (
            <img
              src={user.image}
              alt={user.name || "Kullanıcı"}
              className="w-24 h-24 rounded-full object-cover ring-4 ring-[#1DB954]/30 shadow-xl"
            />
          ) : (
            <div className="w-24 h-24 rounded-full bg-zinc-800 flex items-center justify-center text-3xl font-bold text-zinc-400 ring-4 ring-[#1DB954]/30">
              {user?.name ? user.name.charAt(0) : "U"}
            </div>
          )}

          <div className="flex-1">
            <h1 className="text-2xl sm:text-3xl font-bold text-white mb-1">
              {user?.name || "Kullanıcı Profili"}
            </h1>
            <p className="text-zinc-400 text-sm mb-4">{user?.email || "Hesap bağlı değil"}</p>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-[#1DB954]/10 border border-[#1DB954]/30 text-[#1DB954] text-xs font-semibold">
              <span className="w-1.5 h-1.5 rounded-full bg-[#1DB954]" />
              {token ? "Spotify Hesabı Doğrulandı" : "Giriş Yapılmadı"}
            </div>
          </div>

          <div>
            <button
              onClick={handleSync}
              disabled={syncing || !token}
              className="px-5 py-2.5 rounded-full text-sm font-semibold bg-[#1DB954] hover:bg-[#1ed760] disabled:opacity-50 text-black transition shadow-lg shadow-[#1DB954]/20 cursor-pointer"
            >
              {syncing ? "Senkronize Ediliyor..." : "Spotify Verilerini Yenile"}
            </button>
          </div>
        </div>

        {syncMessage && (
          <div className="mt-6 p-3 rounded-xl bg-zinc-800/80 border border-zinc-700/80 text-zinc-300 text-xs text-center animate-in fade-in">
            {syncMessage}
          </div>
        )}
      </div>

      {/* Dynamic Stats Cards */}
      <h2 className="text-xl font-bold text-white mb-4">Dinleme Analizleri</h2>
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
        <div className="p-5 rounded-2xl bg-zinc-900/40 border border-zinc-800">
          <div className="text-xs uppercase tracking-wider text-zinc-500 font-semibold mb-1">
            En Çok Dinlenen Sanatçı
          </div>
          <div className="text-lg font-bold text-white truncate">{topArtistName}</div>
          <div className="text-xs text-zinc-400 mt-1">Son dinleme analizine göre</div>
        </div>

        <div className="p-5 rounded-2xl bg-zinc-900/40 border border-zinc-800">
          <div className="text-xs uppercase tracking-wider text-zinc-500 font-semibold mb-1">
            Zirvedeki Parça
          </div>
          <div className="text-lg font-bold text-white truncate">{topTrackName}</div>
          <div className="text-xs text-zinc-400 mt-1">En yüksek frekanslı parça</div>
        </div>

        <div className="p-5 rounded-2xl bg-zinc-900/40 border border-zinc-800">
          <div className="text-xs uppercase tracking-wider text-zinc-500 font-semibold mb-1">
            Kayıtlı Parça Sayısı
          </div>
          <div className="text-lg font-bold text-[#1DB954]">
            {topTracks.length + recentHistory.length}
          </div>
          <div className="text-xs text-zinc-400 mt-1">Eşleştirme motorunda aktif</div>
        </div>
      </div>

      {/* Empty State when no tracks synced yet */}
      {recentHistory.length === 0 && topTracks.length === 0 && (
        <div className="mb-8 p-8 rounded-3xl bg-zinc-900/40 border border-zinc-800 text-center">
          <div className="text-3xl mb-3">🎵</div>
          <h3 className="text-base font-bold text-white mb-2">Henüz Senkronize Edilmiş Parça Yok</h3>
          <p className="text-xs text-zinc-400 max-w-md mx-auto leading-relaxed mb-4">
            Spotify&apos;daki son dinleme geçmişinizi ve en çok dinlediğiniz şarkıları sisteme yüklemek için yukarıdaki <strong className="text-[#1DB954]">&quot;Spotify Verilerini Yenile&quot;</strong> butonuna tıklayın.
          </p>
        </div>
      )}

      {/* Recently Played Section — Son 5 Şarkı */}
      {recentHistory.length > 0 && (
        <div className="mb-8">
          <div className="flex items-center gap-2 mb-3">
            <span className="text-lg">🕐</span>
            <h2 className="text-lg font-bold text-white">Son Dinlediğin 5 Şarkı</h2>
            <span className="ml-auto text-xs text-zinc-500">Spotify geçmişinden</span>
          </div>
          <div className="space-y-2">
            {recentHistory.slice(0, 5).map((item, idx) => {
              const playedAt = new Date(item.playedAt);
              const timeAgo = (() => {
                const diff = Date.now() - playedAt.getTime();
                const mins = Math.floor(diff / 60000);
                if (mins < 60) return `${mins} dk önce`;
                const hours = Math.floor(mins / 60);
                if (hours < 24) return `${hours} sa önce`;
                return `${Math.floor(hours / 24)} gün önce`;
              })();

              return (
                <div
                  key={item.id || idx}
                  onClick={() =>
                    setActivePreview({
                      id: item.track.spotifyId,
                      name: item.track.name,
                      artist: item.track.artists.join(", "),
                      imageUrl: item.track.imageUrl,
                      previewUrl: item.track.previewUrl,
                      spotifyUrl: item.track.url,
                    })
                  }
                  className="p-3 rounded-2xl bg-zinc-900/40 border border-zinc-800/80 hover:border-[#1DB954]/40 flex items-center gap-3 cursor-pointer transition group"
                >
                  <div className="flex items-center gap-3 min-w-0 flex-1">
                    <span className="text-xs font-bold text-zinc-600 w-4 group-hover:text-[#1DB954] transition">{idx + 1}</span>
                    {item.track.imageUrl ? (
                      <img
                        src={item.track.imageUrl}
                        alt={item.track.name}
                        className="w-10 h-10 rounded-lg object-cover shadow-md"
                      />
                    ) : (
                      <div className="w-10 h-10 rounded-lg bg-zinc-800 flex items-center justify-center text-sm">
                        🎵
                      </div>
                    )}
                    <div className="min-w-0 flex-1">
                      <div className="text-sm font-semibold text-white truncate">{item.track.name}</div>
                      <div className="text-xs text-zinc-400 truncate">{item.track.artists?.join(", ")}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 shrink-0">
                    <span className="text-xs text-zinc-600">{timeAgo}</span>
                    <button className="px-3 py-1 rounded-full text-xs font-semibold bg-zinc-800 text-zinc-400 group-hover:bg-[#1DB954]/20 group-hover:text-[#1DB954] transition">
                      ▶
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Top Tracks Section */}
      {topTracks.length > 0 && (
        <div className="mb-8">
          <h2 className="text-lg font-bold text-white mb-3">En Çok Dinlediğin Şarkılar</h2>
          <div className="space-y-2">
            {topTracks.slice(0, 5).map((item, idx) => (
              <div
                key={item.id || idx}
                onClick={() =>
                  setActivePreview({
                    id: item.track.spotifyId,
                    name: item.track.name,
                    artist: item.track.artists.join(", "),
                    imageUrl: item.track.imageUrl,
                    previewUrl: item.track.previewUrl,
                    spotifyUrl: item.track.url,
                  })
                }
                className="p-3 rounded-2xl bg-zinc-900/40 border border-zinc-800/80 hover:border-zinc-700 flex items-center justify-between gap-3 cursor-pointer transition"
              >
                <div className="flex items-center gap-3 min-w-0">
                  <span className="text-xs font-bold text-zinc-500 w-4">{idx + 1}</span>
                  {item.track.imageUrl ? (
                    <img
                      src={item.track.imageUrl}
                      alt={item.track.name}
                      className="w-10 h-10 rounded-lg object-cover"
                    />
                  ) : (
                    <div className="w-10 h-10 rounded-lg bg-zinc-800 flex items-center justify-center text-sm">
                      🎵
                    </div>
                  )}
                  <div className="min-w-0">
                    <div className="text-sm font-semibold text-white truncate">
                      {item.track.name}
                    </div>
                    <div className="text-xs text-zinc-400 truncate">
                      {item.track.artists?.join(", ")}
                    </div>
                  </div>
                </div>
                <button className="px-3 py-1 rounded-full text-xs font-semibold bg-[#1DB954]/20 text-[#1DB954] hover:bg-[#1DB954] hover:text-black transition">
                  Dinle ▶
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="text-center pt-4">
        <Link
          href="/matches"
          className="inline-flex items-center gap-2 text-sm font-semibold text-[#1DB954] hover:underline"
        >
          Benzer zevklere sahip dinleyicileri gör &rarr;
        </Link>
      </div>

      <TrackPreviewPlayer track={activePreview} onClose={() => setActivePreview(null)} />
    </div>
  );
}
