# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the project
dotnet build Erg/Erg.csproj

# Run the game
dotnet run --project Erg/Erg.csproj

# Build in release mode
dotnet build Erg/Erg.csproj -c Release
```

## Project Overview

Erg is a roguelike game written in C# targeting .NET 9.0, using Raylib-cs for graphics rendering. It follows a terminal-style character grid display (80x25) typical of roguelikes.

## Architecture

### Platform Abstraction Layer
The game uses abstraction interfaces (`IInput`, `IOutput`) to decouple core game logic from the rendering platform:
- `Core/Abstractions/` - Platform-independent interfaces
- `Platforms/Raylib/` - Raylib-cs implementation (`RaylibInput`, `RaylibOutput`)

### Game Flow
- `Game` class manages the main loop and view transitions
- `IGameView` interface defines views (IntroView, PlayView, InventoryView)
- Views handle their own input processing and rendering
- `Session` holds the current game state (Area, Player, Fov, Random)
- `SessionConfig` configures session parameters including seed for deterministic generation

### World Model
- `Area` - 2D grid of tiles (80x20) containing entities; has `Depth` property for dungeon level
- `Entity` - Base class for all game objects with position, glyph, and blocking properties
- `Critter` - Mobile entities with speed/energy system for turn scheduling; has Inventory, `CanOpenDoor` (virtual, default false)
- `Player` - The player character (extends Critter); `CanOpenDoor = true`
- `Item` - Collectible objects with stacking support (`CanStackWith`, `StackWith`)
- `Inventory` - Collection of items with automatic stacking on add
- `Tile` - Static map elements with walkability and transparency; can contain one Critter and multiple Items
  - Display priority: Critter > Multiple items (%) > Single item > Tile glyph
  - Types: `Floor`, `Wall`, `DungeonWall`, `Rock`, `OpenDoor`, `ClosedDoor`, `SecretDoor`, `StairsUp`, `StairsDown`, `ShallowWater`, `DeepWater`
  - Structures: `Room`, `Corridor`, `Entrance`, `Cave`, `None`

### Turn System
Critters use a speed/energy system for fair turn scheduling:
- Each critter has `Speed` (100 = normal) and accumulates `Energy += Speed` each segment
- Actions require `Energy >= 0` to execute, then deduct `EnergyCost` (standard: 1000)
- Faster critters (Speed > 100) get multiple actions per round
- `Session.ProcessCritterTurns()` handles NPC turn scheduling

### Behavior/AI System
- `IBehavior` interface: `DecideAction(Critter, Session)` returns a `CritterAction`
- `CritterAction` abstract class: defines `EnergyCost` and `Execute()` method
- Implementations:
  - `PassiveBehavior` - do nothing (Dummy)
  - `SpinAttackBehavior` - special spinning attack (SpinningDummy)
  - `AmoebaBehavior` - greedy movement toward enemies, prefers water
  - `ZombieBehavior` - scans area in expanding squares (1-6), uses LOS and A* pathfinding, attacks non-zombies
- Located in `Core/World/Behaviors/`

### Combat System
- `Combat.MeleeAttack(attacker, defender, session)` handles damage calculation
- Damage uses `Dice` class (e.g., "1d6+2") from `Core/Types/Dice.cs`
- Death triggers `Critter.OnDeath()`, NPCs drop inventory items
- Killing grants XP to attacker equal to defender's `Value`
- Combat messages vary based on player visibility of combatants

### Experience System
- `ExperienceLevel` - starts at 1, increases on level up
- `ExperiencePoints` - current XP, accumulates from kills
- `ExperienceToNextLevel` - calculated via `ExperienceConfig.CalculateXPForLevel(level)` using formula `BaseXP * level^Exponent`
- `ExperienceMultiplier` - modifies XP gain (100 = x1.0)
- `BaseValue` / `Value` - critter worth; `Value = BaseValue * ExperienceLevel`
- `GainExperience(baseAmount, session?)` - adds XP with auto level-up; shows messages if session provided
- `OnLevelUp(newLevel)` - grants +4 MaxHitPoints and HitPoints
- Config in `Core/Types/ExperienceConfig.cs` (BaseXP=100, Exponent=2.0)

### Message System
- `MessageBuffer` - Handles game messages with word-wrapping and pagination
- Messages cleared on player movement; --More-- prompt for long messages

### Dungeon Generation
`DungeonGenerator3` creates procedural dungeons through a multi-phase algorithm:
1. **Fill** - Initialize map with rock
2. **Room Generation** - Place rooms (4-9 wide, 4-7 tall) with margins until 100 consecutive failures
3. **Corridor Connection** - Connect rooms with corridors (3-15 tiles, optional 90° turns)
4. **Remove Disconnected** - Fill orphaned rooms with rock
5. **Cave Generation** - If 3+ rooms were removed, generate caves in unused space
6. **Room Specialization** - Add features to 50% of rooms (columns, rounded corners, water, crosses)
7. **Dead Ends** - Create 20 dead-end corridors
8. **Door Processing** - Randomize door types (30% open, 66% closed, 4% secret between rooms)
9. **Wall Processing** - Convert rocks adjacent to floors to DungeonWall
10. **Impenetrable Rock** - Seal map edges
11. **Stairs Placement** - StairsUp at player start, StairsDown at farthest room
12. **Item Placement** - Scatter 2-10 gold coins in rooms
13. **Critter Placement** - Spawn enemies in rooms

Debug support: `GenerateStepByStep()` yields generation steps; `DebugGenerationView` visualizes progress (D from intro)

### Field of View (FOV)
`FieldOfView` implements recursive shadowcasting algorithm:
- `Visibility` enum: `Unknown` (never seen), `Known` (seen before), `Seen` (currently visible)
- Computed from player position with configurable radius (default 10)
- Respects tile transparency (walls and closed doors block light)
- Rendering: Unknown=black, Known=dimmed (1/3 brightness), Seen=normal

### Pathfinding
Located in `Core/World/Pathfinding/`:
- `Pathfinder.FindPath(area, critter, goalX, goalY)` - A* algorithm for finding paths
  - Respects `Critter.CanEnterTile()` based on Locomotion type
  - Uses `GetMovementCostMultiplier()` for terrain costs
  - Returns list of (x, y) steps or empty if no path
  - `maxNodes` parameter prevents infinite search (default 200)
- `LineOfSight.CanSee(area, x1, y1, x2, y2)` - Bresenham line algorithm
  - Checks both directions to handle tile-based asymmetry
  - Returns true if either direction has clear line of sight

### Rendering System
- `Glyph` - Character + foreground/background colors (RGBA as uint, format: 0xRRGGBBAA)
- `Writer` - Helper for text output with cursor positioning and color management
- Entities only rendered when in player's FOV (Seen tiles)
- Screen layout (80x25): Messages (rows 0-1), Area (rows 2-21), StatusLine (rows 22-24)
- `IOverlayRenderer` interface for sub-pixel rendering (health bars)
  - `QueueHealthBar(col, row, healthPercent)` - queues health bar for critter
  - Health bars: red background, green foreground proportional to HP%
  - Cleared automatically each frame in `Render()`

### Critters
Located in `Core/World/Critters/`:
- `Dummy` - stationary target, 'd', PassiveBehavior
- `SpinningDummy` - attacks adjacent, 'd' red, SpinAttackBehavior
- `Amoeba` - semiaquatic, 'j' teal, AmoebaBehavior, speed 90
- `Zombie` - slow undead, 'z' brown, ZombieBehavior, speed 80, 25 HP, 1d6 damage

Spawn ratios in dungeon generator:
- `SpecialEffect.UndeadAura` tiles (cemeteries): 100% Zombie
- Water tiles: 100% Amoeba
- Normal tiles: 40% Dummy, 40% SpinningDummy, 20% Zombie

### Input System
- `KeyPulse` - Single press detection with key repeat support
- `KeyHeld` - Continuous hold detection
- Movement: arrow keys and numpad (including diagonals via Kp1/3/7/9)
- Actions: I=inventory, O=open doors, C=close doors, G=pick up items, Space=start game/continue messages
- Debug: F5=toggle cheat mode (reveals all tiles)
