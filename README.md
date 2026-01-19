# Erg

*A roguelike where things want to eat you.*

```
  ###·####·###
  #··········#
  #··z··@····#
  ············
  #····z·····#
  ###·########
```

## What is this?

Erg is a traditional roguelike in the spirit of the classics. Procedurally generated dungeons, permadeath (eventually), turn-based tactical combat, and everything rendered in glorious Unicode.

You descend into the depths. Zombies shamble through crypts. The drowned rise from flooded chambers. Something watches from the shadows...

## Features

**Dungeon Generation**
- Multi-level procedural dungeons with rooms, corridors, dead ends, and natural caves
- Special rooms: flooded chambers, crypts with graves, columned halls
- Secret doors hiding treasures

**Creatures**

*The Undead* - Territorial creatures that guard their domains:
- Zombies shambling through crypts and corridors
- Drowned lurking in flooded chambers - swift swimmers with powerful strokes
- Hanged haunting the depths - rare, resilient, and relentless

*Other Creatures*
- Amoebas dwelling in underground pools

**Combat & Mechanics**
- Turn-based with speed/energy system (faster creatures act more often)
- Field of view with proper shadowcasting
- Terrain matters: water slows land creatures, some can swim, some can't
- Health bars so you know who's winning

**The Little Things**
- Examine mode to inspect your surroundings
- Message log that doesn't overwhelm you
- Items you can pick up (gold and corpses, for now)

## Getting Started

You'll need .NET 9.0.

```bash
# Clone and run
git clone <repo-url>
cd Erg
dotnet run --project Erg/Erg.csproj
```

Or build first if you prefer:
```bash
dotnet build Erg/Erg.csproj
dotnet run --project Erg/Erg.csproj
```

## Controls

### Movement & Combat

| Key | Action |
|-----|--------|
| Arrow keys | Move in 4 directions |
| Numpad 1-9 | Move in 8 directions (diagonals included) |
| Numpad 5 | Wait a turn |

To attack a hostile creature, simply walk into it. The game will automatically engage in combat.

If you try to attack a non-hostile creature, you'll be asked to confirm with `Y` or `N`.

### Interaction

| Key | Action |
|-----|--------|
| `g` | Pick up items |
| `o` | Open door |
| `c` | Close door |
| `s` | Search for secret doors |

When multiple doors are adjacent, you'll be prompted to choose a direction. Doors cannot be closed if something is blocking them.

### Stairs

| Key | Action |
|-----|--------|
| `>` | Descend stairs |
| `<` | Ascend stairs |

At depth 1, ascending the stairs means escaping the dungeon.

### Examination

| Key | Action |
|-----|--------|
| `x` | Examine adjacent tile (choose direction) |
| `Alt+x` | Free-look mode (move cursor freely) |

### Screens

| Key | Action |
|-----|--------|
| `i` | Inventory |
| `#` | Skills |

### Messages

| Key | Action |
|-----|--------|
| `Space` | Continue to next message |
| `Enter` | Skip all messages |
| `Escape` | Cancel current action |

## Current State

This is an active work in progress. The dungeon generates, creatures roam, combat works, and you can die. Many features are still being added - inventory use, more enemy types, items with effects, and whatever else seems fun.

Feel free to poke around.

## Tech

- C# / .NET 9.0
- Swappable rendering backends:
  - **SFML.Net 3.0** (currently active)
  - **Raylib-cs 7.0.2** (available)
- Custom A* pathfinding and line-of-sight algorithms
- No external game frameworks - just the basics

## License

See LICENSE.txt
