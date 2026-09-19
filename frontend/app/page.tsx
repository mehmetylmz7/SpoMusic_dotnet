import Link from "next/link";

export default function Home() {
  return (
    <div className="flex flex-col items-center justify-center">
      {/* Hero Section */}
      <section className="relative w-full max-w-6xl mx-auto px-4 sm:px-6 pt-20 pb-16 md:pt-28 md:pb-24 text-center overflow-hidden">
        {/* Glow blur background effect */}
        <div className="absolute top-1/4 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-[#1DB954]/15 rounded-full blur-3xl pointer-events-none -z-10" />

        <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-zinc-900/80 border border-zinc-800 text-zinc-300 text-xs sm:text-sm font-medium mb-8 backdrop-blur">
          <span className="w-2 h-2 rounded-full bg-[#1DB954] animate-pulse" />
          Akıllı Müzik Eşleşme Algoritması v1.0
        </div>

        <h1 className="text-4xl sm:text-6xl lg:text-7xl font-extrabold tracking-tight text-white mb-6">
          Müzik Zevkinin{" "}
          <span className="bg-gradient-to-r from-[#1DB954] via-emerald-400 to-teal-300 bg-clip-text text-transparent">
            Ruh Eşini
          </span>{" "}
          Keşfet.
        </h1>

        <p className="max-w-2xl mx-auto text-lg sm:text-xl text-zinc-400 mb-10 leading-relaxed">
          SpoMusic, Spotify dinleme geçmişinizi, en çok dinlediğiniz sanatçıları ve güncel parçalarınızı analiz ederek sizinle aynı müzik frekansında olan insanları bulur.
        </p>

        <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
          <Link
            href="/login"
            className="w-full sm:w-auto inline-flex items-center justify-center gap-3 px-8 py-4 rounded-full font-bold text-base bg-[#1DB954] text-black hover:bg-[#1ed760] transition-all transform hover:-translate-y-0.5 shadow-xl shadow-[#1DB954]/25"
          >
            <svg className="w-6 h-6" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
            </svg>
            Spotify ile Bağlan & Başla
          </Link>
          <Link
            href="/matches"
            className="w-full sm:w-auto inline-flex items-center justify-center px-8 py-4 rounded-full font-semibold text-base bg-zinc-900 text-zinc-300 hover:text-white hover:bg-zinc-800 border border-zinc-800 transition-all"
          >
            Müzik Eşleşmelerine Git
          </Link>
        </div>
      </section>

      {/* Feature Highlights Grid */}
      <section className="w-full max-w-6xl mx-auto px-4 sm:px-6 py-16">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="p-8 rounded-3xl bg-zinc-900/40 border border-zinc-800/80 backdrop-blur-sm hover:border-zinc-700/80 transition-all">
            <div className="w-12 h-12 rounded-2xl bg-[#1DB954]/10 text-[#1DB954] flex items-center justify-center font-bold text-xl mb-6">
              40%
            </div>
            <h3 className="text-xl font-bold text-white mb-2">Ortak Parça Uyumu</h3>
            <p className="text-zinc-400 text-sm leading-relaxed">
              En sevdiğiniz şarkıların diğer kullanıcılarla kesişim kümesini O(min(m,n)) karmaşıklığında anında analiz eder.
            </p>
          </div>

          <div className="p-8 rounded-3xl bg-zinc-900/40 border border-zinc-800/80 backdrop-blur-sm hover:border-zinc-700/80 transition-all">
            <div className="w-12 h-12 rounded-2xl bg-emerald-500/10 text-emerald-400 flex items-center justify-center font-bold text-xl mb-6">
              25%
            </div>
            <h3 className="text-xl font-bold text-white mb-2">Sanatçı Benzerliği</h3>
            <p className="text-zinc-400 text-sm leading-relaxed">
              Farklı parçalar olsa dahi aynı sanatçıları takip eden dinleyicileri ortak zevk algoritmasıyla yakalar.
            </p>
          </div>

          <div className="p-8 rounded-3xl bg-zinc-900/40 border border-zinc-800/80 backdrop-blur-sm hover:border-zinc-700/80 transition-all">
            <div className="w-12 h-12 rounded-2xl bg-teal-500/10 text-teal-400 flex items-center justify-center font-bold text-xl mb-6">
              20%
            </div>
            <h3 className="text-xl font-bold text-white mb-2">Dinleme Güncelliği</h3>
            <p className="text-zinc-400 text-sm leading-relaxed">
              Bugün ve son 30 gün içinde aynı şarkıları aynı dönemde dinleyen kişilere ekstra uyumluluk puanı verir.
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
