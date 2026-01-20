using Erg.Core.Abstractions;
using SFML.Window;
using static Raylib_cs.Raylib;

namespace Erg.Platforms.Raylib;

public class RaylibInput : IInput
{
    public Dictionary<KeyboardKey, TimeSpan> KeyLastPressTime { get; } = new();
    public Dictionary<KeyboardKey, bool> KeyPulse { get; } = new();
    public Dictionary<KeyboardKey, bool> KeyHeld { get; } = new();
    
    private Dictionary<KeyboardKey, TimeSpan> _keyPressTime = new();
    private Dictionary<KeyboardKey, TimeSpan> _keyLastRepeatTime = new();
    
    private static readonly TimeSpan InitialRepeatDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan RepeatInterval = TimeSpan.FromMilliseconds(100);

    public void Update()
    {
        var now = TimeSpan.FromSeconds(GetTime());
        
        UpdateKey(KeyboardKey.Insert, Raylib_cs.KeyboardKey.Insert, now);
        UpdateKey(KeyboardKey.Delete, Raylib_cs.KeyboardKey.Delete, now);
        UpdateKey(KeyboardKey.Home, Raylib_cs.KeyboardKey.Home, now);
        UpdateKey(KeyboardKey.End, Raylib_cs.KeyboardKey.End, now);
        UpdateKey(KeyboardKey.PageUp, Raylib_cs.KeyboardKey.PageUp, now);
        UpdateKey(KeyboardKey.PageDown, Raylib_cs.KeyboardKey.PageDown, now);
        
        UpdateKey(KeyboardKey.Left, Raylib_cs.KeyboardKey.Left, now);
        UpdateKey(KeyboardKey.Right, Raylib_cs.KeyboardKey.Right, now);
        UpdateKey(KeyboardKey.Up, Raylib_cs.KeyboardKey.Up, now);
        UpdateKey(KeyboardKey.Down, Raylib_cs.KeyboardKey.Down, now);
        
        UpdateKey(KeyboardKey.Kp7, Raylib_cs.KeyboardKey.Kp7, now);
        UpdateKey(KeyboardKey.Kp8, Raylib_cs.KeyboardKey.Kp8, now);
        UpdateKey(KeyboardKey.Kp9, Raylib_cs.KeyboardKey.Kp9, now);
        UpdateKey(KeyboardKey.Kp4, Raylib_cs.KeyboardKey.Kp4, now);
        UpdateKey(KeyboardKey.Kp5, Raylib_cs.KeyboardKey.Kp5, now);
        UpdateKey(KeyboardKey.Kp6, Raylib_cs.KeyboardKey.Kp6, now);
        UpdateKey(KeyboardKey.Kp1, Raylib_cs.KeyboardKey.Kp1, now);
        UpdateKey(KeyboardKey.Kp2, Raylib_cs.KeyboardKey.Kp2, now);
        UpdateKey(KeyboardKey.Kp3, Raylib_cs.KeyboardKey.Kp3, now);
        
        UpdateKey(KeyboardKey.One, Raylib_cs.KeyboardKey.One, now);
        UpdateKey(KeyboardKey.Two, Raylib_cs.KeyboardKey.Two, now);
        UpdateKey(KeyboardKey.Three, Raylib_cs.KeyboardKey.Three, now);
        UpdateKey(KeyboardKey.Four, Raylib_cs.KeyboardKey.Four, now);
        UpdateKey(KeyboardKey.Five, Raylib_cs.KeyboardKey.Five, now);
        UpdateKey(KeyboardKey.Six, Raylib_cs.KeyboardKey.Six, now);
        UpdateKey(KeyboardKey.Seven, Raylib_cs.KeyboardKey.Seven, now);
        UpdateKey(KeyboardKey.Eight, Raylib_cs.KeyboardKey.Eight, now);
        UpdateKey(KeyboardKey.Nine, Raylib_cs.KeyboardKey.Nine, now);
        UpdateKey(KeyboardKey.Zero, Raylib_cs.KeyboardKey.Zero, now);
        
        UpdateKey(KeyboardKey.A, Raylib_cs.KeyboardKey.A, now);
        UpdateKey(KeyboardKey.B, Raylib_cs.KeyboardKey.B, now);
        UpdateKey(KeyboardKey.C, Raylib_cs.KeyboardKey.C, now);
        UpdateKey(KeyboardKey.D, Raylib_cs.KeyboardKey.D, now);
        UpdateKey(KeyboardKey.E, Raylib_cs.KeyboardKey.E, now);
        UpdateKey(KeyboardKey.F, Raylib_cs.KeyboardKey.F, now);
        UpdateKey(KeyboardKey.G, Raylib_cs.KeyboardKey.G, now);
        UpdateKey(KeyboardKey.H, Raylib_cs.KeyboardKey.H, now);
        UpdateKey(KeyboardKey.I, Raylib_cs.KeyboardKey.I, now);
        UpdateKey(KeyboardKey.J, Raylib_cs.KeyboardKey.J, now);
        UpdateKey(KeyboardKey.K, Raylib_cs.KeyboardKey.K, now);
        UpdateKey(KeyboardKey.L, Raylib_cs.KeyboardKey.L, now);
        UpdateKey(KeyboardKey.M, Raylib_cs.KeyboardKey.M, now);
        UpdateKey(KeyboardKey.N, Raylib_cs.KeyboardKey.N, now);
        UpdateKey(KeyboardKey.O, Raylib_cs.KeyboardKey.O, now);
        UpdateKey(KeyboardKey.P, Raylib_cs.KeyboardKey.P, now);
        UpdateKey(KeyboardKey.Q, Raylib_cs.KeyboardKey.Q, now);
        UpdateKey(KeyboardKey.R, Raylib_cs.KeyboardKey.R, now);
        UpdateKey(KeyboardKey.S, Raylib_cs.KeyboardKey.S, now);
        UpdateKey(KeyboardKey.T, Raylib_cs.KeyboardKey.T, now);
        UpdateKey(KeyboardKey.U, Raylib_cs.KeyboardKey.U, now);
        UpdateKey(KeyboardKey.V, Raylib_cs.KeyboardKey.V, now);
        UpdateKey(KeyboardKey.W, Raylib_cs.KeyboardKey.W, now);
        UpdateKey(KeyboardKey.X, Raylib_cs.KeyboardKey.X, now);
        UpdateKey(KeyboardKey.Y, Raylib_cs.KeyboardKey.Y, now);
        UpdateKey(KeyboardKey.Z, Raylib_cs.KeyboardKey.Z, now);
        
        UpdateKey(KeyboardKey.Escape, Raylib_cs.KeyboardKey.Escape, now);
        UpdateKey(KeyboardKey.Space, Raylib_cs.KeyboardKey.Space, now);
        UpdateKey(KeyboardKey.Enter, Raylib_cs.KeyboardKey.Enter, now);

        // Cheat keys
        UpdateKey(KeyboardKey.F5, Raylib_cs.KeyboardKey.F5, now);

        // Stairs keys
        UpdateKey(KeyboardKey.Period, Raylib_cs.KeyboardKey.Period, now);
        UpdateKey(KeyboardKey.Comma, Raylib_cs.KeyboardKey.Comma, now);
        UpdateKey(KeyboardKey.LeftShift, Raylib_cs.KeyboardKey.LeftShift, now);
        UpdateKey(KeyboardKey.RightShift, Raylib_cs.KeyboardKey.RightShift, now);
        
        UpdateKey(KeyboardKey.LeftAlt, Raylib_cs.KeyboardKey.LeftAlt, now);
        UpdateKey(KeyboardKey.RightAlt, Raylib_cs.KeyboardKey.RightAlt, now);
    }

    private void UpdateKey(KeyboardKey key, Raylib_cs.KeyboardKey raylibKey, TimeSpan now)
    {
        KeyPulse[key] = false;
        bool isDown = IsKeyDown(raylibKey);
        bool pressed = IsKeyPressed(raylibKey);
        
        KeyHeld[key] = isDown;
        
        if (pressed)
        {
            KeyLastPressTime[key] = now;
            _keyPressTime[key] = now;
            _keyLastRepeatTime[key] = now;
            KeyPulse[key] = true;
            return;
        }
        
        if (!isDown)
            return;
        
        if (!_keyPressTime.TryGetValue(key, out var pressTime))
            return;

        var heldTime = now - pressTime;

        if (heldTime < InitialRepeatDelay)
            return;

        if (!_keyLastRepeatTime.TryGetValue(key, out var lastRepeat) || now - lastRepeat >= RepeatInterval)
        {
            _keyLastRepeatTime[key] = now;
            KeyPulse[key] = true;
        }
    }

}
