using PuzzleKit.Backends.Maui;
using PuzzleKit.Core;
using PuzzleKit.Samples.MatchThree;

namespace MatchThree.Maui;

public partial class MainPage : ContentPage
{
    private DateTime _lastUpdate = DateTime.Now;
    private bool _isInitialized = false;
    private MauiGraphicsService? _graphicsService;
    
    public MainPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeGameIfReady();
    }
    
    private void OnPageLoaded(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("MainPage loaded");
        InitializeGameIfReady();
    }
    
    private void InitializeGameIfReady()
    {
        if (_isInitialized) return;
        
        // GraphicsViewがレイアウトされていることを確認
        if (GameGraphicsView.Width <= 0 || GameGraphicsView.Height <= 0)
        {
            System.Diagnostics.Debug.WriteLine("GraphicsView not yet sized, waiting for layout");
            GameGraphicsView.SizeChanged += OnGraphicsViewSizeChanged;
            return;
        }
        
        _isInitialized = true;
        InitializeGame();
        StartGameLoop();
    }
    
    private void OnGraphicsViewSizeChanged(object? sender, EventArgs e)
    {
        if (GameGraphicsView.Width <= 0 || GameGraphicsView.Height <= 0) return;
        
        System.Diagnostics.Debug.WriteLine($"GraphicsView sized: {GameGraphicsView.Width}x{GameGraphicsView.Height}");
        GameGraphicsView.SizeChanged -= OnGraphicsViewSizeChanged;
        
        if (!_isInitialized)
        {
            _isInitialized = true;
            InitializeGame();
            StartGameLoop();
        }
    }
    
    private void InitializeGame()
    {
        System.Diagnostics.Debug.WriteLine($"Initializing game with GraphicsView size: {GameGraphicsView.Width}x{GameGraphicsView.Height}");
        
        _graphicsService = new MauiGraphicsService(GameGraphicsView);
        var inputService = new MauiInputService(GameGraphicsView);
        var audioService = new MauiAudioService();
        
        GameApplication.Instance.Initialize(_graphicsService, inputService, audioService);
        GameApplication.Instance.Run(new MatchThreeGameState());
    }

    private void StartGameLoop()
    {
        System.Diagnostics.Debug.WriteLine("Starting game loop");
        Device.StartTimer(TimeSpan.FromMilliseconds(16), OnFrameUpdate);
    }

    private bool OnFrameUpdate()
    {
        if (!_isInitialized) return true;
        
        var now = DateTime.Now;
        var delta = (float)(now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;
        
        // 極端なデルタ時間を制限
        if (delta > 0.1f) delta = 0.016f;
        
        GameApplication.Instance.Update(delta);
        _graphicsService?.RequestRedraw();
        
        return true; // ループを継続
    }
}