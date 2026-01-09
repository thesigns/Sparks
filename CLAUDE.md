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
- **Particle** - Struct with Position, Velocity, Age, MaxAge, Delay; properties IsAlive, IsVisible, Progress
- **Emitter** - Class managing particles with Tween properties: Lifetime, Speed, Size, Color, Gravity, SpawnDelay

## Key Design Decisions

- **Particle as struct** - Performance, no GC allocations
- **Tween implicit conversions** - Simple API: `Size = 4f` or `Size = (8f, 2f)`
- **SpawnDelay** - Particle moves but is invisible until delay passes (for effects like smoke rising from fire)
- **AngleMin/AngleMax** - Emission direction in degrees (not SpreadAngle) for explicit control

## Demo Controls (Program.cs)

- `1` - Sparks effect
- `2` - Fire with smoke (multiple emitters)
- `3` - Water stream
- LMB - Burst emission
- RMB (hold) - Continuous emission

## Technical Details

- **Framework**: .NET 10.0
- **Graphics**: Raylib-cs 7.0.2
- **Nullable**: Enabled
