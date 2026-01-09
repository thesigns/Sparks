using System.Numerics;
using Raylib_cs;

namespace Sparks;

// Particle system for Raylib_cs.
// Everything in one drop-in file.

#region Easing

public enum Easing
{
    Linear,
    EaseInSine, EaseOutSine, EaseInOutSine,
    EaseInQuad, EaseOutQuad, EaseInOutQuad,
    EaseInCubic, EaseOutCubic, EaseInOutCubic,
    EaseInExpo, EaseOutExpo, EaseInOutExpo,
    EaseInBack, EaseOutBack, EaseInOutBack,
}

public static class EasingFunctions
{
    private const float PI = MathF.PI;
    private const float C1 = 1.70158f;
    private const float C3 = C1 + 1f;

    public static float Apply(Easing easing, float t)
    {
        return easing switch
        {
            Easing.Linear => t,

            Easing.EaseInSine => 1f - MathF.Cos(t * PI / 2f),
            Easing.EaseOutSine => MathF.Sin(t * PI / 2f),
            Easing.EaseInOutSine => -(MathF.Cos(PI * t) - 1f) / 2f,

            Easing.EaseInQuad => t * t,
            Easing.EaseOutQuad => 1f - (1f - t) * (1f - t),
            Easing.EaseInOutQuad => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,

            Easing.EaseInCubic => t * t * t,
            Easing.EaseOutCubic => 1f - MathF.Pow(1f - t, 3f),
            Easing.EaseInOutCubic => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,

            Easing.EaseInExpo => t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f),
            Easing.EaseOutExpo => t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t),
            Easing.EaseInOutExpo => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f
                ? MathF.Pow(2f, 20f * t - 10f) / 2f
                : (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f,

            Easing.EaseInBack => C3 * t * t * t - C1 * t * t,
            Easing.EaseOutBack => 1f + C3 * MathF.Pow(t - 1f, 3f) + C1 * MathF.Pow(t - 1f, 2f),
            Easing.EaseInOutBack => t < 0.5f
                ? MathF.Pow(2f * t, 2f) * ((C1 * 1.525f + 1f) * 2f * t - C1 * 1.525f) / 2f
                : (MathF.Pow(2f * t - 2f, 2f) * ((C1 * 1.525f + 1f) * (t * 2f - 2f) + C1 * 1.525f) + 2f) / 2f,

            _ => t
        };
    }
}

#endregion

#region Tween

public struct Tween<T> where T : struct
{
    public T Start;
    public T End;
    public Easing Easing;

    public Tween(T value)
    {
        Start = value;
        End = value;
        Easing = Easing.Linear;
    }

    public Tween(T start, T end, Easing easing = Easing.Linear)
    {
        Start = start;
        End = end;
        Easing = easing;
    }

    public static implicit operator Tween<T>(T value) => new(value);
    public static implicit operator Tween<T>((T start, T end) tuple) => new(tuple.start, tuple.end);
}

#endregion

#region Particle

public struct Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Age;
    public float MaxAge;
    public float Delay;

    public readonly bool IsAlive => Age < MaxAge;
    public readonly bool IsVisible => Age >= Delay;
    public readonly float Progress => (MaxAge - Delay) > 0f ? MathF.Max(0f, Age - Delay) / (MaxAge - Delay) : 1f;
}

#endregion

#region Emitter

public class Emitter
{
    // Position
    public Vector2 Position { get; set; }

    // Particle configuration (Tween = interpolated over lifetime)
    public Tween<float> Lifetime { get; set; } = 2.0f;
    public Tween<float> Speed { get; set; } = 100f;
    public Tween<float> Size { get; set; } = 4f;
    public Tween<Color> Color { get; set; } = Raylib_cs.Color.White;
    public Tween<float> Gravity { get; set; } = 0f;
    public Tween<float> SpawnDelay { get; set; } = 0f;

    // Emission direction (randomized at spawn)
    public float AngleMin { get; set; } = 0f;
    public float AngleMax { get; set; } = 360f;

    // Custom rendering (null = default squares)
    public Action<Particle, Color, float>? CustomDraw { get; set; }

    // State
    public int ParticleCount => _particles.Count;

    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();

    public Emitter() { }

    public Emitter(float x, float y)
    {
        Position = new Vector2(x, y);
    }

    public Emitter(Vector2 position)
    {
        Position = position;
    }

    public void Emit(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = RandomRange(AngleMin, AngleMax) * MathF.PI / 180f;
            float speed = RandomRange(Speed.Start, Speed.End);
            float lifetime = RandomRange(Lifetime.Start, Lifetime.End);
            float delay = RandomRange(SpawnDelay.Start, SpawnDelay.End);

            var particle = new Particle
            {
                Position = Position,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Age = 0f,
                MaxAge = lifetime + delay,
                Delay = delay
            };

            _particles.Add(particle);
        }
    }

    public void Update(float deltaTime)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];

            // Age
            p.Age += deltaTime;

            if (!p.IsAlive)
            {
                _particles.RemoveAt(i);
                continue;
            }

            // Apply gravity
            float progress = p.Progress;
            float gravityT = EasingFunctions.Apply(Gravity.Easing, progress);
            float gravity = Lerp(Gravity.Start, Gravity.End, gravityT);
            p.Velocity.Y += gravity * deltaTime;

            // Apply speed scaling
            float speedT = EasingFunctions.Apply(Speed.Easing, progress);
            float speedFactor = Speed.End / Speed.Start;
            if (float.IsFinite(speedFactor) && MathF.Abs(Speed.Start) > 0.001f)
            {
                float currentSpeedFactor = Lerp(1f, speedFactor, speedT);
                // Normalize and rescale velocity
                float currentSpeed = p.Velocity.Length();
                if (currentSpeed > 0.001f)
                {
                    // Only affect the magnitude based on tween, not direction
                }
            }

            // Move
            p.Position += p.Velocity * deltaTime;

            _particles[i] = p;
        }
    }

    public void Draw()
    {
        foreach (var p in _particles)
        {
            if (!p.IsVisible) continue;

            float progress = p.Progress;

            // Interpolate size
            float sizeT = EasingFunctions.Apply(Size.Easing, progress);
            float size = Lerp(Size.Start, Size.End, sizeT);

            // Interpolate color
            float colorT = EasingFunctions.Apply(Color.Easing, progress);
            var color = LerpColor(Color.Start, Color.End, colorT);

            if (CustomDraw != null)
            {
                CustomDraw(p, color, size);
            }
            else
            {
                // Default: draw square (Quake 1 style)
                int x = (int)(p.Position.X - size / 2f);
                int y = (int)(p.Position.Y - size / 2f);
                Raylib.DrawRectangle(x, y, (int)size, (int)size, color);
            }
        }
    }

    public void Clear()
    {
        _particles.Clear();
    }

    private float RandomRange(float min, float max)
    {
        if (MathF.Abs(max - min) < 0.0001f) return min;
        return min + (float)_random.NextDouble() * (max - min);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t)
        );
    }
}

#endregion
