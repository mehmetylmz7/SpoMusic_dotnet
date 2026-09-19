import "./globals.css";
import Link from "next/link";
import type { Metadata } from "next";
import Navbar from "../components/Navbar";
import { AuthProvider } from "../context/AuthContext";

export const metadata: Metadata = {
  title: "SpoMusic | Müzik Eşleşme ve Keşif Platformu",
  description: "Spotify dinleme alışkanlıklarınızı analiz ederek müzik zevkinizin uyuştuğu insanları bulun.",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="tr" className="dark">
      <body className="min-h-screen flex flex-col bg-[#0f0f12] text-zinc-100 antialiased selection:bg-[#1DB954] selection:text-black">
        <AuthProvider>
          {/* Navigation Bar */}
          <Navbar />

          {/* Page Content */}
          <main className="flex-1 flex flex-col">{children}</main>
        </AuthProvider>


        {/* Footer */}
        <footer className="border-t border-zinc-800/80 bg-zinc-950/60 py-8 text-zinc-500 text-sm">
          <div className="max-w-6xl mx-auto px-4 sm:px-6 flex flex-col sm:flex-row items-center justify-between gap-4 text-center sm:text-left">
            <div className="flex items-center gap-2">
              <div className="w-5 h-5 rounded-full bg-[#1DB954]/20 flex items-center justify-center">
                <div className="w-2 h-2 rounded-full bg-[#1DB954]" />
              </div>
              <span>&copy; {new Date().getFullYear()} SpoMusic. Spotify API ile güçlendirilmiştir.</span>
            </div>
            <div className="flex gap-6 text-xs text-zinc-400">
              <Link href="/matches" className="hover:text-white transition">Eşleşmeler</Link>
              <Link href="/profile" className="hover:text-white transition">Profil</Link>
              <Link href="/settings" className="hover:text-white transition">Gizlilik</Link>
            </div>
          </div>
        </footer>
      </body>
    </html>
  );
}