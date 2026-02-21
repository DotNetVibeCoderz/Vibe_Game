# DoomNet - FPS Game Clone

## English

**DoomNet** is a First-Person Shooter (FPS) game clone inspired by the classic Doom. It is built using a hybrid architecture combining **WPF (Windows Presentation Foundation)** for the desktop window container, **Blazor Hybrid** for bridging .NET and Web technologies, and **Three.js** for high-performance 3D rendering in the browser environment.

### Features

- **First-Person Shooter Controls**: WASD movement, Mouse look, Jump, Sprint, and Crouch.
- **3D Environment**: Procedurally generated maze-like rooms with textures, dynamic lighting, and fog.
- **Combat System**: Multiple weapons (Pistol, Shotgun, Rifle), shooting mechanics with raycasting, and ammo management.
- **Enemies & AI**: Simple enemy AI that chases the player and deals damage on contact.
- **HUD**: Health bar, Ammo counter, and Weapon selector.
- **Menu System**: New Game, Options (Sensitivity/Volume), About, and Exit.
- **Game Loop**: Win/Loss conditions, Game Over screen, and Restart functionality.

### Technology Stack

- **Host**: WPF (.NET 8)
- **WebView**: Microsoft.AspNetCore.Components.WebView.Wpf
- **Rendering**: Three.js (JavaScript)
- **Language**: C# and JavaScript

### How to Run

1. Ensure **.NET 8 SDK** is installed.
2. Open the project folder in Visual Studio or VS Code.
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```
5. Run the game:
   ```bash
   dotnet run
   ```

### Controls

- **W, A, S, D**: Move
- **Mouse**: Look Around
- **Space**: Jump
- **Left Shift**: Sprint
- **Left Ctrl**: Crouch
- **1, 2, 3**: Switch Weapons
- **Left Mouse Button**: Shoot
- **Esc**: Pause / Release Cursor

---

## Bahasa Indonesia

**DoomNet** adalah kloning game First-Person Shooter (FPS) yang terinspirasi oleh game klasik Doom. Aplikasi ini dibangun menggunakan arsitektur hibrida yang menggabungkan **WPF (Windows Presentation Foundation)** sebagai wadah jendela desktop, **Blazor Hybrid** untuk menjembatani teknologi .NET dan Web, serta **Three.js** untuk rendering 3D berkinerja tinggi di lingkungan browser.

### Fitur Utama

- **Kontrol FPS**: Gerakan WASD, pandangan mouse, lompat, sprint, dan merunduk.
- **Lingkungan 3D**: Ruangan bergaya labirin yang dibuat secara prosedural dengan tekstur, pencahayaan dinamis, dan efek kabut.
- **Sistem Tempur**: Berbagai senjata (Pistol, Shotgun, Rifle), mekanisme menembak menggunakan raycasting, dan manajemen amunisi.
- **Musuh & AI**: AI musuh sederhana yang akan mengejar pemain dan memberikan damage jika bersentuhan.
- **HUD Interface**: Bar kesehatan, penghitung peluru, dan pemilih senjata.
- **Sistem Menu**: New Game, Options (Sensitivitas/Volume), About, dan Exit.
- **Mekanisme Game**: Kondisi kalah/menang, layar Game Over, dan fungsi restart.

### Teknologi

- **Host**: WPF (.NET 8)
- **WebView**: Microsoft.AspNetCore.Components.WebView.Wpf
- **Rendering**: Three.js (JavaScript)
- **Bahasa**: C# dan JavaScript

### Cara Menjalankan

1. Pastikan **.NET 8 SDK** sudah terinstal di komputer Anda.
2. Buka folder proyek di Visual Studio atau VS Code.
3. Pulihkan dependensi (restore):
   ```bash
   dotnet restore
   ```
4. Bangun (build) proyek:
   ```bash
   dotnet build
   ```
5. Jalankan game:
   ```bash
   dotnet run
   ```

### Kontrol Permainan

- **W, A, S, D**: Bergerak
- **Mouse**: Melihat sekeliling
- **Spasi**: Lompat
- **Shift Kiri**: Lari Cepat (Sprint)
- **Ctrl Kiri**: Merunduk (Crouch)
- **1, 2, 3**: Ganti Senjata
- **Klik Kiri Mouse**: Menembak
- **Esc**: Jeda / Melepaskan Kursor

---

### Credits

Created by **Jacky the Code Bender**  
**Gravicode Studios** Team Lead: Kang Fadhil

*If you enjoy this project, consider buying me a coffee (or pulsa)!*  
[Support Here](https://studios.gravicode.com/products/budax)

---

*Note: This is a prototype/demo. Assets are procedural/generated code. No external 3D models are included to keep the project lightweight.*