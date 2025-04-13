# ECS7016P Coursework: Procedural Level Generator

**Student:** Kehao Liu  
**Project Title:** Zombie Apocalypse - Procedural Island Escape  
**Unity Version:** 2023.1.0f1  
**StudentID:** 231249184

---

## Design Brief

This project fulfills the brief for:

> **Game Concept 2: Zombie Apocalypse**  
> *A level made up of multiple islands in the ocean. The player is a survivor trying to locate a hidden laboratory to escape. The level generator must reflect this gameplay vision.*

---

## Generator Structure (Three Stages)

The generator is implemented in `MapGenerator.cs` and follows a 3-phase structure:

### Stage 1: Initial Map Generation
- Supports **two generation modes**: `RandomFill` and `IslandArchipelago`
- Uses Perlin Noise and distance constraints to generate natural-looking, scattered islands
- Configurable island count, radius, spacing, and seed

### Stage 2: Smoothing (CA)
- Applies **cellular automata rules** to smooth terrain
- Executed for 5 iterations
- Optional: can be skipped or customized

### Stage 3: Final Map Processing
- Detects islands, removes edge/small islands
- Places **player spawn point** and **lab** on distant islands
- Connects them with a **main bridge (A*)**
- Generates **fake bridges** to other islands for misdirection

---

## Core Features

###  Island Connectivity
- Main island pair chosen by difficulty (`hardMode`)
- Main path calculated using A* algorithm
- Fake bridges misleading player path (settable count)

### Code Structure
- All core logic in `MapGenerator.cs`
- Additional scripts: `PlayerAgent.cs`, `ZombieAgentPathing.cs`, `GameManager.cs`, `MeshGenerator.cs`

### Visual Placement
- `PlayerPrefab`, `LabPrefab`, `BridgePrefab` placed at runtime
- All prefabs instantiated at grid-aligned positions
- Automatically clears previous instances

---

## Autonomous Agent Feature (20% Bonus)

### PlayerAgent
- Automatically walks from spawn to lab
- Uses the same A* path as the main bridge
- Triggers win condition upon reaching lab

### ZombieAgent
- Randomly spawned on connected islands
- Uses **dynamic A\*** to chase player
- Path recalculated every 1.5s
- Cannot cross ocean (only walks on islands & bridges)
- Triggers loss condition if zombie catches player

### AI Behaviors
- Fully autonomous
- Agent prefab + path recalculation in `ZombieAgentPathing.cs`
- Victory/defeat handled via `GameManager.cs`

---

## Implementation Notes

- Generator is implemented inside **`MapGenerator.cs` only**, as required
- All stages are cleanly modularized and well-commented
- `MeshGenerator.cs` is based on Sebastian Lague’s cave generation:
  [Sebastian Lague - Procedural Cave Generation](https://www.youtube.com/watch?v=v7yyZZjF1z4)

---

## Submission Info

- Project cleaned of `Library`, `Temp`, and other large folders
- Final compressed size < 50MB
- Built for Unity 2023.1
- Scripts referenced:
  - A* Pathfinding: self-implemented in `MapGenerator.cs`
  - AI behaviors: original implementation
  - Visual debugging via `Gizmos`

---

## Acknowledgements

- Pathfinding and CA concepts inspired by Sebastian Lague
- Some scripting logic supported by [ChatGPT] for modularity & structure