namespace MatchThree.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }
    
    protected override void OnStart()
    {
        base.OnStart();
    }
}