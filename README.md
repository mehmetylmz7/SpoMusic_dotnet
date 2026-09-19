# SpoMusic (.NET 9 & Docker)

SpoMusic, kullanıcıların Spotify dinleme geçmişini ve en çok dinlediği parçaları analiz ederek müzik zevkleri en uyumlu dinleyicileri keşfetmesini ve ortak Blend çalma listeleri oluşturmasını sağlayan modern bir müzik eşleşme platformudur.

Bu depo, projenin **.NET 9 (C#)** mimarisine taşınmış, konteynerize edilmiş ve uçtan uca test edilmiş halini içerir.

---

## 🏗️ Mimari ve Teknolojiler

- **Backend:** .NET 9 ASP.NET Core Web API (`SpoMusic.Api`)
  - RESTful API mimarisi
  - JWT Authentication & Spotify OAuth 2.0 State/CSRF korumalı One-Time Code takası
  - Entity Framework Core 9 (PostgreSQL Npgsql sağlayıcısı)
  - Otomatik Migration & Seed veri yükleme
  - Spotify Web API istemcisi (Profil, Son Dinlenenler, En Çok Dinlenenler, Token Refresh, Blend)
  - Ağırlıklı Uyumluluk Algoritması (%40 Ortak Şarkı, %25 Sanatçı Benzerliği, %20 Dinleme Güncelliği, %15 Popülerlik)
- **Frontend:** .NET 9 ASP.NET Core MVC (`SpoMusic.Web`)
  - Spotify dark-mode tasarım kimliği (`#0b0b0d`, `#1db954`, glassmorphism)
  - Responsive Razor Views (`Home`, `Auth/Login`, `Matches`, `Profile`, `Settings`)
  - HTML5 Audio Önizleme Çubuğu ve Toast Bildirimleri
- **Veritabanı:** PostgreSQL 16 (Docker)
- **Konteynerizasyon:** Docker Compose (3 servis: `spomusic-postgres`, `spomusic-api`, `spomusic-web`)
- **Testler:** xUnit Birim Testleri (`SpoMusic.Tests`) - 4/4 Geçti

---

## 🚀 Hızlı Başlangıç (Docker ile Çalıştırma)

### 1. Ön Gereksinimler
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (Yerel geliştirme için)

### 2. Konteynerleri Başlatma
```bash
docker compose up -d --build
```

Servisler ayağa kalktığında:
- **MVC Web Arayüzü:** `http://localhost:3000`
- **Web API Swagger & Health:** `http://localhost:4000/health`
- **PostgreSQL:** `localhost:5432`

---

## 🎵 Spotify Entegrasyonu

Spotify Developer Dashboard üzerinde uygulamanız için aşağıdaki Redirect URI tanımlanmalıdır:
- `http://127.0.0.1:4000/auth/spotify/callback`
- `http://localhost:4000/auth/spotify/callback`

---

## 📊 Eşleşme Algoritması Formülü

Kullanıcılar arasındaki uyum skoru aşağıdaki ağırlıklı matematiksel modelle hesaplanır:

$$\text{Toplam Skor} = 0.40 \times S_{\text{track}} + 0.25 \times S_{\text{artist}} + 0.20 \times S_{\text{recency}} + 0.15 \times S_{\text{popularity}}$$

1. **Ortak Parça Uyumu (%40):** Ortak en çok dinlenen şarkıların $O(\min(m,n))$ karmaşıklığında ağırlıklı Jaccard benzerliği.
2. **Sanatçı Benzerliği (%25):** Farklı parçalar olsa dahi aynı sanatçıları dinleme oranı.
3. **Dinleme Güncelliği (%20):** Son 30 gün içinde dinlenen güncel parçaların zaman ağırlıklı korelasyonu.
4. **Popülerlik / Zevk Nadirliği (%15):** Dinlenen parçaların popülerlik indekslerinin mutlak farkı.

---

## 📄 Lisans
MIT License
