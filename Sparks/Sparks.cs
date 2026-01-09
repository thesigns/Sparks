using System.Numerics;
using Raylib_cs;

namespace Sparks;

// Particle system for Raylib_cs.
// Everything in one drop-in file.

#region Easing

/// <summary>
/// Easing functions for smooth interpolation. Based on easings.net formulas.
/// </summary>
public enum Easing
{
    /// <summary>No easing, constant rate of change.</summary>
    Linear,
    /// <summary>Slow start using sine curve.</summary>
    EaseInSine,
    /// <summary>Slow end using sine curve.</summary>
    EaseOutSine,
    /// <summary>Slow start and end using sine curve.</summary>
    EaseInOutSine,
    /// <summary>Slow start using quadratic curve (t^2).</summary>
    EaseInQuad,
    /// <summary>Slow end using quadratic curve.</summary>
    EaseOutQuad,
    /// <summary>Slow start and end using quadratic curve.</summary>
    EaseInOutQuad,
    /// <summary>Slow start using cubic curve (t^3).</summary>
    EaseInCubic,
    /// <summary>Slow end using cubic curve.</summary>
    EaseOutCubic,
    /// <summary>Slow start and end using cubic curve.</summary>
    EaseInOutCubic,
    /// <summary>Very slow start using exponential curve.</summary>
    EaseInExpo,
    /// <summary>Very slow end using exponential curve.</summary>
    EaseOutExpo,
    /// <summary>Very slow start and end using exponential curve.</summary>
    EaseInOutExpo,
    /// <summary>Slight overshoot at start (pulls back then accelerates).</summary>
    EaseInBack,
    /// <summary>Slight overshoot at end (overshoots then settles).</summary>
    EaseOutBack,
    /// <summary>Slight overshoot at both start and end.</summary>
    EaseInOutBack,
}

/// <summary>
/// Static class providing easing function implementations.
/// </summary>
public static class EasingFunctions
{
    private const float PI = MathF.PI;
    private const float C1 = 1.70158f;
    private const float C3 = C1 + 1f;

    /// <summary>
    /// Applies an easing function to a normalized time value.
    /// </summary>
    /// <param name="easing">The easing function to apply.</param>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <returns>Eased value (0 to 1, may exceed bounds for Back easings).</returns>
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

/// <summary>
/// A generic struct for interpolated values over time.
/// Supports implicit conversion from single values and tuples for convenient API.
/// </summary>
/// <typeparam name="T">The value type (float, Color, etc.).</typeparam>
/// <example>
/// <code>
/// emitter.Size = 4f;              // Constant value
/// emitter.Size = (8f, 2f);        // Interpolate from 8 to 2
/// emitter.Size = new Tween&lt;float&gt;(8f, 2f, Easing.EaseOutCubic); // With custom easing
/// </code>
/// </example>
public struct Tween<T> where T : struct
{
    /// <summary>The starting value at progress 0.</summary>
    public T Start;

    /// <summary>The ending value at progress 1.</summary>
    public T End;

    /// <summary>The easing function used for interpolation.</summary>
    public Easing Easing;

    /// <summary>
    /// Creates a tween with a constant value (Start == End).
    /// </summary>
    /// <param name="value">The constant value.</param>
    public Tween(T value)
    {
        Start = value;
        End = value;
        Easing = Easing.Linear;
    }

    /// <summary>
    /// Creates a tween that interpolates between two values.
    /// </summary>
    /// <param name="start">The starting value.</param>
    /// <param name="end">The ending value.</param>
    /// <param name="easing">The easing function (default: Linear).</param>
    public Tween(T start, T end, Easing easing = Easing.Linear)
    {
        Start = start;
        End = end;
        Easing = easing;
    }

    /// <summary>Implicit conversion from a single value to a constant tween.</summary>
    public static implicit operator Tween<T>(T value) => new(value);

    /// <summary>Implicit conversion from a tuple to an interpolating tween with Linear easing.</summary>
    public static implicit operator Tween<T>((T start, T end) tuple) => new(tuple.start, tuple.end);
}

#endregion

#region Particle

/// <summary>
/// Represents a single particle in the system. Stored as a struct for performance (no GC allocations).
/// </summary>
public struct Particle
{
    /// <summary>Current position in world space.</summary>
    public Vector2 Position;

    /// <summary>Current velocity (direction and speed combined).</summary>
    public Vector2 Velocity;

    /// <summary>Time elapsed since spawn (in seconds).</summary>
    public float Age;

    /// <summary>Total lifetime including spawn delay (in seconds).</summary>
    public float MaxAge;

    /// <summary>
    /// Time before the particle becomes visible (in seconds).
    /// Particle still moves during delay, useful for effects like smoke rising from fire.
    /// </summary>
    public float Delay;

    /// <summary>Speed at spawn time, used for speed tweening calculations.</summary>
    public float InitialSpeed;

    /// <summary>True if the particle is still alive (Age &lt; MaxAge).</summary>
    public readonly bool IsAlive => Age < MaxAge;

    /// <summary>True if the particle should be rendered (Age &gt;= Delay).</summary>
    public readonly bool IsVisible => Age >= Delay;

    /// <summary>
    /// Normalized lifetime progress (0 to 1), excluding spawn delay.
    /// Used for interpolating Size, Color, and other tweened properties.
    /// </summary>
    public readonly float Progress => (MaxAge - Delay) > 0f ? MathF.Max(0f, Age - Delay) / (MaxAge - Delay) : 1f;
}

#endregion

#region Emitter

/// <summary>
/// Manages a collection of particles with configurable emission, physics, and rendering.
/// All particle properties support tweening (interpolation over lifetime).
/// </summary>
/// <example>
/// <code>
/// var emitter = new Emitter(400, 300);
/// emitter.Lifetime = 2f;
/// emitter.Size = (8f, 2f);  // Shrink over time
/// emitter.Color = (Color.Yellow, Color.Red);  // Yellow to red
/// emitter.Gravity = 200f;
/// emitter.Emit(50);
/// </code>
/// </example>
public class Emitter
{
    /// <summary>World position where new particles are spawned.</summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Particle lifetime in seconds. Start/End define the random range for each particle.
    /// </summary>
    public Tween<float> Lifetime { get; set; } = 2.0f;

    /// <summary>
    /// Initial particle speed in pixels/second.
    /// <para><b>Randomization:</b> Each particle gets a random speed between Start and End at spawn.</para>
    /// <para><b>Tweening:</b> If Start != End, particle speed is scaled over lifetime by factor (End/Start).
    /// For example, Speed = (100, 50) means particles slow down to 50% of initial speed.</para>
    /// <para><b>Easing:</b> Controls how quickly the speed changes over time.</para>
    /// </summary>
    public Tween<float> Speed { get; set; } = 100f;

    /// <summary>
    /// Particle size in pixels. Interpolated from Start to End over lifetime.
    /// </summary>
    public Tween<float> Size { get; set; } = 4f;

    /// <summary>
    /// Particle color. Interpolated from Start to End over lifetime (including alpha).
    /// </summary>
    public Tween<Color> Color { get; set; } = Raylib_cs.Color.White;

    /// <summary>
    /// Vertical acceleration in pixels/second^2. Positive = down, negative = up.
    /// <para><b>Tweening:</b> Gravity force is interpolated from Start to End over particle lifetime.</para>
    /// <para><b>Example:</b> Gravity = (-100, 200) with EaseInQuad creates particles that float up
    /// initially, then accelerate downward as they age.</para>
    /// </summary>
    public Tween<float> Gravity { get; set; } = 0f;

    /// <summary>
    /// Delay in seconds before particle becomes visible after spawn.
    /// <para>Particle still moves during delay - useful for effects like smoke rising from fire,
    /// where smoke should appear above the flames, not at the flame source.</para>
    /// <para>Start/End define random range for each particle.</para>
    /// </summary>
    public Tween<float> SpawnDelay { get; set; } = 0f;

    /// <summary>Minimum emission angle in degrees (0 = right, 90 = down, 180 = left, 270 = up).</summary>
    public float AngleMin { get; set; } = 0f;

    /// <summary>Maximum emission angle in degrees.</summary>
    public float AngleMax { get; set; } = 360f;

    /// <summary>
    /// Maximum number of particles this emitter can hold. New particles are not spawned when limit is reached.
    /// </summary>
    public int MaxParticles { get; set; } = 10000;

    /// <summary>
    /// Optional random seed for deterministic particle emission. Set to null for non-deterministic behavior.
    /// Setting this property resets the random number generator.
    /// </summary>
    public int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _random = value.HasValue ? new Random(value.Value) : new Random();
        }
    }
    private int? _seed;

    /// <summary>
    /// Custom draw callback for particles. If null, particles are drawn as squares (Quake 1 style).
    /// Parameters: (Particle particle, Color color, float size).
    /// </summary>
    public Action<Particle, Color, float>? CustomDraw { get; set; }

    /// <summary>Current number of active particles.</summary>
    public int ParticleCount => _particles.Count;

    private readonly List<Particle> _particles = new(1000);  // Pre-allocated to reduce reallocations during bursts
    private Random _random = new();

    /// <summary>Creates an emitter at position (0, 0).</summary>
    public Emitter() { }

    /// <summary>Creates an emitter at the specified position.</summary>
    /// <param name="x">X coordinate in world space.</param>
    /// <param name="y">Y coordinate in world space.</param>
    public Emitter(float x, float y)
    {
        Position = new Vector2(x, y);
    }

    /// <summary>Creates an emitter at the specified position.</summary>
    /// <param name="position">Position in world space.</param>
    public Emitter(Vector2 position)
    {
        Position = position;
    }

    /// <summary>
    /// Spawns new particles at the emitter's current position.
    /// Particles will not spawn if <see cref="MaxParticles"/> limit is reached.
    /// </summary>
    /// <param name="count">Number of particles to spawn.</param>
    public void Emit(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_particles.Count >= MaxParticles) break;
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
                Delay = delay,
                InitialSpeed = speed
            };

            _particles.Add(particle);
        }
    }

    /// <summary>
    /// Updates all particles: aging, physics (gravity, speed), and movement.
    /// Dead particles are automatically removed.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public void Update(float deltaTime)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];

            // Age
            p.Age += deltaTime;

            if (!p.IsAlive)
            {
                // Swap-remove optimization: O(1) instead of O(n)
                _particles[i] = _particles[^1];
                _particles.RemoveAt(_particles.Count - 1);
                continue;
            }

            // Apply gravity
            float progress = p.Progress;
            float gravityT = EasingFunctions.Apply(Gravity.Easing, progress);
            float gravity = Lerp(Gravity.Start, Gravity.End, gravityT);
            p.Velocity.Y += gravity * deltaTime;

            // Apply speed scaling (tween affects magnitude, not direction)
            // Check Speed.Start first to avoid division by zero
            if (MathF.Abs(Speed.Start) > 0.001f)
            {
                float speedFactor = Speed.End / Speed.Start;
                if (float.IsFinite(speedFactor) && MathF.Abs(1f - speedFactor) > 0.001f)
                {
                    float speedT = EasingFunctions.Apply(Speed.Easing, progress);
                    float targetSpeed = Lerp(p.InitialSpeed, p.InitialSpeed * speedFactor, speedT);
                    float currentSpeed = p.Velocity.Length();
                    if (currentSpeed > 0.001f)
                    {
                        p.Velocity = Vector2.Normalize(p.Velocity) * targetSpeed;
                    }
                }
            }

            // Move
            p.Position += p.Velocity * deltaTime;

            _particles[i] = p;
        }
    }

    /// <summary>
    /// Renders all visible particles. Uses <see cref="CustomDraw"/> if set, otherwise draws squares.
    /// </summary>
    public void Draw()
    {
        foreach (var p in _particles)
        {
            if (!p.IsVisible) continue;

            float progress = p.Progress;

            // Interpolate size (clamp to prevent negative values from Back easing overshoot)
            float sizeT = EasingFunctions.Apply(Size.Easing, progress);
            float size = MathF.Max(0f, Lerp(Size.Start, Size.End, sizeT));

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

    /// <summary>
    /// Removes all particles from the emitter.
    /// </summary>
    public void Clear()
    {
        _particles.Clear();
    }

    private float RandomRange(float min, float max)
    {
        // Swap if min > max to handle inverted ranges gracefully
        if (min > max) (min, max) = (max, min);
        if (MathF.Abs(max - min) < 0.0001f) return min;
        return min + (float)_random.NextDouble() * (max - min);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        // Clamp to prevent overflow from Back easing overshoot
        return new Color(
            (byte)Math.Clamp((int)(a.R + (b.R - a.R) * t), 0, 255),
            (byte)Math.Clamp((int)(a.G + (b.G - a.G) * t), 0, 255),
            (byte)Math.Clamp((int)(a.B + (b.B - a.B) * t), 0, 255),
            (byte)Math.Clamp((int)(a.A + (b.A - a.A) * t), 0, 255)
        );
    }
}

#endregion
