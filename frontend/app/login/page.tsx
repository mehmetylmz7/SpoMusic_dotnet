"use client";

import { useEffect, useState, Suspense } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { useAuth } from "../../context/AuthContext";

function LoginContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const { loginWithSpotify, exchangeCode } = useAuth();

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);
  const [isPremiumError, setIsPremiumError] = useState(false);

  useEffect(() => {
    const code = searchParams.get("code");
    const err = searchParams.get("error");

    if (err) {
      const decoded = decodeURIComponent(err);
      if (decoded.toLowerCase().includes("premium")) {
        setIsPremiumError(true);
        setError(null);
      } else {
        setError(decoded);
      }
    }

    if (code) {
      setLoading(true);
      exchangeCode(code).then((ok) => {
        setLoading(false);
        if (ok) {
          // Remove code from URL history for privacy
          window.history.replaceState({}, "", "/login");
          setSuccess("Giriş başarılı! Eşleşmelerinize yönlendiriliyorsunuz...");
          setTimeout(() => {
            router.push("/matches");
          }, 1000);
        } else {
          setError("Tek kullanımlık yetki kodu geçersiz veya süresi dolmuş.");
        }
      });
    }
  }, [searchParams, router, exchangeCode]);

  return (
    <div className="min-h-[calc(100vh-10rem)] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-md p-8 rounded-3xl bg-zinc-900/60 border border-zinc-800/80 backdrop-blur-xl shadow-2xl relative overflow-hidden">
        {/* Glow */}
        <div className="absolute -top-24 -right-24 w-48 h-48 bg-[#1DB954]/20 rounded-full blur-2xl pointer-events-none" />

        <div className="text-center mb-8">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-tr from-[#1DB954] to-emerald-400 flex items-center justify-center mx-auto mb-4 shadow-lg shadow-[#1DB954]/20">
            <svg className="w-8 h-8 text-black" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
            </svg>
          </div>
          <h2 className="text-2xl font-bold text-white mb-2">SpoMusic'e Bağlan</h2>
          <p className="text-zinc-400 text-sm">
            Müzik eşleşmelerinizi görmek için Spotify hesabınızla güvenle giriş yapın.
          </p>
        </div>

        {/* Premium Error Banner */}
        {isPremiumError && (
          <div className="mb-6 p-4 rounded-xl bg-amber-950/60 border border-amber-700/80 text-sm">
            <div className="flex items-start gap-3">
              <span className="text-2xl">🎵</span>
              <div>
                <p className="text-amber-300 font-semibold mb-1">Spotify Yetkilendirme Uyarısı</p>
                <p className="text-amber-400/80 text-xs leading-relaxed">
                  Spotify Web API erişimi için hesabınızın geliştirici panelinde kayıtlı ve gerekli izinlere sahip olması gerekebilir.
                </p>
              </div>
            </div>
          </div>
        )}

        {error && (
          <div className="mb-6 p-4 rounded-xl bg-red-950/50 border border-red-800/80 text-red-300 text-sm text-center">
            {error}
          </div>
        )}

        {success && (
          <div className="mb-6 p-4 rounded-xl bg-emerald-950/50 border border-emerald-800/80 text-emerald-300 text-sm text-center">
            {success}
          </div>
        )}

        <div className="space-y-4">
          <button
            onClick={loginWithSpotify}
            disabled={loading}
            className="w-full flex items-center justify-center gap-3 py-3.5 px-6 rounded-2xl font-bold text-black bg-[#1DB954] hover:bg-[#1ed760] transition-all transform hover:scale-[1.02] shadow-lg shadow-[#1DB954]/20 cursor-pointer disabled:opacity-50"
          >
            <svg className="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
            </svg>
            Spotify ile Giriş Yap
          </button>
        </div>


        <p className="mt-8 text-center text-xs text-zinc-500">
          Spotify verileriniz yalnızca müzik zevki analizi için kullanılır ve üçüncü taraflarla paylaşılmaz.
        </p>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center text-zinc-500">Yükleniyor...</div>}>
      <LoginContent />
    </Suspense>
  );
}
