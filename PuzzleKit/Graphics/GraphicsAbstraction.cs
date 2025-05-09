namespace PuzzleKit.Graphics;

public struct Color(byte r, byte g, byte b, byte a = 255)
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;
}

public interface ICanvas
{
    void DrawRectangle(float x, float y, float width, float height);
    void FillRectangle(float x, float y, float width, float height);
    void DrawCircle(float x, float y, float radius);
    void FillCircle(float x, float y, float radius);
    void DrawLine(float x1, float y1, float x2, float y2);
    void DrawText(string text, float x, float y);
    void SetStrokeColor(Color color);
    void SetFillColor(Color color);
    void SetStrokeWidth(float width);
    void SetFont(string fontName, float fontSize);
}

public interface IDrawable
{
    void Draw(ICanvas canvas);
}

public interface IGraphicsService
{
    void Initialize();
    void RequestRedraw();
    ICanvas BeginDraw();
    void EndDraw();

    int ScreenWidth { get; }
    int ScreenHeight { get; }
}