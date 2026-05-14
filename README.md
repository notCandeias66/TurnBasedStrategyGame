# TurnBasedStrategyGame

![Unity](https://img.shields.io/badge/Unity-2022.3.22f1-black?logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS-blue)
![License](https://img.shields.io/badge/License-Academic-lightgrey)
![ISCTE](https://img.shields.io/badge/ISCTE-Programming%20%26%20Generation%20of%20Virtual%20Worlds-orange)

A 3D turn-based strategy game built in Unity for the **Programming and Generation of Virtual Worlds** course at ISCTE. Two game instances run simultaneously on separate boards inside a fully dressed 3D studio environment, replaying pre-defined match sequences loaded from XML configuration files.

---

## Table of Contents

- [About the Project](#about-the-project)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Download Game Build](#download-game-build)
- [Configuration — XML Files](#configuration--xml-files)
- [Gameplay Overview](#gameplay-overview)
- [Demos](#demos)
- [License](#license)
- [Authors](#authors)

---

## About the Project

Turn Based Strategy Game, a 3D simulation built in Unity where two separate strategy game instances play out in real time inside a virtual studio environment. The matches are not player-controlled as all moves are pre-authored in XML files, and the engine faithfully replays each turn in sequence.
 
The studio set contains two physical board tables, each running an independent game. Units, represented as geometric shapes, move across tile-based grids, engage in combat, and trigger optional close-up battle sequences when swordsmen clash.
 
This project was developed as a course assignment at ISCTE, with a focus on procedural environment generation, XML-driven game logic, and Unity scene management.

---

## Features

- **Dual simultaneous boards** — two independent game instances run in the same Unity scene inside a shared 3D studio environment
- **XML-driven replay system** — all players, board dimensions, and turn sequences are defined in external XML files; no hardcoded match data
- **Four unit types** — Swordsmen, Archers, Mages, and Catapults, each with distinct roles on the board
- **Tile-based grid boards** — board size and layout are fully configurable via XML
- **Swordsman battle prompt** — when a swordsman combat occurs on the currently viewed board, a prompt appears offering a close-up battle scene
- **3D studio environment** — richly decorated virtual studio built with free Unity Asset Store props
- **Prefab-based unit system** — all units are instantiated from Unity prefabs at runtime

---

## Architecture

<img width="1920" height="1080" alt="Architecture" src="https://github.com/user-attachments/assets/51e854a1-6651-40eb-b235-f887ed231e0a" />

---

## Tech Stack

| Component | Technology |
|---|---|
| Game Engine & Editor | Unity (C#)
| Match configuration & game data | XML |
| Environment props & assets | Unity Asset Store |

---

## Project Structure

```
TurnBasedStrategyGame/
├── CompletedTurnBasedStrategyGame/         # Main Unity project folder
│   ├── Assets/
│   │   ├── Animations/                 # Unit & combat animations
│   │   ├── Character Controller/       # Character Controllers
│   │   ├── Exterior Terrain/           # Terrain settings/props (battle scene)
│   │   ├── Images/                     # Images used in the tiles
│   │   ├── Mini Map/                   # Mini map view of the scene
│   │   ├── Prefabs/                    # Unit and environment prefabs
│   │   ├── Props/                      # All propos used in the board scene
│   │   ├── Scenes/                     # Scenes used (SampleScene = board scene; Cutscene = battle scene)
│   │   ├── Scripts/
│   │   │   ├── Cameras/                # All Camera logic
│   │   │   ├── HUD/                    # Handles HUD operations
│   │   │   ├── Functionality/          # All functionality (Board management, tile and unit clickers)
│   │   │   ├── Lights/                 # Lamp lights logic
│   │   │   ├── Loader/                 # Board loader
│   │   │   ├── MenuOptions/            # Quit game logic
│   │   │   ├── Throwables/             # Arrow and cannonball logic
│   │   │   ├── TIles/                  # Tile logic
│   │   │   ├── Units/                  # Unit behaviour & movement
│   │   │   ├── Sounds/                 # All sounds
│   │   ├── XML Files/                  # Match configuration files
│   ├── Packages/                       # Unity package manifest
│   └── ProjectSettings/                # Unity project settings
├── .gitignore
└── README.md
```

---

## Getting Started

### Prerequisites
 
- **Unity Hub** installed — [Download here](https://unity.com/download)
- **Unity 2022.3.22f1** — install via Unity Hub (use the exact version to avoid compatibility issues)
- Git (to clone the repository)

1. **Clone the repository**
   ```bash
   git clone https://github.com/notCandeias66/TurnBasedStrategyGame.git
   cd TurnBasedStrategyGame
   ```
 
2. **Open with Unity Hub**
   - Launch Unity Hub
   - Click **Open** → **Add project from disk**
   - Navigate to and select the `FinalTurnBasedStrategyGame/` folder
   - Make sure Unity **2022.3.22f1** is selected as the editor version
3. **Wait for Unity to import assets**
   - First import may take several minutes depending on your machine
4. **Open the main scene**
   - In the Unity Project window, navigate to `Assets/Scenes/`
   - Double-click the main studio scene to open it
5. **Press Play**
   - Hit the Play button in the Unity Editor to start the simulation

---

## Download Game Build

The standalone game build is available in the Releases section.

Download and extract all files, keeping them in the same folder as the `.exe`, then simply run the `.exe` to play.

---

## Configuration — XML Files

All match data is driven by XML configuration files. No code changes are needed to modify a game. Simply assign the desired XML file to either board1 or board2 and pressing Play.

### XML Structure

```xml
<?xml version="1.0"?>
<game>
	<roles>
		<role name="Player 1"/>
		<role name="Player 2"/>
	</roles>
	<board width="8" height="2">
		<sea/><sea/><sea/><sea/><sea/><sea/><sea/><sea/>
		<village/><desert/><desert/><desert/><desert/><desert/><desert/><desert/>
	</board>
	<turns>
		<turn>
			<unit id="1" role="Player 1" type="archer" action="spawn" x="1" y="1" />
			<unit id="2" role="Player 2" type="archer" action="spawn" x="2" y="1" />
			<unit id="3" role="Player 1" type="soldier" action="spawn" x="3" y="2" />
			<unit id="4" role="Player 2" type="soldier" action="spawn" x="3" y="2" />
			<unit id="5" role="Player 1" type="mage" action="spawn" x="5" y="2" />
		</turn>
		<turn>
			<unit id="1" role="Player 1" type="archer" action="attack" x="2" y="1" />
			<unit id="2" role="Player 2" type="archer" action="hold" x="2" y="1" />
			<unit id="3" role="Player 1" type="soldier" action="attack" x="3" y="2" />
			<unit id="4" role="Player 2" type="soldier" action="hold" x="3" y="2" />
			<unit id="5" role="Player 1" type="mage" action="attack" x="6" y="2" />
		</turn>
	</turns>
</game>
```

> **Note:** The XML moves are always assumed to be legal. The engine does not validate move legality at runtime. Instead, it defines what *happens* during each action (e.g. what an attack does). Each tile supports up to 4 units simultaneously.

### Configurable Parameters
 
| Parameter | Description |
|---|---|
| `Roles` | Player names |
| `board ` | Dimensions of the tile grid + what tiles in each position |
| `Turns` | Contains all turns to be replayed |
| `Turn` | Actual moves to be replayed |

---

## Gameplay Overview

### The Studio Scene
 
The game opens inside a 3D virtual studio. Two physical board tables sit in the environment, each hosting a live game instance. Both boards begin replaying their respective XML matches simultaneously when Play is pressed.

### Unit Types
 
| Unit | Notes |
|---|---|
| **Swordsman** | Triggers the optional close-up battle scene when two swordsmen clash |
| **Archer** |  |
| **Mage** |  |
| **Catapult** |  |

### Turn Replay
 
Each turn, the engine reads the next move from the XML file and animates the corresponding unit on the board. Both boards advance independently.
The game runs automatically. However, there are options to manually adavnce the game, revert to previous turns.

### Swordsman Battle

To view the swordsman battle scene, make sure **Battle Cutscenes** is set to **ON** on the left side of the screen before the battle begins. Since the battle actions are predetermined, this setting must be enabled in advance.

> **Note:** The camera controls may feel a bit rough or unresponsive at times. This is a known issue and can make the battle scene harder to navigate.

---

## Demos

### Gameplay Simulation

https://www.youtube.com/watch?v=UtJyib_S1hY&t=1s

### Environment Creation

https://www.youtube.com/watch?v=-kQzRxfOlI8&t=3s

### Project Explanation (Audio: Portuguese)

https://www.youtube.com/watch?v=YEi4b1RiX-I&t=2s

> **Note:** Two additional group members appear in this video. They were part of the project group but did not contribute to the codebase or documented work.

---

## License

This project was developed as a university course assignment at **ISCTE — Instituto Universitário de Lisboa** for the *Programming and Generation of Virtual Worlds* course.

If you wish to use or adapt this work, please contact the author directly.

---

## Authors

| Name | GitHub | Contribution |
|---|---|---|
| Afonso Candeias | [@notCandeias66](https://github.com/notCandeias66) | Board generation, XML parser, turn execution & unit movement |
| Rui Jesus | [@rui-iscte](https://github.com/rui-iscte) | Scene switching, combat animations, environment creation |
