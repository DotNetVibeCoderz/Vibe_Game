# 🎮 Breakout - Neon Edition

> *A classic Breakout game reimagined with modern neon visuals, particle effects, and multi-level progression.*

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![SkiaSharp](https://img.shields.io/badge/SkiaSharp-2.88-green)
![Windows Forms](https://img.shields.io/badge/Platform-Windows-0078D6)
![Status](https://img.shields.io/badge/Status-Completed-brightgreen)

---

## 📸 Screenshots

```
╔═══════════════════════════════════════════════════════╗
║              ██████  BREAKOUT  ██████                ║
║              ██  Neon Edition  ██                    ║
║                                                      ║
║               ▶ New Game                             ║
║                 Settings                             ║
║                 About                                ║
║                 Exit                                 ║
║                                                      ║
║        ↑↓ Navigate • Enter/Space to select           ║
╚═══════════════════════════════════════════════════════╝
```

---

## ✨ Features

### 🎯 Core Gameplay
- **30+ Procedural Levels** — Each level gets progressively harder with faster ball speed, more rows, and complex brick layouts
- **4 Brick Types:**
  - 🟥 **Standard** — Destroy in one hit
  - 🟦 **Multi-Hit** — Requires 3 hits to break (shows remaining hits)
  - 🟧 **Explosive** — Destroys nearby bricks on impact
  - 🟩 **Power-Up** — Releases special items when broken
- **5 Power-Ups:**
  - ⬌ **Paddle Increase** — Widen your paddle for easier catching
  - ⬍ **Paddle Decrease** — Narrow paddle for extra challenge
  - ⚬ **Multi-Ball** — Multiple balls in play
  - ⚡ **Laser** — Shoot projectiles to destroy bricks
  - ◷ **Slow-Motion** — Time slows down for precise control
- **Combo Multiplier** — Chain hits together for bonus points (up to 3x!)
- **3 Difficulty Levels** — Easy 😊, Medium 😐, Hard 😈

### ✨ Visual Effects
- **Neon Glow** — All objects have beautiful neon glow effects
- **Ball Trail** — Smooth fading trail behind the ball
- **Particle System** — 6 different particle effects:
  - Brick shatter
  - Explosions
  - Paddle hits
  - Power-up collection
  - Life lost
  - Level clear fireworks
- **Dynamic Backgrounds** — 3 themes that rotate per level:
  - 🌌 **Space** — Stars, nebulae, deep space gradient
  - 💠 **Neon** — Cyberpunk grid with glowing intersections
  - 👾 **Retro Arcade** — Scanlines, grid dots, rainbow bars
- **Parallax Scrolling** — Backgrounds animate smoothly

### 🖥️ User Interface
- **Animated Main Menu** — Pulsing neon title with particle background
- **Settings Menu** — Toggle difficulty, sound, music, FPS, trail, particles
- **About Screen** — Complete feature list
- **In-Game HUD** — Score, lives (♥), level, combo indicator
- **Pause Overlay** — Smooth pause screen
- **Game Over Screen** — Final score and high score display
- **Level Clear Screen** — Celebratory message between levels

### 🔊 Audio
- Retro-inspired beep sound effects
- Level clear victory melody
- Sound and music toggle in settings

### 🛠️ Level Editor
- Built-in visual level editor
- Place/remove bricks with mouse clicks
- Switch between brick types (1-4 keys)
- Export your custom levels

---

## 🎮 How to Play

### Controls

| Key | Action |
|-----|--------|
| `←` / `A` | Move paddle left |
| `→` / `D` | Move paddle right |
| `Space` | Launch ball / Confirm |
| `ESC` | Pause / Back to menu |
| `P` | Toggle pause |
| `↑` / `W` | Navigate menu up |
| `↓` / `S` | Navigate menu down |
| `Enter` | Select menu item |

### Goal
Break all the bricks in each level without letting the ball fall! Collect power-ups to gain advantages. Progress through 30+ increasingly challenging levels!

### Scoring
- **Standard Brick:** 10 pts
- **Multi-Hit Brick:** 25 pts
- **Explosive Brick:** 50 pts
- **Power-Up Brick:** 75 pts
- **Combo Bonus:** +10% per consecutive hit (up to 3x multiplier)

---

## 🏗️ Architecture

```
Breakout/
│
├── 📁 Core/                       # Game Engine Core
│   ├── GameEngine.cs              # Main game loop & state management
│   ├── GameState.cs               # Game state enum
│   ├── InputManager.cs            # Keyboard & mouse input
│   ├── CollisionDetector.cs       # Physics & collision logic
│   ├── ScoreManager.cs            # Score, lives, combo, high score
│   └── Settings.cs                # App settings (JSON persistence)
│
├── 📁 GameObjects/                # In-Game Objects
│   ├── Ball.cs                    # Ball with glow & trail
│   ├── Paddle.cs                  # Player paddle
│   ├── Brick.cs                   # 4 brick types
│   ├── PowerUp.cs                 # 5 power-up items
│   └── Laser.cs                   # Laser projectile
│
├── 📁 Levels/                     # Level System
│   ├── LevelData.cs               # Level data models
│   ├── LevelManager.cs            # Procedural generation (30 levels)
│   └── LevelEditor.cs             # Visual level editor
│
├── 📁 Effects/                    # Visual Effects
│   ├── Particle.cs                # Single particle
│   ├── ParticleSystem.cs          # Particle engine (6 effects)
│   ├── BallTrail.cs               # Ball trail renderer
│   └── Background.cs              # 3 dynamic themes
│
├── 📁 Audio/                      # Sound System
│   └── AudioManager.cs            # Beep-based sound effects
│
├── 📁 UI/                         # User Interface
│   ├── MenuManager.cs             # All menu screens
│   └── Hud.cs                     # In-game HUD
│
├── Form1.cs                       # Main WinForms form
├── Form1.Designer.cs              # Form designer
├── Program.cs                     # Entry point
├── Breakout.csproj                # Project file
├── README.md                      # This file
└── PLAN.md                        # Development plan
```

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 8.0 | Framework |
| **C#** | 12 | Language |
| **Windows Forms** | — | Desktop UI |
| **SkiaSharp** | 2.88.7 | 2D Graphics & Rendering |
| **SkiaSharp.Views.WindowsForms** | 2.88.7 | SkiaSharp WinForms integration |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows OS (7 or later)

### Run the Game

```bash
# Clone or navigate to the project directory
cd Breakout

# Build and run
dotnet run
```

### Build Release

```bash
dotnet build -c Release
```

The compiled executable will be at `bin\Release\net8.0-windows\Breakout.exe`.

---

## 🎨 Customization

### Adding New Levels
Levels are generated procedurally, but you can modify the generation parameters in `Levels/LevelManager.cs`:
- Adjust `BRICK_WIDTH`, `BRICK_HEIGHT`, `BRICK_MARGIN` for brick sizing
- Modify `GetBrickType()` for different brick distribution
- Change `rows` calculation for different difficulty curves

### Using the Level Editor
1. Select **Level Editor** from the main menu (if enabled)
2. Click on grid cells to place bricks
3. Press keys `1`-`4` to switch brick types
4. Press `S` to save your level design
5. Press `ESC` to exit

---

## 🤝 Contributing

This project was created by **Jacky the Code Bender** from **Gravicode Studios**. Feel free to fork, modify, and enhance!

### Ideas for Enhancement
- ✅ Add more power-ups (shield, fireball, magnet paddle)
- ✅ Implement high score leaderboard
- ✅ Add more background themes
- ✅ Multiplayer mode
- ✅ Save/Load custom levels
- ✅ Add achievements system

---

## 📄 License

This project is provided for educational and entertainment purposes.

---

## 🙏 Credits

- **Developer:** Jacky the Code Bender
- **Studio:** [Gravicode Studios](https://studios.gravicode.com)
- **Graphics Engine:** [SkiaSharp](https://github.com/mono/SkiaSharp)
- **Inspiration:** Classic Breakout by Atari (1976)

---

> *"If you can break bricks, you can break barriers."* 💪

---

⭐ **Like this project?** Consider supporting at https://studios.gravicode.com/products/budax
