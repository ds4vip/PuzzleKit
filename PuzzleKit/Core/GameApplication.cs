using System;
using System.Timers;

namespace PuzzleKit.Core;

public class GameApplication
{
    private IGameState? _currentState;
    private bool _isRunning;
    private float _accumulatedTime = 0;
    private float _totalTime = 0;
    private DateTime _lastUpdate;

    /// <summary>論理フレームレート（単位: 秒）</summary>
    public float TargetElapsedTime { get; set; } = 1.0f / 30f;

    /// <summary>描画スキップ最大回数</summary>
    public int MaxFrameSkip { get; set; } = 5;

    /// <summary>外部で描画要求メソッドを提供する必要あり（プラットフォーム固有層でBindする）</summary>
    public Action? RequestRedraw { get; set; }

    public Graphics.IGraphicsService? Graphics { get; private set; }
    public Input.IInputService? Input { get; private set; }
    public Audio.IAudioService? Audio { get; private set; }

    private static GameApplication? _instance;
    public static GameApplication Instance => _instance ??= new GameApplication();

    private GameTime _gameTime = new();
    private System.Timers.Timer? _timer;

    /// <summary>
    /// PuzzleKit.Coreはプラットフォーム非依存。Platform層でIGraphicsService等を提供して初期化する
    /// </summary>
    public void Initialize(Graphics.IGraphicsService graphics, Input.IInputService input, Audio.IAudioService audio)
    {
        Graphics = graphics;
        Input = input;
        Audio = audio;
        Graphics.Initialize();
        Input.Initialize();
        Audio.Initialize();
    }

    /// <summary>
    /// Update/Drawループ開始
    /// </summary>
    public void Run(IGameState initialState)
    {
        if (_isRunning) return;
        _currentState = initialState;
        _currentState.Enter();
        _isRunning = true;
        StartGameLoop();
    }

    public void ChangeState(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    //public void Update(float deltaTime)
    //{
    //    var gameTime = new GameTime();
    //    gameTime.ElapsedTime = deltaTime;
    //    _currentState?.Update(gameTime);
    //}

    public void Update(GameTime gameTime)
    {
        _currentState?.Update(gameTime);
    }

    public void Draw(Graphics.ICanvas canvas)
    {
        _currentState?.Draw(canvas);
        Graphics?.RequestRedraw();
    }

    /// <summary>
    /// System.Timers.Timerを用いたクロスプラットフォームなゲームループ
    /// </summary>
    private void StartGameLoop()
    {
        _accumulatedTime = 0;
        _totalTime = 0;
        _lastUpdate = DateTime.Now;
        _gameTime.TotalTime = 0;
        _gameTime.ElapsedTime = 0;
        _gameTime.SkippedRenderFrames = 0;
        _gameTime.IsLagging = false;

        _timer = new System.Timers.Timer(TargetElapsedTime * 1000.0)
        {
            AutoReset = true,
            Enabled = true,
        };
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private int _skippedFrames = 0;

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_isRunning) return;

        
        var now = DateTime.Now;
        float realDelta = (float)(now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        // フレーム落ち等で異常値の場合は誤爆回避
        if (realDelta > 0.3f) realDelta = TargetElapsedTime;

        _accumulatedTime += realDelta;
        int numUpdates = 0;
        _gameTime.IsLagging = false;
        _gameTime.RealElapsedTime = realDelta;
        _gameTime.SkippedRenderFrames = 0;

        // 遅延分のUpdateを追いつくまで複数回呼ぶ
        while (_accumulatedTime >= TargetElapsedTime && numUpdates < MaxFrameSkip + 1)
        {
            _gameTime.ElapsedTime = TargetElapsedTime;
            _totalTime += TargetElapsedTime;
            _gameTime.TotalTime = _totalTime;
            _currentState?.Update(_gameTime);
            _accumulatedTime -= TargetElapsedTime;
            numUpdates++;
        }

        if (numUpdates > 1)
        {
            _gameTime.IsLagging = true;
            _gameTime.SkippedRenderFrames = Math.Min(numUpdates - 1, MaxFrameSkip);
        }

        // 遅延時、最大MaxFrameSkip回までは描画をスキップ
        if (_skippedFrames < _gameTime.SkippedRenderFrames)
        {
            _skippedFrames++;
            return;
        }
        _skippedFrames = 0;

        RequestRedraw?.Invoke();
    }
}
