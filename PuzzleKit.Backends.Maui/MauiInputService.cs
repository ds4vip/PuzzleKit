using Microsoft.Maui.Controls;
using PuzzleKit.Input;

namespace PuzzleKit.Backends.Maui;

public class MauiInputService : IInputService
{
    private readonly GraphicsView _graphicsView;
    private readonly List<IInputListener> _listeners = [];
    private bool _isHandlingInput = false;
    
    public MauiInputService(GraphicsView graphicsView)
    {
        _graphicsView = graphicsView;
        
        // TouchEffectを使わず、直接GraphicsViewから入力を取得
        // タッチイベントのハンドラを設定
        _graphicsView.StartInteraction += OnStartInteraction;
        _graphicsView.MoveHoverInteraction += OnMoveInteraction;
        _graphicsView.EndInteraction += OnEndInteraction;
    }

    public void Initialize()
    {
        _isHandlingInput = true;
        System.Diagnostics.Debug.WriteLine("Input service initialized");
    }

    public void AddListener(IInputListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
            System.Diagnostics.Debug.WriteLine($"Added input listener: {listener.GetType().Name}");
        }
    }

    public void RemoveListener(IInputListener listener)
    {
        _listeners.Remove(listener);
    }

    public void ProcessEvent(InputEvent inputEvent)
    {
        if (!_isHandlingInput) return;

        _graphicsView.Dispatcher.Dispatch(() =>
        {
            foreach (var listener in _listeners)
            {
                listener.OnInput(inputEvent);
            }
        });
    }

    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        // 最初のタッチ位置を取得
        var touches = e.Touches;
        if (touches != null && touches.Length > 0)
        {
            float x = (float)touches[0].X;
            float y = (float)touches[0].Y;
            System.Diagnostics.Debug.WriteLine($"Touch down at: {x},{y}");
            
            var inputEvent = new InputEvent(InputType.Touch, x, y);
            ProcessEvent(inputEvent);
        }
    }

    private void OnMoveInteraction(object? sender, TouchEventArgs e)
    {
        var touches = e.Touches;
        if (touches != null && touches.Length > 0)
        {
            float x = (float)touches[0].X;
            float y = (float)touches[0].Y;
            
            var inputEvent = new InputEvent(InputType.Move, x, y);
            ProcessEvent(inputEvent);
        }
    }
    
    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        var touches = e.Touches;
        if (touches != null && touches.Length > 0)
        {
            float x = (float)touches[0].X;
            float y = (float)touches[0].Y;
            System.Diagnostics.Debug.WriteLine($"Touch up at: {x},{y}");
            
            var inputEvent = new InputEvent(InputType.Release, x, y);
            ProcessEvent(inputEvent);
        }
    }
}

