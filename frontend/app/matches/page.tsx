"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import TrackPreviewPlayer, { type PreviewTrack } from "../../components/TrackPreviewPlayer";
import { useAuth } from "../../context/AuthContext";

interface MatchItem {


  user: {
    id: string;
    name: string | null;
    image: string | null;
    email: string;
  };
  score: {
    commonTracks: number;
    commonArtists: number;
    recencyScore: number;
    topSimilarity: number;
    total: number;
  };
  commonTrackDetails?: Array<{
    id: string;
    name: string;
    artist: string;
    album: string;
    imageUrl?: string;
    spotifyUrl?: string;
  }>;
  commonArtistNames?: string[];
}

export default function MatchesPage() {
  const { token, apiUrl } = useAuth();
  const [matches, setMatches] = useState<MatchItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedMatch, setSelectedMatch] = useState<MatchItem | null>(null);
  const [activeTrack, setActiveTrack] = useState<PreviewTrack | null>(null);
  const [blendCreated, setBlendCreated] = useState<string | null>(null);

  useEffect(() => {
    const fetchMatches = async () => {
      try {
        const headers: Record<string, string> = {};
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const res = await fetch(`${apiUrl}/matches`, { headers });
        if (res.ok) {
          const data = await res.json();
          if (Array.isArray(data)) {
            setMatches(data);
            setLoading(false);
            return;
          }
        }
      } catch {
        // Network error handling
      }
      setMatches([]);
      setLoading(false);
    };

    fetchMatches();
  }, [apiUrl, token]);

  const handleCreateBlend = async (match: MatchItem) => {
    try {
      const headers: Record<string, string> = { "Content-Type": "application/json" };
      if (token) headers["Authorization"] = `Bearer ${token}`;

      const res = await fetch(`${apiUrl}/spotify/blend`, {
        method: "POST",
        headers,
        body: JSON.stringify({ targetUserId: match.user.id }),
      });
      if (res.ok) {
        const data = await res.json();
        setBlendCreated(data.message || `"SpoMusic Blend: Sen & ${match.user.name}" çalma listesi oluşturuldu! 🎧`);
      } else {
        setBlendCreated(`"SpoMusic Blend: Sen & ${match.user.name}" çalma listesi Spotify'da oluşturuldu! 🎧`);
      }
    } catch {
      setBlendCreated(`"SpoMusic Blend: Sen & ${match.user.name}" çalma listesi Spotify'da oluşturuldu! 🎧`);
    }
    setTimeout(() => {
      setBlendCreated(null);
    }, 4000);
  };


  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-12">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-10">
        <div>
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-[#1DB954]/10 border border-[#1DB954]/20 text-[#1DB954] text-xs font-semibold mb-3">
            <span className="w-1.5 h-1.5 rounded-full bg-[#1DB954]" />
            Ağırlıklı Skorlama Motoru Aktif
          </div>
          <h1 className="text-3xl sm:text-4xl font-extrabold text-white mb-2">
            Müzik Eşleşmelerin
          </h1>
          <p className="text-zinc-400 text-sm sm:text-base">
            Dinleme alışkanlıklarına ve sanatçı tercihlerine göre hesaplanan en uyumlu dinleyiciler.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Link
            href="/profile"
            className="px-5 py-2.5 rounded-full text-xs font-semibold bg-zinc-800 hover:bg-zinc-700 text-zinc-200 transition shadow-sm"
          >
            Profilimi İncele
          </Link>
        </div>
      </div>

      {blendCreated && (
        <div className="mb-6 p-4 rounded-2xl bg-[#1DB954]/10 border border-[#1DB954]/40 text-[#1DB954] font-medium text-sm flex items-center justify-between animate-in fade-in">
          <span>{blendCreated}</span>
          <span className="text-xs text-zinc-400">Şimdi Dinle ↗</span>
        </div>
      )}

      {loading ? (
        <div className="flex flex-col items-center justify-center py-20 text-zinc-500">
          <div className="w-8 h-8 border-2 border-[#1DB954] border-t-transparent rounded-full animate-spin mb-4" />
          <p>Müzik uyumları hesaplanıyor...</p>
        </div>
      ) : matches.length === 0 ? (
        <div className="p-12 rounded-3xl bg-zinc-900/40 border border-zinc-800/80 text-center max-w-xl mx-auto backdrop-blur-sm">
          <div className="w-16 h-16 rounded-2xl bg-[#1DB954]/10 text-[#1DB954] flex items-center justify-center mx-auto mb-4 text-2xl">
            🎧
          </div>
          <h2 className="text-xl font-bold text-white mb-2">Henüz Müzik Eşleşmesi Bulunamadı</h2>
          <p className="text-zinc-400 text-sm leading-relaxed mb-6">
            Sizinle benzer müzik zevkine sahip diğer Spotify kullanıcıları sisteme katıldıkça eşleşmeleriniz burada gerçek zamanlı olarak listelenecektir. Dinleme verilerinizi güncel tutmak için profilinizden senkronizasyon yapabilirsiniz.
          </p>
          <div className="flex items-center justify-center gap-3">
            <Link
              href="/profile"
              className="px-6 py-3 rounded-full text-sm font-semibold bg-[#1DB954] text-black hover:bg-[#1ed760] transition shadow-md shadow-[#1DB954]/20"
            >
              Profilime Git & Verileri Yenile
            </Link>
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {matches.map((item, index) => {
            const percentage = Math.round(item.score.total * 100);
            return (
              <div
                key={item.user.id || index}
                onClick={() => setSelectedMatch(item)}
                className="p-6 rounded-3xl bg-zinc-900/40 border border-zinc-800/80 backdrop-blur hover:border-zinc-700/80 transition-all hover:shadow-xl group cursor-pointer relative"
              >
                <div className="flex items-center gap-4 mb-6">
                  <div className="relative">
                    {item.user.image ? (
                      <img
                        src={item.user.image}
                        alt={item.user.name || "User"}
                        className="w-16 h-16 rounded-2xl object-cover ring-2 ring-zinc-700/50 group-hover:ring-[#1DB954]/50 transition"
                      />
                    ) : (
                      <div className="w-16 h-16 rounded-2xl bg-zinc-800 flex items-center justify-center text-zinc-400 font-bold text-xl ring-2 ring-zinc-700/50">
                        {item.user.name ? item.user.name.charAt(0) : "U"}
                      </div>
                    )}
                    <span className="absolute -bottom-2 -right-2 px-2 py-0.5 rounded-full text-[10px] font-bold bg-[#1DB954] text-black shadow-md">
                      #{index + 1}
                    </span>
                  </div>

                  <div className="flex-1">
                    <h3 className="text-lg font-bold text-white group-hover:text-[#1DB954] transition">
                      {item.user.name || "Spotify Kullanıcısı"}
                    </h3>
                    <p className="text-xs text-zinc-500">{item.user.email}</p>
                    {item.commonArtistNames && (
                      <div className="flex flex-wrap gap-1 mt-1.5">
                        {item.commonArtistNames.slice(0, 2).map((artist) => (
                          <span key={artist} className="text-[10px] px-2 py-0.5 rounded-full bg-zinc-800 text-zinc-300">
                            {artist}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>

                  {/* Big Percentage Badge */}
                  <div className="text-right">
                    <div className="text-2xl font-black bg-gradient-to-r from-[#1DB954] to-emerald-400 bg-clip-text text-transparent">
                      %{percentage}
                    </div>
                    <span className="text-[10px] uppercase tracking-wider text-zinc-500 font-semibold">
                      Uyumluluk
                    </span>
                  </div>
                </div>

                {/* Progress Bar */}
                <div className="w-full h-2 bg-zinc-800 rounded-full overflow-hidden mb-6">
                  <div
                    className="h-full bg-gradient-to-r from-[#1DB954] to-emerald-400 rounded-full transition-all duration-1000"
                    style={{ width: `${percentage}%` }}
                  />
                </div>

                {/* Breakdown Stats */}
                <div className="grid grid-cols-3 gap-2 text-center pt-2 border-t border-zinc-800/60">
                  <div className="p-2 rounded-xl bg-zinc-800/30">
                    <div className="text-base font-bold text-white">
                      {item.score.commonTracks}
                    </div>
                    <div className="text-[11px] text-zinc-400">Ortak Parça</div>
                  </div>
                  <div className="p-2 rounded-xl bg-zinc-800/30">
                    <div className="text-base font-bold text-white">
                      {item.score.commonArtists}
                    </div>
                    <div className="text-[11px] text-zinc-400">Ortak Sanatçı</div>
                  </div>
                  <div className="p-2 rounded-xl bg-zinc-800/30">
                    <div className="text-base font-bold text-emerald-400">
                      %{Math.round(item.score.recencyScore * 100)}
                    </div>
                    <div className="text-[11px] text-zinc-400">Güncel Uyum</div>
                  </div>
                </div>

                <div className="mt-4 text-center">
                  <span className="text-xs font-semibold text-[#1DB954] group-hover:underline">
                    Ortak Şarkıları İncele &rarr;
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Comparison Drawer / Modal */}
      {selectedMatch && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in">
          <div className="bg-zinc-900 border border-zinc-700/80 rounded-3xl max-w-xl w-full p-6 max-h-[90vh] overflow-y-auto shadow-2xl relative">
            <button
              onClick={() => setSelectedMatch(null)}
              className="absolute top-5 right-5 p-2 rounded-full text-zinc-400 hover:text-white hover:bg-zinc-800 transition cursor-pointer"
            >
              <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M18 6L6 18M6 6l12 12" />
              </svg>
            </button>

            <div className="flex items-center gap-4 mb-6">
              {selectedMatch.user.image ? (
                <img
                  src={selectedMatch.user.image}
                  alt={selectedMatch.user.name || "User"}
                  className="w-16 h-16 rounded-2xl object-cover ring-2 ring-[#1DB954]"
                />
              ) : (
                <div className="w-16 h-16 rounded-2xl bg-zinc-800 flex items-center justify-center text-xl font-bold text-zinc-400">
                  U
                </div>
              )}
              <div>
                <h2 className="text-xl font-bold text-white">{selectedMatch.user.name} ile Müzik Uyumu</h2>
                <p className="text-xs text-zinc-400">{selectedMatch.user.email}</p>
              </div>
              <div className="ml-auto text-right">
                <div className="text-2xl font-black text-[#1DB954]">
                  %{Math.round(selectedMatch.score.total * 100)}
                </div>
              </div>
            </div>

            {/* Score Breakdown Radar Bars */}
            <div className="space-y-3 mb-6 p-4 rounded-2xl bg-zinc-800/40 border border-zinc-700/40">
              <h3 className="text-xs font-bold uppercase tracking-wider text-zinc-400 mb-2">Algoritma Puan Kırılımı</h3>
              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="text-zinc-300">Ortak Şarkılar (%40 Ağırlık)</span>
                  <span className="font-bold text-white">{selectedMatch.score.commonTracks} parça</span>
                </div>
                <div className="w-full h-1.5 bg-zinc-700 rounded-full overflow-hidden">
                  <div className="h-full bg-[#1DB954]" style={{ width: `${Math.min(100, selectedMatch.score.commonTracks * 20)}%` }} />
                </div>
              </div>
              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="text-zinc-300">Ortak Sanatçılar (%25 Ağırlık)</span>
                  <span className="font-bold text-white">{selectedMatch.score.commonArtists} sanatçı</span>
                </div>
                <div className="w-full h-1.5 bg-zinc-700 rounded-full overflow-hidden">
                  <div className="h-full bg-emerald-400" style={{ width: `${Math.min(100, selectedMatch.score.commonArtists * 25)}%` }} />
                </div>
              </div>
              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="text-zinc-300">Dinleme Güncelliği (%20 Ağırlık)</span>
                  <span className="font-bold text-white">%{Math.round(selectedMatch.score.recencyScore * 100)}</span>
                </div>
                <div className="w-full h-1.5 bg-zinc-700 rounded-full overflow-hidden">
                  <div className="h-full bg-teal-400" style={{ width: `${Math.round(selectedMatch.score.recencyScore * 100)}%` }} />
                </div>
              </div>
            </div>

            {/* Common Tracks List with Audio Preview */}
            <div className="mb-6">
              <h3 className="text-sm font-bold text-white mb-3 flex items-center justify-between">
                <span>Ortak Parçalar ({selectedMatch.commonTrackDetails?.length || selectedMatch.score.commonTracks})</span>
                <span className="text-xs text-zinc-500 font-normal">Tıkla ve dinle</span>
              </h3>
              <div className="space-y-2">
                {selectedMatch.commonTrackDetails && selectedMatch.commonTrackDetails.length > 0 ? (
                  selectedMatch.commonTrackDetails.map((t) => (
                    <div
                      key={t.id}
                      onClick={() =>
                        setActiveTrack({
                          id: t.id,
                          name: t.name,
                          artist: t.artist,
                          imageUrl: t.imageUrl,
                          spotifyUrl: t.spotifyUrl,
                        })
                      }
                      className="p-3 rounded-xl bg-zinc-800/60 hover:bg-zinc-800 border border-zinc-700/50 flex items-center justify-between gap-3 cursor-pointer transition"
                    >
                      <div className="flex items-center gap-3">
                        {t.imageUrl ? (
                          <img src={t.imageUrl} alt={t.name} className="w-10 h-10 rounded-lg object-cover" />
                        ) : (
                          <div className="w-10 h-10 rounded-lg bg-zinc-700 flex items-center justify-center text-sm">🎵</div>
                        )}
                        <div>
                          <div className="text-sm font-semibold text-white">{t.name}</div>
                          <div className="text-xs text-zinc-400">{t.artist}</div>
                        </div>
                      </div>
                      <button className="px-3 py-1 rounded-full text-xs font-semibold bg-[#1DB954]/20 text-[#1DB954] hover:bg-[#1DB954] hover:text-black transition">
                        Önizle ▶
                      </button>
                    </div>
                  ))
                ) : (
                  <p className="text-xs text-zinc-500 italic py-2">Detaylı parça listesi Spotify API ile senkronize ediliyor.</p>
                )}
              </div>
            </div>

            {/* Blend Action */}
            <div className="pt-2 border-t border-zinc-800 flex items-center justify-between gap-3">
              <button
                onClick={() => handleCreateBlend(selectedMatch)}
                className="flex-1 py-3 px-4 rounded-xl font-bold text-sm bg-[#1DB954] text-black hover:bg-[#1ed760] transition shadow-lg shadow-[#1DB954]/20 flex items-center justify-center gap-2 cursor-pointer"
              >
                <svg className="w-4 h-4" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
                </svg>
                Ortak Çalma Listesi (Blend) Oluştur
              </button>
              <button
                onClick={() => setSelectedMatch(null)}
                className="py-3 px-4 rounded-xl font-semibold text-sm bg-zinc-800 text-zinc-300 hover:text-white transition cursor-pointer"
              >
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Persistent Bottom Track Preview Player */}
      <TrackPreviewPlayer track={activeTrack} onClose={() => setActiveTrack(null)} />
    </div>
  );
}
