using Erg.Core.Abstractions;
using SFML.System;
using SFML.Window;

namespace Erg.Platforms.Sfml;

public class SfmlInput : IInput
{
    public Dictionary<KeyboardKey, TimeSpan> KeyLastPressTime { get; } = new();
    public Dictionary<KeyboardKey, bool> KeyPulse { get; } = new();
    public Dictionary<KeyboardKey, bool> KeyHeld { get; } = new();

    private readonly Dictionary<KeyboardKey, TimeSpan> _keyPressTime = new();
    private readonly Dictionary<KeyboardKey, TimeSpan> _keyLastRepeatTime = new();
    private readonly Dictionary<KeyboardKey, bool> _previousKeyState = new();
    private readonly Clock _clock = new();

    private static readonly TimeSpan InitialRepeatDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan RepeatInterval = TimeSpan.FromMilliseconds(100);

    public void Update()
    {
        var now = TimeSpan.FromSeconds(_clock.ElapsedTime.AsSeconds());

        UpdateKey(KeyboardKey.Insert, Keyboard.Key.Insert, now);
        UpdateKey(KeyboardKey.Delete, Keyboard.Key.Delete, now);
        UpdateKey(KeyboardKey.Home, Keyboard.Key.Home, now);
        UpdateKey(KeyboardKey.End, Keyboard.Key.End, now);
        UpdateKey(KeyboardKey.PageUp, Keyboard.Key.PageUp, now);
        UpdateKey(KeyboardKey.PageDown, Keyboard.Key.PageDown, now);
        
        UpdateKey(KeyboardKey.Left, Keyboard.Key.Left, now);
        UpdateKey(KeyboardKey.Right, Keyboard.Key.Right, now);
        UpdateKey(KeyboardKey.Up, Keyboard.Key.Up, now);
        UpdateKey(KeyboardKey.Down, Keyboard.Key.Down, now);

        UpdateKey(KeyboardKey.Kp7, Keyboard.Key.Numpad7, now);
        UpdateKey(KeyboardKey.Kp8, Keyboard.Key.Numpad8, now);
        UpdateKey(KeyboardKey.Kp9, Keyboard.Key.Numpad9, now);
        UpdateKey(KeyboardKey.Kp4, Keyboard.Key.Numpad4, now);
        UpdateKey(KeyboardKey.Kp5, Keyboard.Key.Numpad5, now);
        UpdateKey(KeyboardKey.Kp6, Keyboard.Key.Numpad6, now);
        UpdateKey(KeyboardKey.Kp1, Keyboard.Key.Numpad1, now);
        UpdateKey(KeyboardKey.Kp2, Keyboard.Key.Numpad2, now);
        UpdateKey(KeyboardKey.Kp3, Keyboard.Key.Numpad3, now);

        UpdateKey(KeyboardKey.Two, Keyboard.Key.Num2, now);
        UpdateKey(KeyboardKey.Three, Keyboard.Key.Num3, now);

        UpdateKey(KeyboardKey.A, Keyboard.Key.A, now);
        UpdateKey(KeyboardKey.B, Keyboard.Key.B, now);
        UpdateKey(KeyboardKey.C, Keyboard.Key.C, now);
        UpdateKey(KeyboardKey.D, Keyboard.Key.D, now);
        UpdateKey(KeyboardKey.E, Keyboard.Key.E, now);
        UpdateKey(KeyboardKey.F, Keyboard.Key.F, now);
        UpdateKey(KeyboardKey.G, Keyboard.Key.G, now);
        UpdateKey(KeyboardKey.H, Keyboard.Key.H, now);
        UpdateKey(KeyboardKey.I, Keyboard.Key.I, now);
        UpdateKey(KeyboardKey.J, Keyboard.Key.J, now);
        UpdateKey(KeyboardKey.K, Keyboard.Key.K, now);
        UpdateKey(KeyboardKey.L, Keyboard.Key.L, now);
        UpdateKey(KeyboardKey.M, Keyboard.Key.M, now);
        UpdateKey(KeyboardKey.N, Keyboard.Key.N, now);
        UpdateKey(KeyboardKey.O, Keyboard.Key.O, now);
        UpdateKey(KeyboardKey.P, Keyboard.Key.P, now);
        UpdateKey(KeyboardKey.Q, Keyboard.Key.Q, now);
        UpdateKey(KeyboardKey.R, Keyboard.Key.R, now);
        UpdateKey(KeyboardKey.S, Keyboard.Key.S, now);
        UpdateKey(KeyboardKey.T, Keyboard.Key.T, now);
        UpdateKey(KeyboardKey.U, Keyboard.Key.U, now);
        UpdateKey(KeyboardKey.V, Keyboard.Key.V, now);
        UpdateKey(KeyboardKey.W, Keyboard.Key.W, now);
        UpdateKey(KeyboardKey.X, Keyboard.Key.X, now);
        UpdateKey(KeyboardKey.Y, Keyboard.Key.Y, now);
        UpdateKey(KeyboardKey.Z, Keyboard.Key.Z, now);

        UpdateKey(KeyboardKey.Escape, Keyboard.Key.Escape, now);
        UpdateKey(KeyboardKey.Space, Keyboard.Key.Space, now);
        UpdateKey(KeyboardKey.Enter, Keyboard.Key.Enter, now);

        // Cheat keys
        UpdateKey(KeyboardKey.F5, Keyboard.Key.F5, now);

        // Stairs keys
        UpdateKey(KeyboardKey.Period, Keyboard.Key.Period, now);
        UpdateKey(KeyboardKey.Comma, Keyboard.Key.Comma, now);
        UpdateKey(KeyboardKey.LeftShift, Keyboard.Key.LShift, now);
        UpdateKey(KeyboardKey.RightShift, Keyboard.Key.RShift, now);

        UpdateKey(KeyboardKey.LeftAlt, Keyboard.Key.LAlt, now);
        UpdateKey(KeyboardKey.RightAlt, Keyboard.Key.RAlt, now);
    }

    private void UpdateKey(KeyboardKey key, Keyboard.Key sfmlKey, TimeSpan now)
    {
        KeyPulse[key] = false;
        bool isDown = Keyboard.IsKeyPressed(sfmlKey);
        bool wasDown = _previousKeyState.GetValueOrDefault(key, false);
        bool justPressed = isDown && !wasDown;

        _previousKeyState[key] = isDown;
        KeyHeld[key] = isDown;

        if (justPressed)
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
