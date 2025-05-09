using PuzzleKit.Graphics;
using PuzzleKit.Input;
using PuzzleKit.Core;

namespace PuzzleKit.Samples.MatchThree;

public enum GemType
{
    Red, Blue, Green, Yellow, Purple, Orange
}

public class Gem : IDrawable
{
    public GemType Type { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float Size { get; set; }
    public bool IsFalling { get; set; }
    public bool IsMatched { get; set; }

    public Gem(GemType type, int row, int col, float size)
    {
        Type = type;
        Row = row;
        Column = col;
        Size = size;
        X = TargetX = 0;
        Y = TargetY = 0;
    }

    public void Draw(ICanvas canvas)
    {
        Color color = Type switch
        {
            GemType.Red => new(255, 0, 0),
            GemType.Blue => new(0, 0, 255),
            GemType.Green => new(0, 255, 0),
            GemType.Yellow => new(255, 255, 0),
            GemType.Purple => new(128, 0, 128),
            _ => new(255, 128, 0)
        };
        canvas.SetFillColor(color);
        float half = Size / 2;
        canvas.FillRectangle(X - half, Y - half, Size, Size);

        if (IsMatched)
        {
            canvas.SetStrokeColor(new Color(255, 255, 255));
            canvas.SetStrokeWidth(3);
            canvas.DrawRectangle(X - half - 2, Y - half - 2, Size + 4, Size + 4);
        }
    }

    public void UpdatePosition(float deltaTime)
    {
        const float speed = 5.0f;
        X += (TargetX - X) * Math.Min(1, speed * deltaTime);
        Y += (TargetY - Y) * Math.Min(1, speed * deltaTime);
        if (Math.Abs(X - TargetX) < 0.5f) X = TargetX;
        if (Math.Abs(Y - TargetY) < 0.5f) Y = TargetY;
    }
}

public class GameBoard : IDrawable, IInputListener
{
    private const int GRID = 8;
    private readonly float _x, _y, _cell;
    private readonly Gem[,] _gems = new Gem[GRID, GRID];
    private readonly Random _rnd = new();
    private int _score = 0;
    private bool _isAnimating = false;
    private bool _isSwapping = false;
    private Gem? _selectedGem;
    private float _touchStartX, _touchStartY;
    private int _touchStartRow, _touchStartCol;
    private const float SwipeThreshold = 20f;

    public event Action<int>? ScoreChanged;

    public GameBoard(float x, float y, float width)
    {
        _x = x;
        _y = y;
        _cell = width / GRID;
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
                CreateGemAt(r, c, r, c, false);
        UpdateGemPositions();
        ResolveInitialMatches();
    }

    private void CreateGemAt(int row, int col, int logicalRow, int logicalCol, bool fromTop)
    {
        var avail = Enum.GetValues<GemType>().ToList();
        if (col >= 2 && _gems[row, col - 1]?.Type == _gems[row, col - 2]?.Type)
            avail.Remove(_gems[row, col - 1].Type);
        if (row >= 2 && _gems[row - 1, col]?.Type == _gems[row - 2, col]?.Type)
            avail.Remove(_gems[row - 1, col].Type);
        var type = avail[_rnd.Next(avail.Count)];
        var gem = new Gem(type, logicalRow, logicalCol, _cell * 0.8f);
        float gx = _x + col * _cell + _cell / 2;
        float gy = _y + row * _cell + _cell / 2;
        gem.TargetX = gx;
        gem.TargetY = gy;
        if (fromTop)
        {
            gem.X = gx;
            gem.Y = _y - _cell; // 上から落下
        }
        else
        {
            gem.X = gx;
            gem.Y = gy;
        }
        _gems[row, col] = gem;
    }

    private void ResolveInitialMatches()
    {
        bool found;
        do
        {
            found = false;
            for (int r = 0; r < GRID; r++)
                for (int c = 0; c < GRID; c++)
                {
                    if (c >= 2 && _gems[r, c].Type == _gems[r, c - 1].Type && _gems[r, c].Type == _gems[r, c - 2].Type)
                    { CreateGemAt(r, c, r, c, false); found = true; }
                    if (r >= 2 && _gems[r, c].Type == _gems[r - 1, c].Type && _gems[r, c].Type == _gems[r - 2, c].Type)
                    { CreateGemAt(r, c, r, c, false); found = true; }
                }
        } while (found);
    }

    public void Update(GameTime gameTime)
    {
        var deltaTime = gameTime.ElapsedTime;
        bool animating = false;
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
            {
                _gems[r, c].UpdatePosition(deltaTime);
                if (Math.Abs(_gems[r, c].X - _gems[r, c].TargetX) > 0.5f ||
                    Math.Abs(_gems[r, c].Y - _gems[r, c].TargetY) > 0.5f)
                    animating = true;
            }
        _isAnimating = animating;

        // 落下やスワップアニメーション終了後にマッチ処理
        if (!_isAnimating && _isSwapping)
        {
            _isSwapping = false;
            if (!CheckAndRemoveMatches())
                UndoSwap(); // スワップでマッチしなかったら元に戻す
        }
        else if (!_isAnimating)
        {
            if (CheckAndRemoveMatches())
                return;
        }
    }

    public void Draw(ICanvas canvas)
    {
        canvas.SetFillColor(new Color(50, 50, 80));
        canvas.FillRectangle(_x, _y, _cell * GRID, _cell * GRID);
        canvas.SetStrokeColor(new Color(70, 70, 100));
        canvas.SetStrokeWidth(1);
        for (int i = 0; i <= GRID; i++)
        {
            float pos = i * _cell;
            canvas.DrawLine(_x, _y + pos, _x + _cell * GRID, _y + pos);
            canvas.DrawLine(_x + pos, _y, _x + pos, _y + _cell * GRID);
        }
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
                _gems[r, c].Draw(canvas);
    }

    public void OnInput(InputEvent inputEvent)
    {
        if (_isAnimating) return;

        if (inputEvent.Type == InputType.Touch)
        {
            int row, col;
            if (GetGemAt(inputEvent.X, inputEvent.Y, out row, out col))
            {
                _selectedGem = _gems[row, col];
                _touchStartX = inputEvent.X;
                _touchStartY = inputEvent.Y;
                _touchStartRow = row;
                _touchStartCol = col;
            }
        }
        else if (inputEvent.Type == InputType.Release && _selectedGem != null)
        {
            float dx = inputEvent.X - _touchStartX;
            float dy = inputEvent.Y - _touchStartY;
            int dirRow = 0, dirCol = 0;
            if (Math.Abs(dx) > Math.Abs(dy) && Math.Abs(dx) > SwipeThreshold)
            {
                dirCol = dx > 0 ? 1 : -1;
            }
            else if (Math.Abs(dy) > SwipeThreshold)
            {
                dirRow = dy > 0 ? 1 : -1;
            }
            int toRow = _touchStartRow + dirRow;
            int toCol = _touchStartCol + dirCol;
            if (dirRow != 0 || dirCol != 0)
            {
                if (toRow >= 0 && toRow < GRID && toCol >= 0 && toCol < GRID)
                {
                    SwapGems(_touchStartRow, _touchStartCol, toRow, toCol);
                }
            }
            _selectedGem = null;
        }
    }

    private bool GetGemAt(float x, float y, out int row, out int col)
    {
        row = (int)((y - _y) / _cell);
        col = (int)((x - _x) / _cell);
        if (row >= 0 && row < GRID && col >= 0 && col < GRID)
            return true;
        return false;
    }

    private void SwapGems(int rowA, int colA, int rowB, int colB)
    {
        var gemA = _gems[rowA, colA];
        var gemB = _gems[rowB, colB];
        _gems[rowA, colA] = gemB;
        _gems[rowB, colB] = gemA;
        (gemA.Row, gemA.Column, gemB.Row, gemB.Column) = (rowB, colB, rowA, colA);
        UpdateGemPositions();
        _isSwapping = true;
    }

    private void UndoSwap()
    {
        // 最後のスワップを元に戻す
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
            {
                var gem = _gems[r, c];
                if (gem.Row != r || gem.Column != c)
                {
                    var other = _gems[gem.Row, gem.Column];
                    _gems[r, c] = other;
                    _gems[gem.Row, gem.Column] = gem;
                    (gem.Row, gem.Column, other.Row, other.Column) = (r, c, gem.Row, gem.Column);
                    UpdateGemPositions();
                    return;
                }
            }
    }

    private void UpdateGemPositions()
    {
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
            {
                var g = _gems[r, c];
                g.TargetX = _x + c * _cell + _cell / 2;
                g.TargetY = _y + r * _cell + _cell / 2;
            }
    }

    private bool CheckAndRemoveMatches()
    {
        bool[,] matched = new bool[GRID, GRID];
        bool found = false;
        // 横
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID - 2; c++)
            {
                var t = _gems[r, c].Type;
                if (t == _gems[r, c + 1].Type && t == _gems[r, c + 2].Type)
                {
                    matched[r, c] = matched[r, c + 1] = matched[r, c + 2] = true;
                    found = true;
                }
            }
        // 縦
        for (int c = 0; c < GRID; c++)
            for (int r = 0; r < GRID - 2; r++)
            {
                var t = _gems[r, c].Type;
                if (t == _gems[r + 1, c].Type && t == _gems[r + 2, c].Type)
                {
                    matched[r, c] = matched[r + 1, c] = matched[r + 2, c] = true;
                    found = true;
                }
            }
        if (!found) return false;

        // スコア加算
        int matchedCount = 0;
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
                if (matched[r, c]) matchedCount++;
        _score += matchedCount * 100;
        ScoreChanged?.Invoke(_score);

        // マッチしたジェムを消去
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
                if (matched[r, c])
                    _gems[r, c].IsMatched = true;

        // 落下処理
        AnimateFall(matched);
        return true;
    }

    private void AnimateFall(bool[,] matched)
    {
        for (int c = 0; c < GRID; c++)
        {
            int writeRow = GRID - 1;
            for (int readRow = GRID - 1; readRow >= 0; readRow--)
            {
                if (!matched[readRow, c])
                {
                    if (writeRow != readRow)
                    {
                        // 上のジェムを下に移動
                        var g = _gems[readRow, c];
                        _gems[writeRow, c] = g;
                        g.Row = writeRow;
                        g.Column = c;
                    }
                    writeRow--;
                }
            }
            // 新規ジェムを上から生成
            for (; writeRow >= 0; writeRow--)
            {
                CreateGemAt(writeRow, c, writeRow, c, true);
            }
        }
        UpdateGemPositions();
        // 全ジェムのIsMatchedフラグをリセット
        for (int r = 0; r < GRID; r++)
            for (int c = 0; c < GRID; c++)
                _gems[r, c].IsMatched = false;
        _isAnimating = true;
    }
}

public class MatchThreeGameState : IGameState
{
    private GameBoard? _gameBoard;
    private int _score = 0;

    public void Enter()
    {
        var g = GameApplication.Instance.Graphics!;
        int w = g.ScreenWidth, h = g.ScreenHeight;
        float boardSize = Math.Min(w * 0.9f, h * 0.7f);
        float boardX = (w - boardSize) / 2f;
        float boardY = h * 0.15f;
        _gameBoard = new GameBoard(boardX, boardY, boardSize);
        _gameBoard.ScoreChanged += OnScoreChanged;
        GameApplication.Instance.Input!.AddListener(_gameBoard);
    }

    public void Exit()
    {
        if (_gameBoard != null)
        {
            _gameBoard.ScoreChanged -= OnScoreChanged;
            GameApplication.Instance.Input!.RemoveListener(_gameBoard);
        }
    }

    public void Update(GameTime gameTime) => _gameBoard?.Update(gameTime);

    public void Draw(ICanvas canvas)
    {
        var g = GameApplication.Instance.Graphics!;
        int w = g.ScreenWidth, h = g.ScreenHeight;
        canvas.SetFillColor(new Color(20, 20, 40));
        canvas.FillRectangle(0, 0, w, h);
        canvas.SetFillColor(new Color(255, 255, 255));
        canvas.SetFont("Arial", 32);
        string title = "3マッチパズル";
        canvas.DrawText(title, (w - title.Length * 18) / 2, h * 0.08f);
        canvas.SetFillColor(new Color(255, 220, 120));
        canvas.SetFont("Arial", 24);
        canvas.DrawText($"スコア: {_score}", 20, h - 40);
        _gameBoard?.Draw(canvas);
    }

    private void OnScoreChanged(int newScore) => _score = newScore;
}
