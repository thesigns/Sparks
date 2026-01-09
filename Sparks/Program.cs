using Raylib_cs;
using Sparks;

Raylib.InitWindow(1920, 1080, "Sparks Demo");
Raylib.SetTargetFPS(60);

// Emitters list (some effects use multiple emitters)
var emitters = new List<Emitter>();
string currentEffect = "Sparks";

void SetupSparks()
{
    emitters.Clear();
    currentEffect = "1: Sparks";
    emitters.Add(new Emitter
    {
        Lifetime = (1.0f, 2.0f),
        Speed = (50f, 150f),
        Size = (6f, 2f),
        Color = (Color.Yellow, Color.Red),
        Gravity = 200f,
        AngleMin = 0f,
        AngleMax = 360f,
    });
}

void SetupFire()
{
    emitters.Clear();
    currentEffect = "2: Fire";

    // Fire core - bright yellow/orange
    emitters.Add(new Emitter
    {
        Lifetime = (0.3f, 0.6f),
        Speed = (80f, 120f),
        Size = (8f, 4f),
        Color = (Color.Yellow, Color.Orange),
        Gravity = -150f,  // fire goes up
        AngleMin = 250f,
        AngleMax = 290f,
    });

    // Fire outer - orange/red
    emitters.Add(new Emitter
    {
        Lifetime = (0.4f, 0.8f),
        Speed = (60f, 100f),
        Size = (10f, 6f),
        Color = (Color.Orange, Color.Red),
        Gravity = -120f,
        AngleMin = 240f,
        AngleMax = 300f,
    });

    // Smoke - gray, slower, longer life, delayed appearance
    emitters.Add(new Emitter
    {
        Lifetime = (1.0f, 2.0f),
        Speed = (40f, 60f),
        Size = (4f, 14f),
        Color = (new Color(100, 100, 100, 180), new Color(50, 50, 50, 0)),
        Gravity = -40f,
        AngleMin = 245f,
        AngleMax = 295f,
        SpawnDelay = (0.25f, 1.25f),  // appears after traveling up
    });
}

void SetupWater()
{
    emitters.Clear();
    currentEffect = "3: Water";

    // Water stream
    emitters.Add(new Emitter
    {
        Lifetime = (1.5f, 2.5f),
        Speed = (300f, 350f),
        Size = (5f, 3f),
        Color = (new Color(100, 180, 255, 255), new Color(50, 120, 200, 150)),
        Gravity = 400f,  // water falls
        AngleMin = -30f,
        AngleMax = -10f,  // angled stream to the right
    });

    // Water droplets/splash
    emitters.Add(new Emitter
    {
        Lifetime = (0.8f, 1.2f),
        Speed = (200f, 280f),
        Size = (3f, 2f),
        Color = (new Color(150, 200, 255, 200), new Color(100, 150, 220, 50)),
        Gravity = 500f,
        AngleMin = -40f,
        AngleMax = 0f,
    });
}

// Start with sparks
SetupSparks();

while (Raylib.WindowShouldClose() == false)
{
    float dt = Raylib.GetFrameTime();

    // Switch effects with number keys
    if (Raylib.IsKeyPressed(KeyboardKey.One)) SetupSparks();
    if (Raylib.IsKeyPressed(KeyboardKey.Two)) SetupFire();
    if (Raylib.IsKeyPressed(KeyboardKey.Three)) SetupWater();

    // Emit particles on mouse click
    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        var pos = Raylib.GetMousePosition();
        foreach (var e in emitters)
        {
            e.Position = pos;
            e.Emit(30);
        }
    }

    // Hold right mouse button for continuous emission
    if (Raylib.IsMouseButtonDown(MouseButton.Right))
    {
        var pos = Raylib.GetMousePosition();
        foreach (var e in emitters)
        {
            e.Position = pos;
            e.Emit(3);
        }
    }

    // Update all emitters
    foreach (var e in emitters)
        e.Update(dt);

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    // Draw all emitters
    foreach (var e in emitters)
        e.Draw();

    int totalParticles = emitters.Sum(e => e.ParticleCount);
    Raylib.DrawText($"Particles: {totalParticles}", 10, 10, 20, Color.White);
    Raylib.DrawText($"Effect: {currentEffect}", 10, 40, 20, Color.White);
    Raylib.DrawText("1: Sparks | 2: Fire | 3: Water", 10, 70, 16, Color.Gray);
    Raylib.DrawText("LMB: burst | RMB: continuous", 10, 95, 16, Color.Gray);

    Raylib.EndDrawing();
}

Raylib.CloseWindow();
