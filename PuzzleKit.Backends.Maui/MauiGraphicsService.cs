using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using PuzzleKit.Core;
using PuzzleKit.Graphics;
using PuzzleKit.Audio;
using PuzzleKit.Input;

using ICanvas = PuzzleKit.Graphics.ICanvas;
using Color = PuzzleKit.Graphics.Color;

namespace PuzzleKit.Backends.Maui;

public class MauiCanvas : ICanvas
{
    private readonly Microsoft.Maui.Graphics.ICanvas _canvas;

    public MauiCanvas(Microsoft.Maui.Graphics.ICanvas canvas)
    {
        _canvas = canvas;
    }

    public void DrawRectangle(float x, float y, float width, float height)
    {
        _canvas.DrawRectangle(x, y, width, height);
    }

    public void FillRectangle(float x, float y, float width, float height)
    {
        _canvas.FillRectangle(x, y, width, height);
    }

    public void DrawCircle(float x, float y, float radius)
    {
        _canvas.DrawCircle(x, y, radius);
    }
    
    public void FillCircle(float x, float y, float radius)
    {
        _canvas.FillCircle(x, y, radius);
    }

    public void DrawLine(float x1, float y1, float x2, float y2)
    {
        _canvas.DrawLine(x1, y1, x2, y2);
    }

    public void DrawText(string text, float x, float y)
    {
        _canvas.DrawString(text, x, y, HorizontalAlignment.Left);
    }

    public void SetStrokeColor(PuzzleKit.Graphics.Color color)
    {
        _canvas.StrokeColor = new Microsoft.Maui.Graphics.Color((float)color.R / 255f, 
            (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
    }

    public void SetFillColor(PuzzleKit.Graphics.Color color)
    {
        _canvas.FillColor = new Microsoft.Maui.Graphics.Color((float)color.R / 255f, 
            (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
    }

    public void SetStrokeWidth(float width)
    {
        _canvas.StrokeSize = width;
    }
    
    public void SetFont(string fontName, float fontSize)
    {
        _canvas.Font = new Microsoft.Maui.Graphics.Font(fontName);
        _canvas.FontSize = fontSize;
    }
}

public class MauiGraphicsService : IGraphicsService
{
    private readonly GraphicsView _graphicsView;
    private readonly MauiDrawable _drawable;
    private bool _isInitialized = false;

    public int ScreenWidth => (int)_graphicsView.Width;
    public int ScreenHeight => (int)_graphicsView.Height;

    public MauiGraphicsService(GraphicsView graphicsView)
    {
        _graphicsView = graphicsView;
        _drawable = new MauiDrawable(this);
        _graphicsView.Drawable = _drawable;
        
        // サイズ変更イベントをリッスン
        _graphicsView.SizeChanged += OnGraphicsViewSizeChanged;
    }

    private void OnGraphicsViewSizeChanged(object? sender, EventArgs e)
    {
        if (!_isInitialized && _graphicsView.Width > 0 && _graphicsView.Height > 0)
        {
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine($"GraphicsView size initialized: {_graphicsView.Width}x{_graphicsView.Height}");
            RequestRedraw();
        }
    }

    public void Initialize()
    {
        // すでにサイズが有効であれば初期化完了とする
        if (_graphicsView.Width > 0 && _graphicsView.Height > 0)
        {
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine($"GraphicsView already has valid size: {_graphicsView.Width}x{_graphicsView.Height}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Waiting for GraphicsView to get a valid size...");
        }
    }

    public ICanvas BeginDraw()
    {
        return _drawable.PuzzleKitCanvas!;
    }

    public void EndDraw()
    {
        // 何もしない（Invalidateはタイマーによって呼ばれる）
        //_graphicsView.Invalidate();
    }
    
    public void RequestRedraw()
    {
        // UIスレッド以外から来るケースも想定
        if (_graphicsView.Dispatcher.IsDispatchRequired)
        {
            _graphicsView.Dispatcher.Dispatch(() => _graphicsView.Invalidate());
        }
        else
        {
            _graphicsView.Invalidate();
        }
    }

    private class MauiDrawable : Microsoft.Maui.Graphics.IDrawable
    {
        private readonly MauiGraphicsService _service;
        public MauiCanvas? PuzzleKitCanvas { get; private set; }

        public MauiDrawable(MauiGraphicsService service)
        {
            _service = service;
        }
        
        public void Draw(Microsoft.Maui.Graphics.ICanvas canvas, RectF dirtyRect)
        {
            PuzzleKitCanvas = new MauiCanvas(canvas);
            PuzzleKit.Core.GameApplication.Instance.Draw(PuzzleKitCanvas);
        }
    }
}
