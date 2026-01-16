# Erg

*A roguelike where things want to eat you.*

```
  ###·####·###
  #·········+#
  #··z··@····#
  +···········
  #··z·····j·#
  ###·########
```

## What is this?

Erg is a traditional roguelike in the spirit of the classics. Procedurally generated dungeons, permadeath (eventually), turn-based tactical combat, and everything rendered in glorious Unicode.

You descend into the depths. Zombies shamble through crypts. Amoebas lurk in underground pools...

## Features

**Dungeon Generation**
- Multi-level procedural dungeons with rooms, corridors, dead ends, and natural caves
- Special rooms: flooded chambers, crypts with graves, columned halls
- Secret doors hiding treasures

**Creatures**
- Zombies with actual AI - they scan their surroundings, track you with A* pathfinding, and occasionally forget what they were doing
- Amoebas dwelling in water, preferring the depths
- More to come...

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

| Key | Action |
|-----|--------|
| Arrow keys / Numpad | Move (8 directions) |
| `>` | Descend stairs |
| `<` | Ascend stairs |
| `g` | Pick up items |
| `i` | Inventory |
| `o` / `c` | Open / Close doors |
| `x` | Examine adjacent tile |
| `Alt+x` | Free-look examine mode |
| `Numpad 5` | Wait a turn |
| `Space` | Continue messages |

## Current State

This is an active work in progress. The dungeon generates, creatures roam, combat works, and you can die. Many features are still being added - inventory use, more enemy types, items with effects, and whatever else seems fun.

Feel free to poke around.

## Tech

- C# / .NET 9.0
- Raylib via raylib-cs for rendering
- Custom A* pathfinding and line-of-sight algorithms
- No external game frameworks - just the basics

## License

See LICENSE.txt
