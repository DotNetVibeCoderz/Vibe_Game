# Mario Kart 3D - Development Plan

This document contains the detailed checklist of modules to be developed for the Mario Kart 3D game. Progress is updated as features are completed.

## ✅ Completed

### 1. Project Setup
- [x] Create WPF Blazor Hybrid project (`MarioCart3D.csproj`)
- [x] Configure WPF host window (`MainWindow.xaml`, `MainWindow.xaml.cs`)
- [x] Configure Blazor WebView and dependency injection
- [x] Create `App.xaml` and `App.xaml.cs`
- [x] Create `wwwroot/index.html` with Three.js integration

### 2. Core 3D Engine (Three.js)
- [x] Initialize Three.js scene, camera, renderer
- [x] Add lighting (ambient + directional with shadows)
- [x] Create procedural kart model (body, wheels, driver, hat, spoiler, headlights)
- [x] Create procedural track system (road, borders, decorations)
- [x] Add animated environment (trees, item boxes, clouds)
- [x] Implement camera follow kart behavior
- [x] Implement game animation loop
- [x] Implement resize handling
- [x] Add keyboard controls (WASD / Arrow keys)
- [x] Create AI kart rendering and updates
- [x] Particle effects system

### 3. UI / UX
- [x] Design main menu overlay with title and mode cards
- [x] Create Settings menu with volume and graphics options
- [x] Create About menu with project information
- [x] Create Pause menu overlay
- [x] Create HUD (lap, time, position, coins)
- [x] Implement CSS styling and animations
- [x] Create Race page with item display and finish screen
- [x] Create Battle Mode page
- [x] Responsive UI for different screen sizes

### 4. Game Modes
- [x] Define game modes: Grand Prix, Time Trial, VS Race, Battle Mode
- [x] Wire menu navigation to game mode selection
- [x] AI count per mode (Time Trial = 0, others = 7)

### 5. Characters & Customization
- [x] Create Racer model and data
- [x] Create Kart, Wheel, Glider models and data
- [x] Create Character Select page
- [x] Create Kart Customization page
- [x] Create Track Select page

### 6. Tracks
- [x] Create RaceTrack model
- [x] Define multiple themed tracks (Mushroom Circuit, Bowser's Castle, Rainbow Road, Koopa Beach, Ghost Valley, DK Mountain)
- [x] Implement track switching capability in JS engine
- [x] Dynamic sky/fog colors per track theme
- [x] Add boost pads on tracks
- [x] Improved track generation with elevation and curves
- [x] Elevated shortcut bridge
- [x] Road center markings
- [x] Theme-specific decorations (rocks for castle/mountain)

### 7. Power-ups
- [x] Create PowerUp model
- [x] Define classic items (Green Shell, Red Shell, Banana, Star, Bullet Bill, Blue Shell)
- [x] Item box collision detection
- [x] Item usage via SPACE key

### 8. Race Management
- [x] Create Checkpoint model
- [x] Create RacePosition model
- [x] Implement RaceManager with lap counting
- [x] Implement position calculation
- [x] Implement finish detection

### 9. AI System
- [x] Create AiRacerService with path following
- [x] AI karts rendered in 3D scene
- [x] AI positions updated in RaceManager
- [x] AI skill levels and dynamic speed
- [x] AI elevation following

### 10. Services
- [x] Create GameDataService for static game data
- [x] Create GameStateService for session state
- [x] Create RaceManager for race logic
- [x] Create AiRacerService for AI behavior
- [x] Create ItemManager for power-ups
- [x] Create AudioService with JS interop
- [x] Create ProgressionService for profile persistence
- [x] Create WeatherService for dynamic weather
- [x] Register all services in DI container

### 11. Routing
- [x] Configure Blazor Router in `Main.razor`
- [x] Create pages: Index, CharacterSelect, KartSelect, TrackSelect, Race, BattleMode
- [x] Create `_Imports.razor`

### 12. Progression
- [x] Player profile persistence to local JSON file
- [x] Unlockable characters, karts, and tracks
- [x] StreetPass-style scoring and global rank
- [x] Record race results (wins, best time, coins)

### 13. Gameplay Mechanics
- [x] Lap counting and checkpoint system
- [x] AI opponents with path following
- [x] Item usage and collision detection
- [x] Boost pads
- [x] Drifting and mini-turbo mechanics

### 14. Audio
- [x] Background music per track theme (procedural melodies)
- [x] Engine sound effects
- [x] Item sound effects
- [x] Boost sound effects
- [x] Volume control integration

### 15. Weather Effects
- [x] Rain particle system
- [x] Snow particle system
- [x] Fog support
- [x] Random weather per race
- [x] Weather fog density adjustment

### 16. Battle Mode
- [x] Battle Mode page
- [x] Lives system
- [x] Score system
- [x] Item collection in arena

### 17. Documentation
- [x] Create `README.md` (English & Indonesia)
- [x] Maintain `PLAN.md` progress

### 18. Build & Verification
- [x] Successful compilation
- [x] Verify wwwroot files are included in output

### 19. Debugging
- [x] Enable Chrome DevTools (auto-open in debug)
- [x] F12 hotkey for DevTools

### 20. Polish
- [x] Spoiler and headlights on karts
- [x] Elevation-based tracks
- [x] Shortcut bridge
- [x] Road markings
- [x] Theme decorations
- [x] Responsive CSS

## 🚧 Optional Future Enhancements

### 21. Multiplayer
- [ ] Implement local split-screen multiplayer
- [ ] Implement LAN multiplayer connection

### 22. Assets
- [ ] Add screenshot for README
- [ ] Add real 3D model assets instead of procedural geometry
- [ ] Add texture support for tracks and karts

### 23. Advanced Gameplay
- [ ] Add collision detection between karts
- [ ] Add item projectiles with physics
- [ ] Add more complex battle mode AI
