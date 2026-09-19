"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "../context/AuthContext";

const navLinks = [
  { href: "/", label: "Keşfet" },
  { href: "/matches", label: "Eşleşmelerim" },
  { href: "/profile", label: "Profilim" },
  { href: "/settings", label: "Ayarlar" },
];

export default function Navbar() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const pathname = usePathname();
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <header className="sticky top-0 z-50 backdrop-blur-md bg-[#0f0f12]/80 border-b border-zinc-800/80">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between">
        {/* Logo */}
        <Link href="/" className="flex items-center gap-2.5 group">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-[#1DB954] to-emerald-400 flex items-center justify-center shadow-lg shadow-[#1DB954]/20 group-hover:scale-105 transition-transform">
            <svg className="w-5 h-5 text-black" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
            </svg>
          </div>
          <span className="font-bold text-xl tracking-tight bg-gradient-to-r from-white to-zinc-400 bg-clip-text text-transparent">
            SpoMusic
          </span>
        </Link>

        {/* Desktop Nav Links */}
        <nav className="hidden md:flex items-center gap-1 bg-zinc-900/60 p-1 rounded-full border border-zinc-800">
          {navLinks.map((link) => {
            const isActive = pathname === link.href;
            return (
              <Link
                key={link.href}
                href={link.href}
                className={`px-4 py-1.5 rounded-full text-sm font-medium transition-all ${
                  isActive
                    ? "bg-zinc-800 text-white shadow-sm"
                    : "text-zinc-400 hover:text-white hover:bg-zinc-800/70"
                }`}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>

        {/* Desktop User Action */}
        <div className="hidden md:flex items-center gap-3">
          {isAuthenticated && user ? (
            <div className="flex items-center gap-3">
              <Link
                href="/profile"
                className="flex items-center gap-2 p-1.5 pr-3 rounded-full bg-zinc-900/80 border border-zinc-800 hover:border-zinc-700 transition"
              >
                {user.image ? (
                  <img
                    src={user.image}
                    alt={user.name || "Profil"}
                    className="w-7 h-7 rounded-full object-cover"
                  />
                ) : (
                  <div className="w-7 h-7 rounded-full bg-zinc-800 flex items-center justify-center text-xs font-bold text-zinc-300">
                    {user.name ? user.name.charAt(0) : "U"}
                  </div>
                )}
                <span className="text-xs font-medium text-zinc-200 truncate max-w-[120px]">
                  {user.name || "Kullanıcı"}
                </span>
              </Link>
              <button
                onClick={logout}
                className="px-3 py-1.5 rounded-full text-xs font-medium text-zinc-400 hover:text-red-400 hover:bg-zinc-900 transition border border-transparent hover:border-red-900/40 cursor-pointer"
              >
                Çıkış Yap
              </button>
            </div>
          ) : (
            <Link
              href="/login"
              className="inline-flex items-center gap-2 px-4 py-2 rounded-full text-sm font-semibold bg-[#1DB954] text-black hover:bg-[#1ed760] transition-colors shadow-md shadow-[#1DB954]/20"
            >
              Giriş Yap
            </Link>
          )}
        </div>

        {/* Mobile Hamburger Button */}
        <div className="md:hidden flex items-center gap-2">
          {isAuthenticated && user && (
            <Link href="/profile" className="flex items-center">
              {user.image ? (
                <img
                  src={user.image}
                  alt={user.name || "Profil"}
                  className="w-8 h-8 rounded-full object-cover ring-1 ring-zinc-700"
                />
              ) : (
                <div className="w-8 h-8 rounded-full bg-zinc-800 flex items-center justify-center text-xs font-bold text-zinc-300">
                  {user.name ? user.name.charAt(0) : "U"}
                </div>
              )}
            </Link>
          )}

          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="p-2 rounded-xl bg-zinc-900 border border-zinc-800 text-zinc-300 hover:text-white transition cursor-pointer"
            aria-label="Menüyü Aç/Kapat"
          >
            {mobileMenuOpen ? (
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            ) : (
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            )}
          </button>
        </div>
      </div>

      {/* Mobile Drawer */}
      {mobileMenuOpen && (
        <div className="md:hidden border-b border-zinc-800 bg-[#0f0f12]/95 backdrop-blur-xl px-4 py-4 animate-in slide-in-from-top duration-200">
          <nav className="flex flex-col gap-1 mb-4">
            {navLinks.map((link) => {
              const isActive = pathname === link.href;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  onClick={() => setMobileMenuOpen(false)}
                  className={`px-4 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    isActive
                      ? "bg-zinc-800 text-white font-semibold"
                      : "text-zinc-400 hover:text-white hover:bg-zinc-900"
                  }`}
                >
                  {link.label}
                </Link>
              );
            })}
          </nav>

          <div className="pt-3 border-t border-zinc-800 flex flex-col gap-2">
            {isAuthenticated ? (
              <button
                onClick={() => {
                  setMobileMenuOpen(false);
                  logout();
                }}
                className="w-full py-2.5 px-4 rounded-xl text-sm font-semibold text-red-400 bg-red-950/20 hover:bg-red-950/40 border border-red-900/40 transition text-center"
              >
                Çıkış Yap
              </button>
            ) : (
              <Link
                href="/login"
                onClick={() => setMobileMenuOpen(false)}
                className="w-full py-2.5 px-4 rounded-xl text-sm font-semibold text-black bg-[#1DB954] hover:bg-[#1ed760] transition text-center"
              >
                Giriş Yap
              </Link>
            )}
          </div>
        </div>
      )}
    </header>
  );
}
