# Sparks

A simple, drop-in particle system for [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) games and applications.

## Features

- Single-file, drop-in design - just add `Sparks.cs` to your project
- Simple, idiot-proof API with sensible defaults
- Tween-based property interpolation (size, color, speed, gravity)
- 16 built-in easing functions (from [easings.net](https://easings.net))
- Spawn delay for advanced effects (e.g., smoke appearing above fire)
- Custom rendering support

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
    // Emit on click
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        emitter.Position = Raylib.GetMousePosition();
        emitter.Emit(20);
    }

    // Update
    emitter.Update(Raylib.GetFrameTime());

    // Draw
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);
    emitter.Draw();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
```

## Configuration

All properties use `Tween<T>` which supports:
- Single value: `emitter.Size = 4f;` (constant)
- Interpolated range: `emitter.Size = (8f, 2f);` (shrinks over lifetime)
- Custom easing: `emitter.Size = new Tween<float>(8f, 2f, Easing.EaseOutCubic);`

```csharp
var emitter = new Emitter
{
    Lifetime = (1.0f, 2.0f),           // random lifetime 1-2 seconds
    Speed = (50f, 150f),               // random initial speed
    Size = (6f, 2f),                   // shrinks from 6 to 2
    Color = (Color.Yellow, Color.Red), // yellow -> red fade
    Gravity = 200f,                    // falls down
    AngleMin = 0f,                     // emission direction
    AngleMax = 360f,                   // (0-360 = all directions)
    SpawnDelay = 0f,                   // delay before particle becomes visible
};
```

## Available Easings

```
Linear (default)
EaseInSine, EaseOutSine, EaseInOutSine
EaseInQuad, EaseOutQuad, EaseInOutQuad
EaseInCubic, EaseOutCubic, EaseInOutCubic
EaseInExpo, EaseOutExpo, EaseInOutExpo
EaseInBack, EaseOutBack, EaseInOutBack
```

## Examples

### Fire with Smoke

```csharp
// Fire core
var fire = new Emitter
{
    Lifetime = (0.3f, 0.6f),
    Speed = (80f, 120f),
    Size = (8f, 4f),
    Color = (Color.Yellow, Color.Orange),
    Gravity = -150f,  // goes up
    AngleMin = 250f,
    AngleMax = 290f,
};

// Smoke (delayed appearance)
var smoke = new Emitter
{
    Lifetime = (1.0f, 2.0f),
    Speed = (40f, 60f),
    Size = (4f, 14f),
    Color = (new Color(100, 100, 100, 180), new Color(50, 50, 50, 0)),
    Gravity = -40f,
    AngleMin = 255f,
    AngleMax = 285f,
    SpawnDelay = (0.25f, 0.5f),  // appears after rising
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
    AngleMax = -10f,  // angled stream
};
```

## Custom Rendering

```csharp
emitter.CustomDraw = (particle, color, size) =>
{
    Raylib.DrawCircle((int)particle.Position.X, (int)particle.Position.Y, size, color);
};
```

## Requirements

- .NET 6.0+
- Raylib-cs 7.0+

## License

Public Domain. See [LICENSE.txt](LICENSE.txt).
