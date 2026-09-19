"use client";

import React, { createContext, useContext, useState, useEffect, useCallback } from "react";
import { useRouter } from "next/navigation";

export interface UserProfile {
  id: string;
  email: string;
  name: string | null;
  image: string | null;
  spotifyId?: string | null;
  spotifyToken?: {
    expiresAt: string;
    createdAt: string;
  } | null;
}

interface AuthContextType {
  user: UserProfile | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  loginWithSpotify: () => void;
  exchangeCode: (code: string) => Promise<boolean>;
  logout: () => void;
  refreshUser: () => Promise<void>;
  apiUrl: string;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const TOKEN_KEY = "spomusic_token";
const USER_KEY = "spomusic_user";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "";

  // Fetch current user details from API using JWT token
  const fetchCurrentUser = useCallback(async (jwtToken: string): Promise<UserProfile | null> => {
    try {
      const res = await fetch(`${apiUrl}/auth/me`, {
        headers: {
          Authorization: `Bearer ${jwtToken}`,
        },
      });

      if (res.ok) {
        const userData = await res.json();
        setUser(userData);
        localStorage.setItem(USER_KEY, JSON.stringify(userData));
        return userData;
      } else {
        // Token expired or invalid
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        setToken(null);
        setUser(null);
        return null;
      }
    } catch {
      // Network error / offline fallback
      const cached = localStorage.getItem(USER_KEY);
      if (cached) {
        try {
          const parsed = JSON.parse(cached);
          setUser(parsed);
          return parsed;
        } catch {
          // ignore
        }
      }
      return null;
    }
  }, [apiUrl]);

  // Initial load
  useEffect(() => {
    const savedToken = localStorage.getItem(TOKEN_KEY);
    if (savedToken) {
      setToken(savedToken);
      fetchCurrentUser(savedToken).finally(() => setIsLoading(false));
    } else {
      setIsLoading(false);
    }
  }, [fetchCurrentUser]);

  const loginWithSpotify = () => {
    window.location.href = `${apiUrl}/auth/spotify`;
  };

  const exchangeCode = async (code: string): Promise<boolean> => {
    setIsLoading(true);
    try {
      const res = await fetch(`${apiUrl}/auth/exchange`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ code }),
      });

      if (!res.ok) {
        throw new Error("Exchange failed");
      }

      const data = await res.json();
      if (data.token && data.user) {
        setToken(data.token);
        setUser(data.user);
        localStorage.setItem(TOKEN_KEY, data.token);
        localStorage.setItem(USER_KEY, JSON.stringify(data.user));
        return true;
      }
      return false;
    } catch (err) {
      console.error("Authorization exchange failed:", err);
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem("spomusic_userId");
    setToken(null);
    setUser(null);
    router.push("/login");
  };

  const refreshUser = async () => {
    if (token) {
      await fetchCurrentUser(token);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        isAuthenticated: !!token && !!user,
        loginWithSpotify,
        exchangeCode,
        logout,
        refreshUser,
        apiUrl,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
