namespace Erg.Core.Abstractions;

public interface IInput
{
    public Dictionary<KeyboardKey, TimeSpan> KeyLastPressTime { get; }
    public Dictionary<KeyboardKey, bool> KeyPulse { get; }
    public Dictionary<KeyboardKey, bool> KeyHeld { get; }

    void Update();
}