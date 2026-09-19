// Audio preview player controller
let currentAudio = null;

function playPreview(url, name, artist, imgUrl) {
  if (!url) {
    showToast("Bu şarkı için Spotify 30 saniyelik önizleme sağlamıyor.");
    return;
  }

  const playerBar = document.getElementById("playerBar");
  const playerThumb = document.getElementById("playerThumb");
  const playerName = document.getElementById("playerName");
  const playerArtist = document.getElementById("playerArtist");
  const playIcon = document.getElementById("playIcon");

  if (currentAudio) {
    currentAudio.pause();
  }

  currentAudio = new Audio(url);
  currentAudio.play();

  if (playerBar) {
    playerBar.style.display = "flex";
    if (playerName) playerName.innerText = name;
    if (playerArtist) playerArtist.innerText = artist;
    if (playerThumb && imgUrl) playerThumb.src = imgUrl;
    if (playIcon) playIcon.innerText = "⏸";
  }

  currentAudio.onended = () => {
    if (playIcon) playIcon.innerText = "▶";
  };
}

function toggleAudio() {
  const playIcon = document.getElementById("playIcon");
  if (!currentAudio) return;

  if (currentAudio.paused) {
    currentAudio.play();
    if (playIcon) playIcon.innerText = "⏸";
  } else {
    currentAudio.pause();
    if (playIcon) playIcon.innerText = "▶";
  }
}

function showToast(message) {
  const toast = document.getElementById("toastBox");
  if (!toast) return;
  toast.innerText = message;
  toast.style.display = "block";
  setTimeout(() => {
    toast.style.display = "none";
  }, 4000);
}

async function createBlend(targetUserId, userName) {
  try {
    const res = await fetch("/matches/blend", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ targetUserId: targetUserId })
    });
    const data = await res.json();
    if (data.success) {
      showToast(data.message || `SpoMusic Blend: ${userName} ile çalma listesi hazırlandı! 🎧`);
    } else {
      showToast(data.message || "Blend oluşturulamadı. Lütfen giriş yapın.");
    }
  } catch (err) {
    showToast("Bir hata oluştu.");
  }
}
