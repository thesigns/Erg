# Erg

A classic roguelike game written in C# with Raylib.

## About

Erg is a traditional roguelike featuring procedurally generated dungeons, turn-based gameplay, and text-based graphics (Unicode) rendered through Raylib. Explore deeper levels, collect items, and discover what lies beneath.

**Work in progress** - core mechanics are being developed.

## Features

- Procedurally generated dungeons with rooms, corridors, and special areas
- Field of view with shadowcasting algorithm
- Multiple tile types: doors (open/closed/secret), water, graves, stairs
- Item system with stacking support
- Message log with pagination

## Requirements

- .NET 9.0
- Raylib (via raylib-cs NuGet package)

## Build & Run

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
| `o` | Open door |
| `c` | Close door |
| `x` | Examine |
| `Space` | Continue messages |

## License

See LICENSE.txt
