# 🏎️ Mario Kart 3D

A fan-made desktop kart racing game built with **WPF Blazor Hybrid** and **Three.js**. Race through iconic tracks, customize your kart, battle AI opponents, unlock new content, and experience dynamic weather and audio!

## 🎮 Features

### Gameplay & Mechanics
- **Grand Prix** - Race through cups to become champion
- **Time Trial** - Beat the clock on your favorite track
- **VS Race** - Custom race against AI opponents
- **Battle Mode** - Item-based arena battles with lives and score
- **Vehicle Customization** - Choose kart body, wheels, and glider
- **Classic Power-ups** - Green/Red/Blue Shells, Banana, Star, Bullet Bill
- **Drifting & Mini-Turbo** - Hold Shift/Space while turning
- **Boost Pads** - Drive over glowing pads for speed boost
- **6 3D Tracks** - Themed circuits with dynamic visuals

### Characters
Play as Mario, Luigi, Peach, Bowser, Yoshi, and Toad. Each has unique stats for speed, acceleration, handling, and weight.

### Tracks
- 🌿 Mushroom Circuit
- 🏰 Bowser's Castle
- 🌈 Rainbow Road
- 🏖️ Koopa Beach
- 👻 Ghost Valley
- ⛰️ DK Mountain

### Audio
- Procedural background music per track theme
- Dynamic engine sound pitch based on speed
- Item and boost sound effects
- Master/Music/SFX volume controls

### Weather
- Dynamic rain, snow, and fog effects
- Random weather applied per race

### Progression
- Unlockable characters, karts, and tracks
- Local profile saved to `%LocalAppData%\MarioCart3D\profile.json`
- StreetPass-style scoring and global ranking
- Race statistics (wins, best time, coins)

### Additional Features
- AI opponents with path following
- Lap counting and checkpoint system
- Live position calculation
- Particle effects for boosts and drifts
- Chrome DevTools enabled in Debug mode (F12)

## 🛠️ Technologies

- **.NET 8**
- **WPF (Windows Presentation Foundation)**
- **Blazor Hybrid**
- **Three.js** for 3D rendering
- **C#**

## 🚀 Getting Started

### Prerequisites
- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or later (recommended)

### Run the Application

```bash
cd MarioCart3D
dotnet run
```

### Using Visual Studio
1. Open `MarioCart3D.csproj` or the folder in Visual Studio.
2. Press `F5` to run.

## 🎮 Controls

| Action | Key |
|--------|-----|
| Accelerate | Up Arrow / W |
| Brake/Reverse | Down Arrow / S |
| Steer Left | Left Arrow / A |
| Steer Right | Right Arrow / D |
| Drift | Shift / Space (while turning) |
| Use Item | Space |
| Pause | Escape |
| DevTools | F12 (Debug mode) |

## 📁 Project Structure

```
MarioCart3D/
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Main.razor
├── _Imports.razor
├── Pages/
│   ├── Index.razor
│   ├── CharacterSelect.razor
│   ├── KartSelect.razor
│   ├── TrackSelect.razor
│   ├── Race.razor
│   └── BattleMode.razor
├── Models/
│   ├── Racer.cs
│   ├── Kart.cs
│   ├── Wheel.cs
│   ├── Glider.cs
│   ├── RaceTrack.cs
│   ├── PowerUp.cs
│   ├── GameMode.cs
│   ├── PlayerProfile.cs
│   ├── Checkpoint.cs
│   └── RacePosition.cs
├── Services/
│   ├── GameDataService.cs
│   ├── GameStateService.cs
│   ├── RaceManager.cs
│   ├── AiRacerService.cs
│   ├── ItemManager.cs
│   ├── AudioService.cs
│   ├── ProgressionService.cs
│   └── WeatherService.cs
├── wwwroot/
│   ├── index.html
│   ├── css/
│   │   └── app.css
│   └── js/
│       ├── audio.js
│       ├── weather.js
│       ├── game-part1.js
│       ├── game-part2.js
│       ├── game-part3.js
│       └── dotnet-bridge.js
├── PLAN.md
└── README.md
```

## 🤝 Contributing

This is a fan-made educational project. Contributions and suggestions are welcome!

## 📄 License

This project is for educational purposes only. Mario Kart and all related characters are trademarks of Nintendo. This project is not affiliated with or endorsed by Nintendo.

---

Created with ❤️ by the team at **Gravicode Studios**.

---

# 🏎️ Mario Kart 3D (Bahasa Indonesia)

Game balap kart desktop buatan penggemar yang dibangun dengan **WPF Blazor Hybrid** dan **Three.js**. Balap di lintasan ikonik, kustomisasi kart, lawan AI, buka konten baru, dan rasakan efek cuaca serta audio dinamis!

## 🎮 Fitur

### Gameplay & Mekanik
- **Grand Prix** - Balap untuk menjadi juara
- **Time Trial** - Kalahkan waktu di lintasan favorit
- **VS Race** - Balap kustom melawan AI
- **Battle Mode** - Pertarungan berbasis item dengan nyawa dan skor
- **Kustomisasi Kendaraan** - Pilih bodi kart, ban, dan glider
- **Power-up Klasik** - Green/Red/Blue Shell, Banana, Star, Bullet Bill
- **Drifting & Mini-Turbo** - Tahan Shift/Space saat belok
- **Boost Pads** - Lewati pad menyala untuk tambah kecepatan
- **6 Lintasan 3D** - Sirkuit bertema dengan visual dinamis

### Karakter
Main sebagai Mario, Luigi, Peach, Bowser, Yoshi, dan Toad. Masing-masing punya stat kecepatan, akselerasi, handling, dan berat yang unik.

### Lintasan
- 🌿 Mushroom Circuit
- 🏰 Bowser's Castle
- 🌈 Rainbow Road
- 🏖️ Koopa Beach
- 👻 Ghost Valley
- ⛰️ DK Mountain

### Audio
- Musik latar prosedural per tema lintasan
- Suara mesin dengan pitch berdasarkan kecepatan
- Efek suara item dan boost
- Kontrol volume Master/Music/SFX

### Cuaca
- Efek hujan, salju, dan kabut dinamis
- Cuaca acak diterapkan per balapan

### Progresi
- Karakter, kart, dan lintasan yang dapat dibuka
- Profil lokal tersimpan di `%LocalAppData%\MarioCart3D\profile.json`
- Skoring bergaya StreetPass dan peringkat global
- Statistik balapan (menang, waktu terbaik, koin)

## 🛠️ Teknologi

- **.NET 8**
- **WPF**
- **Blazor Hybrid**
- **Three.js**
- **C#**

## 🚀 Cara Menjalankan

```bash
cd MarioCart3D
dotnet run
```

## 🎮 Kontrol

| Aksi | Tombol |
|------|--------|
| Gas | Panah Atas / W |
| Rem/Mundur | Panah Bawah / S |
| Belok Kiri | Panah Kiri / A |
| Belok Kanan | Panah Kanan / D |
| Drift | Shift / Space (saat belok) |
| Gunakan Item | Spasi |
| Jeda | Escape |
| DevTools | F12 (mode Debug) |

## 📄 Lisensi

Proyek ini hanya untuk tujuan edukasi. Mario Kart dan karakter terkait adalah merek dagang Nintendo. Proyek ini tidak berafiliasi dengan atau didukung oleh Nintendo.

---

Dibuat dengan ❤️ oleh tim **Gravicode Studios**.
