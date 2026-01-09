# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Sparks is a C# particle system/effects engine built on Raylib-cs. It's designed as a modular, drop-in particle system for games and visualizations.

## Build Commands

```bash
dotnet build          # Build the project
dotnet run            # Run the demo application
dotnet clean          # Clean build artifacts
```

## Architecture

Single-file design in `Sparks/Sparks.cs`:

- **Easing** - Enum with 16 easing functions (Linear, Sine, Quad, Cubic, Expo, Back variants)
- **EasingFunctions** - Static class with `Apply(Easing, float t)` implementing easings.net formulas
- **Tween\<T\>** - Generic struct for interpolated values with implicit conversions:
  - `emitter.Size = 4f;` - constant value
  - `emitter.Size = (8f, 2f);` - interpolated range (tuple implicit)
  - `new Tween<float>(8f, 2f, Easing.EaseOutCubic)` - with custom easing
- **Particle** - Struct with Position, Velocity, Age, MaxAge, Delay, InitialSpeed; properties IsAlive, IsVisible, Progress
- **Emitter** - Class managing particles with Tween properties: Lifetime, Speed, Size, Color, Gravity, SpawnDelay

## Key Design Decisions

- **Particle as struct** - Performance, no GC allocations
- **Tween implicit conversions** - Simple API: `Size = 4f` or `Size = (8f, 2f)`
- **SpawnDelay** - Particle moves but is invisible until delay passes (for effects like smoke rising from fire)
- **AngleMin/AngleMax** - Emission direction in degrees (not SpreadAngle) for explicit control
- **Swap-remove** - O(1) particle removal instead of O(n) RemoveAt
- **Pre-allocated list** - `List<Particle>(1000)` to reduce reallocations during bursts
- **Back easing safety** - Size and Color are clamped to prevent negative values / byte overflow

## Property Semantics

Understanding which properties are randomized vs interpolated:

| Property | At spawn (random range) | Over lifetime (interpolated) |
|----------|:-----------------------:|:----------------------------:|
| Lifetime | Start...End | - |
| Speed | Start...End | scales by End/Start ratio |
| Size | - | Start -> End |
| Color | - | Start -> End |
| Gravity | - | Start -> End |
| SpawnDelay | Start...End | - |

**Speed is special**: it randomizes initial speed AND scales over lifetime. `Speed = (100, 50)` means random 50-100 at spawn, then slows to 50% of initial over lifetime.

## Demo Controls (Program.cs)

- `1` - Sparks effect
- `2` - Fire with smoke (multiple emitters)
- `3` - Water stream
- `4` - Fireworks (animated gravity demo)
- LMB - Burst emission (30 particles)
- RMB (hold) - Continuous emission (FPS-independent, 180/sec)

## Safety Features

- **MaxParticles** (default 10000) - prevents runaway particle count
- **RandomRange min>max tolerance** - automatically swaps if inverted
- **Division by zero guard** - Speed.Start checked before division
- **Back easing clamp** - Size >= 0, Color channels 0-255

## Determinism

Optional `Seed` property enables reproducible particle effects:
```csharp
emitter.Seed = 12345;  // deterministic
emitter.Seed = null;   // non-deterministic (default)
```

## Technical Details

- **Framework**: .NET 10.0
- **Graphics**: Raylib-cs 7.0.2
- **Nullable**: Enabled
- **XML Documentation**: Full coverage on all public APIs
