"use client";

import { useState, useRef, useEffect } from "react";

export interface PreviewTrack {
  id: string;
  name: string;
  artist: string;
  album?: string;
  imageUrl?: string | null;
  spotifyUrl?: string;
  previewUrl?: string | null;
}

interface TrackPreviewPlayerProps {
  track: PreviewTrack | null;
  onClose?: () => void;
}

export default function TrackPreviewPlayer({ track, onClose }: TrackPreviewPlayerProps) {
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(30);
  const [hasError, setHasError] = useState(false);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  useEffect(() => {
    if (!track) {
      setIsPlaying(false);
      setCurrentTime(0);
      return;
    }

    setHasError(false);
    setCurrentTime(0);

    if (track.previewUrl && audioRef.current) {
      audioRef.current.currentTime = 0;
      audioRef.current
        .play()
        .then(() => setIsPlaying(true))
        .catch(() => {
          setIsPlaying(false);
        });
    } else {
      setIsPlaying(false);
    }
  }, [track]);

  const togglePlay = () => {
    if (!audioRef.current || !track?.previewUrl) return;

    if (isPlaying) {
      audioRef.current.pause();
      setIsPlaying(false);
    } else {
      audioRef.current
        .play()
        .then(() => setIsPlaying(true))
        .catch(() => setHasError(true));
    }
  };

  const handleTimeUpdate = () => {
    if (audioRef.current) {
      setCurrentTime(audioRef.current.currentTime);
      if (audioRef.current.duration) {
        setDuration(audioRef.current.duration);
      }
    }
  };

  const handleEnded = () => {
    setIsPlaying(false);
    setCurrentTime(0);
  };

  if (!track) return null;

  const progressPercent = duration > 0 ? (currentTime / duration) * 100 : 0;

  return (
    <div className="fixed bottom-4 left-1/2 -translate-x-1/2 z-50 w-full max-w-lg px-4 transition-all duration-300 animate-in fade-in slide-in-from-bottom-5">
      {track.previewUrl && (
        <audio
          ref={audioRef}
          src={track.previewUrl}
          onTimeUpdate={handleTimeUpdate}
          onEnded={handleEnded}
          onError={() => setHasError(true)}
        />
      )}

      <div className="p-3.5 rounded-2xl bg-zinc-900/95 border border-zinc-700/80 backdrop-blur-xl shadow-2xl flex flex-col gap-2">
        <div className="flex items-center justify-between gap-3">
          {/* Track Thumbnail & Details */}
          <div className="flex items-center gap-3 min-w-0">
            {track.imageUrl ? (
              <img
                src={track.imageUrl}
                alt={track.name}
                className="w-12 h-12 rounded-xl object-cover ring-1 ring-zinc-700 flex-shrink-0"
              />
            ) : (
              <div className="w-12 h-12 rounded-xl bg-zinc-800 flex items-center justify-center text-zinc-500 font-bold flex-shrink-0">
                🎵
              </div>
            )}

            <div className="min-w-0">
              <h4 className="text-sm font-bold text-white truncate">{track.name}</h4>
              <p className="text-xs text-zinc-400 truncate">{track.artist}</p>
            </div>
          </div>

          {/* Controls & Animated Equalizer */}
          <div className="flex items-center gap-3 flex-shrink-0">
            {track.previewUrl ? (
              <button
                onClick={togglePlay}
                className="w-9 h-9 rounded-full bg-[#1DB954] text-black hover:bg-[#1ed760] transition flex items-center justify-center shadow-md shadow-[#1DB954]/20 cursor-pointer"
                title={isPlaying ? "Durdur" : "Oynat"}
              >
                {isPlaying ? (
                  <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
                    <path d="M6 4h4v16H6V4zm8 0h4v16h-4V4z" />
                  </svg>
                ) : (
                  <svg className="w-4 h-4 fill-current ml-0.5" viewBox="0 0 24 24">
                    <path d="M8 5v14l11-7z" />
                  </svg>
                )}
              </button>
            ) : (
              <span className="text-[10px] px-2 py-0.5 rounded bg-zinc-800 text-zinc-400">
                Önizleme Yok
              </span>
            )}

            {/* Equalizer animation active only during playback */}
            <div className="flex items-center gap-0.5 h-4 px-1">
              <span
                className={`w-0.5 bg-[#1DB954] rounded-full transition-all ${
                  isPlaying ? "h-3 animate-pulse" : "h-1 opacity-40"
                }`}
              />
              <span
                className={`w-0.5 bg-[#1DB954] rounded-full transition-all ${
                  isPlaying ? "h-4 animate-pulse delay-75" : "h-1 opacity-40"
                }`}
              />
              <span
                className={`w-0.5 bg-[#1DB954] rounded-full transition-all ${
                  isPlaying ? "h-2 animate-pulse delay-150" : "h-1 opacity-40"
                }`}
              />
            </div>

            {/* Open in Spotify Button */}
            {track.spotifyUrl && (
              <a
                href={track.spotifyUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="px-3 py-1.5 rounded-full text-xs font-semibold bg-zinc-800 text-zinc-200 hover:bg-zinc-700 hover:text-white transition flex items-center gap-1.5"
              >
                <svg className="w-3.5 h-3.5 text-[#1DB954]" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.58 14.42c-.18.3-.57.4-.87.21-2.4-1.47-5.43-1.8-9-1-.34.08-.68-.13-.76-.47-.08-.34.13-.68.47-.76 3.92-.89 7.28-.52 9.95 1.15.3.19.4.58.21.87zm1.22-2.72c-.23.37-.72.49-1.09.26-2.75-1.69-6.94-2.18-10.19-1.19-.42.13-.86-.11-.99-.53-.13-.42.11-.86.53-.99 3.71-1.13 8.35-.58 11.48 1.36.38.23.49.72.26 1.09zm.11-2.83C14.62 8.9 9.17 8.72 6.01 9.68c-.5.15-1.04-.13-1.19-.63-.15-.5.13-1.04.63-1.19 3.66-1.11 9.69-.9 13.5 1.36.46.27.61.87.34 1.33-.27.46-.87.61-1.38.32z" />
                </svg>
                <span>Spotify</span>
              </a>
            )}

            {/* Close Button */}
            {onClose && (
              <button
                onClick={onClose}
                className="p-1 rounded-full text-zinc-400 hover:text-white hover:bg-zinc-800 transition cursor-pointer"
                title="Kapat"
              >
                <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M18 6L6 18M6 6l12 12" />
                </svg>
              </button>
            )}
          </div>
        </div>

        {/* Playback progress bar for previews */}
        {track.previewUrl && (
          <div className="w-full bg-zinc-800 h-1 rounded-full overflow-hidden">
            <div
              className="bg-[#1DB954] h-full transition-all duration-200"
              style={{ width: `${progressPercent}%` }}
            />
          </div>
        )}
      </div>
    </div>
  );
}
