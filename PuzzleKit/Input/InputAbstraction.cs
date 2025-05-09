namespace PuzzleKit.Input;

public enum InputType
{
    Touch,
    Release,
    Move,
    Key
}

public class InputEvent
{
    public InputType Type { get; }
    public float X { get; }
    public float Y { get; }
    public int PointerId { get; }
    public string? KeyCode { get; }

    public InputEvent(InputType type, float x, float y, int pointerId = 0, string? keyCode = null)
    {
        Type = type;
        X = x;
        Y = y;
        PointerId = pointerId;
        KeyCode = keyCode;
    }
}

public interface IInputListener
{
    void OnInput(InputEvent inputEvent);
}

public interface IInputService
{
    void Initialize();
    void AddListener(IInputListener listener);
    void RemoveListener(IInputListener listener);
    void ProcessEvent(InputEvent inputEvent);
}