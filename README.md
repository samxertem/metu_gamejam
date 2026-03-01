# Prison Escape

![Unity](https://img.shields.io/badge/Unity-2022.3-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-10.0-239120?logo=csharp)
![Game Jam](https://img.shields.io/badge/METU-Game%20Jam-red)

A first-person prison escape puzzle game built with Unity for the **METU Game Jam**. Wake up in a dark prison cell, find keys, solve puzzles, and navigate through sewers to freedom.

> **Birinci şahis hapishane kacis/bulmaca oyunu.** Karanlik bir hucrede uyanin, anahtarlari bulun, kablo bulmacalarini cozun ve kanalizasyonlar araciligiyla ozgurlugunuze kavusun.

---

## Screenshots

<p align="center">
  <img src="Assets/Screenshots/screenshot-20260228-095854.png" width="45%" alt="Prison Cell">
  &nbsp;&nbsp;
  <img src="Assets/Screenshots/screenshot-20260228-160526.png" width="45%" alt="Security Room">
</p>

---

## Gameplay

You start locked inside a prison cell. Explore your surroundings, search cabinets for hidden items, collect keys to unlock doors, and hack security panels by solving a wire-matching minigame. Navigate dark corridors with your flashlight and escape through the sewer tunnels.

**Key Features:**
- First-person exploration in a dark, atmospheric environment
- Interactive cabinets, lockers, and multiple door types (prison bars, security doors, manholes, sewer gates)
- Wire hacking minigame — match 4 colored wires within 30 seconds with only 3 wrong attempts allowed
- Flashlight system for navigating pitch-black areas
- Tunnel teleportation with cinematic fade transitions
- Flickering lamp effects for immersive atmosphere

---

## Controls

| Key | Action |
|-----|--------|
| `WASD` | Move |
| `Mouse` | Look around |
| `E` | Interact |
| `F` | Toggle flashlight |
| `Space` | Jump |
| `Shift` | Sprint |
| `Ctrl` | Crouch |
| `Tab` | Show/hide controls |
| `Esc` | Close minigame |

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/          # Movement, camera, interaction, flashlight
│   ├── Manager/         # GameManager, ScreenFader
│   ├── Interaction/     # Doors, cabinets, collectables (IInteractable)
│   ├── Minigame/        # Wire hacking puzzle system
│   ├── Environment/     # Tunnel teleporter, lamp flicker effects
│   └── UI/              # Controls HUD overlay
├── Scenes/              # Game scenes (04_Hapishane_Guncel)
├── Audio/               # Footsteps, doors, ambient SFX
├── Materials/           # Shaders and materials
├── Models/              # 3D mesh assets
├── Screenshots/         # In-game captures
└── UI/                  # UI prefabs and elements
```

**27 C# scripts** organized across 6 modules with a clean `IInteractable` interface pattern for all interactive objects.

---

## Getting Started

### Prerequisites
- [Unity Hub](https://unity.com/download)
- **Unity 2022.3.62f3** (LTS)

### Setup

```bash
git clone https://github.com/samxertem/metu_gamejam.git
```

1. Open **Unity Hub** and click **Open > Add project from disk**
2. Select the cloned `metu_gamejam` folder
3. Open the scene `Assets/Scenes/04_Hapishane_Guncel.unity`
4. Press **Play**

---

## Technical Details

| | |
|---|---|
| **Engine** | Unity 2022.3.62f3 (LTS) |
| **Language** | C# |
| **Render Pipeline** | Built-in |
| **Key Packages** | TextMeshPro, Timeline, Visual Scripting |

### Architecture Highlights
- **Singleton pattern** for GameManager and ScreenFader (DontDestroyOnLoad)
- **IInteractable interface** — all interactive objects implement `Interact()` and `GetInteractText()`
- **Event-driven** minigame system with OnWin/OnLose callbacks
- **Raycast + OverlapSphere** hybrid interaction detection with line-of-sight checks
- **GC-free** timer string caching in minigame UI

---

## Team — Cheedem

| Name |
|---|
| **Sam Ertem** |
| **Yener Er** |
| **Emre Gundogdu** |
| **Deniz Cem Cangoz** |

---

## License

This project was created for the METU Game Jam. All rights reserved by the authors.

Third-party assets (3D models, textures) are subject to their respective licenses.
