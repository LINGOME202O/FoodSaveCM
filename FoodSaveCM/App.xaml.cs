using FoodSaveCM.Services;

namespace FoodSaveCM;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppState.DarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    // Compatible .NET 9 / .NET 10 (MAUI 9, 10.x)
    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(new AppShell());
}
