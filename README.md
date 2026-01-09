# Sparks

A simple, drop-in particle system for [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) games and applications.

## Features

- **Single-file design** - just copy `Sparks.cs` into your project
- **Simple API** - sensible defaults, works out of the box
- **Tween-based interpolation** - animate size, color, speed, and gravity over particle lifetime
- **16 easing functions** - from [easings.net](https://easings.net) (Linear, Sine, Quad, Cubic, Expo, Back)
- **Spawn delay** - particles move but stay invisible (for effects like smoke rising from fire)
- **Performance optimized** - struct-based particles, pre-allocated lists, O(1) particle removal
- **Deterministic mode** - optional seed for reproducible effects
- **Custom rendering** - plug in your own draw callback

## Installation

Copy `Sparks/Sparks.cs` into your Raylib-cs project. That's it.

## Quick Start

```csharp
using Raylib_cs;
using Sparks;

Raylib.InitWindow(800, 600, "Sparks Demo");
Raylib.SetTargetFPS(60);

var emitter = new Emitter();

while (!Raylib.WindowShouldClose())
{
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        emitter.Position = Raylib.GetMousePosition();
        emitter.Emit(20);
    }

    emitter.Update(Raylib.GetFrameTime());

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);
    emitter.Draw();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
```

## Configuration

All animated properties use `Tween<T>` with three assignment styles:

```csharp
emitter.Size = 4f;                                         // constant value
emitter.Size = (8f, 2f);                                   // interpolate from 8 to 2
emitter.Size = new Tween<float>(8f, 2f, Easing.EaseOutCubic);  // with custom easing
```

### Full Configuration Example

```csharp
var emitter = new Emitter
{
    // Lifetime: random per particle (1-2 seconds)
    Lifetime = (1.0f, 2.0f),

    // Speed: random at spawn + scales over lifetime by ratio End/Start
    // (100, 50) = random 50-100 speed, slows to 50% over lifetime
    Speed = (50f, 150f),

    // Size: interpolated over lifetime (shrinks from 6 to 2)
    Size = (6f, 2f),

    // Color: interpolated over lifetime (yellow fades to red)
    Color = (Color.Yellow, Color.Red),

    // Gravity: can be constant or interpolated (positive = down)
    Gravity = 200f,

    // Emission direction in degrees (0=right, 90=down, 180=left, 270=up)
    AngleMin = 0f,
    AngleMax = 360f,

    // Delay before particle becomes visible (still moves during delay)
    SpawnDelay = 0f,

    // Safety limit (default 10000)
    MaxParticles = 5000,

    // Optional: deterministic random (for replays/testing)
    Seed = 12345,
};
```

## Property Behavior

| Property | Per-particle random | Interpolated over lifetime |
|----------|:------------------:|:--------------------------:|
| Lifetime | Start...End range | - |
| Speed | Start...End range | scales by End/Start ratio |
| Size | - | Start -> End |
| Color | - | Start -> End |
| Gravity | - | Start -> End |
| SpawnDelay | Start...End range | - |

## Available Easings

```
Linear (default)
EaseInSine,  EaseOutSine,  EaseInOutSine
EaseInQuad,  EaseOutQuad,  EaseInOutQuad
EaseInCubic, EaseOutCubic, EaseInOutCubic
EaseInExpo,  EaseOutExpo,  EaseInOutExpo
EaseInBack,  EaseOutBack,  EaseInOutBack
```

**Note:** Back easings overshoot (values < 0 or > 1). Size and color are automatically clamped to prevent visual glitches.

## Examples

### Sparks

```csharp
var sparks = new Emitter
{
    Lifetime = (1.0f, 2.0f),
    Speed = (50f, 150f),
    Size = (6f, 2f),
    Color = (Color.Yellow, Color.Red),
    Gravity = 200f,
    AngleMin = 0f,
    AngleMax = 360f,
};
```

### Fire with Smoke

```csharp
// Fire core
var fire = new Emitter
{
    Lifetime = (0.3f, 0.6f),
    Speed = (80f, 120f),
    Size = (8f, 4f),
    Color = (Color.Yellow, Color.Orange),
    Gravity = -150f,  // negative = goes up
    AngleMin = 250f,
    AngleMax = 290f,
};

// Smoke (appears above fire thanks to SpawnDelay)
var smoke = new Emitter
{
    Lifetime = (1.0f, 2.0f),
    Speed = (40f, 60f),
    Size = (4f, 14f),  // grows as it rises
    Color = (new Color(100, 100, 100, 180), new Color(50, 50, 50, 0)),
    Gravity = -40f,
    AngleMin = 245f,
    AngleMax = 295f,
    SpawnDelay = (0.25f, 0.5f),  // invisible while traveling up from fire
};
```

### Water Stream

```csharp
var water = new Emitter
{
    Lifetime = (1.5f, 2.5f),
    Speed = (300f, 350f),
    Size = (5f, 3f),
    Color = (new Color(100, 180, 255, 255), new Color(50, 120, 200, 150)),
    Gravity = 400f,
    AngleMin = -30f,
    AngleMax = -10f,  // angled stream to the right
};
```

### Fireworks (with animated gravity)

```csharp
var fireworks = new Emitter
{
    Lifetime = (1.5f, 2.5f),
    Speed = (150f, 250f),
    Size = (5f, 2f),
    Color = (Color.White, Color.Magenta),
    // Gravity changes over lifetime: negative (rises) -> positive (falls)
    // Creates the characteristic firework arc
    Gravity = new Tween<float>(-200f, 400f, Easing.EaseInQuad),
    AngleMin = 230f,
    AngleMax = 310f,
};

// Glitter that hovers then slowly falls
var glitter = new Emitter
{
    Lifetime = (2.0f, 3.0f),
    Speed = (50f, 100f),
    Size = (2f, 1f),
    Color = (Color.Gold, new Color(255, 200, 100, 0)),
    // Starts with zero gravity, exponentially increases
    Gravity = new Tween<float>(0f, 150f, Easing.EaseInExpo),
    AngleMin = 0f,
    AngleMax = 360f,
};
```

## Custom Rendering

Override the default square rendering:

```csharp
// Circles
emitter.CustomDraw = (particle, color, size) =>
{
    Raylib.DrawCircle((int)particle.Position.X, (int)particle.Position.Y, size, color);
};

// Soft glow (additive would need custom blend mode)
emitter.CustomDraw = (particle, color, size) =>
{
    Raylib.DrawCircleGradient(
        (int)particle.Position.X,
        (int)particle.Position.Y,
        size * 2,
        color,
        new Color(color.R, color.G, color.B, 0)
    );
};
```

## FPS-Independent Continuous Emission

For smooth continuous emission regardless of frame rate:

```csharp
float emitAccumulator = 0f;
const float emitRate = 180f;  // particles per second

// In game loop:
if (Raylib.IsMouseButtonDown(MouseButton.Right))
{
    emitAccumulator += emitRate * deltaTime;
    int toEmit = (int)emitAccumulator;
    emitAccumulator -= toEmit;

    if (toEmit > 0)
    {
        emitter.Position = Raylib.GetMousePosition();
        emitter.Emit(toEmit);
    }
}
else
{
    emitAccumulator = 0f;
}
```

## API Reference

### Emitter

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Position | Vector2 | (0,0) | Spawn position |
| Lifetime | Tween\<float\> | 2.0 | Particle lifetime in seconds (random range) |
| Speed | Tween\<float\> | 100 | Initial speed + lifetime scaling |
| Size | Tween\<float\> | 4 | Particle size in pixels |
| Color | Tween\<Color\> | White | Particle color with alpha |
| Gravity | Tween\<float\> | 0 | Vertical acceleration (px/s^2) |
| SpawnDelay | Tween\<float\> | 0 | Visibility delay in seconds |
| AngleMin/Max | float | 0/360 | Emission angle range in degrees |
| MaxParticles | int | 10000 | Hard limit on particle count |
| Seed | int? | null | Random seed (null = non-deterministic) |
| CustomDraw | Action | null | Custom render callback |
| ParticleCount | int | - | Current active particles (read-only) |

| Method | Description |
|--------|-------------|
| Emit(count) | Spawn particles at current Position |
| Update(dt) | Advance simulation by dt seconds |
| Draw() | Render all visible particles |
| Clear() | Remove all particles |

## Requirements

- .NET 6.0+ (tested on .NET 10.0)
- Raylib-cs 7.0+

## Running the Demo

```bash
cd Sparks
dotnet run
```

Controls:
- `1` - Sparks effect
- `2` - Fire with smoke
- `3` - Water stream
- `4` - Fireworks (animated gravity)
- `LMB` - Burst emission
- `RMB` (hold) - Continuous emission

## License

Public Domain. See [LICENSE.txt](LICENSE.txt).
