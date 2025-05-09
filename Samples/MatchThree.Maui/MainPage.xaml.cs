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
        }
    }
    
    private void InitializeGame()
    {
        System.Diagnostics.Debug.WriteLine($"Initializing game with GraphicsView size: {GameGraphicsView.Width}x{GameGraphicsView.Height}");
        
        _graphicsService = new MauiGraphicsService(GameGraphicsView);
        var inputService = new MauiInputService(GameGraphicsView);
        var audioService = new MauiAudioService();

        var app = GameApplication.Instance;
        app.Initialize(_graphicsService, inputService, audioService);
        app.Run(new MatchThreeGameState());
    }

}