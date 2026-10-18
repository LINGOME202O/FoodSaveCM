using FoodSaveCM.Services;

namespace FoodSaveCM;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppState.DarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}