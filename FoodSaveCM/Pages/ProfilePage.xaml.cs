using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class ProfilePage : ContentPage
{
    bool _loading;

    public ProfilePage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _loading = true;
        NameLabel.Text = $"NOMS : {AppState.FullName.ToUpperInvariant()}";
        MealsLabel.Text = AppState.MealsSaved.ToString();
        Co2Label.Text = AppState.Co2Saved.ToString("0.#");
        MoneyLabel.Text = AppState.MoneySaved.ToString("N0");
        DarkSwitch.IsToggled = AppState.DarkMode;
        NotifSwitch.IsToggled = AppState.Notifications;
        _loading = false;
    }

    void OnDarkToggled(object? sender, ToggledEventArgs e)
    {
        if (_loading) return;
        AppState.DarkMode = e.Value;
        Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
    }

    async void OnNotifToggled(object? sender, ToggledEventArgs e)
    {
        if (_loading) return;
        AppState.Notifications = e.Value;
        await Ui.Show(e.Value ? "Alertes activées 🔔" : "Alertes désactivées");
    }

    async void OnReservations(object? sender, EventArgs e) => await Shell.Current.GoToAsync("reservations");
    async void OnPublications(object? sender, EventArgs e) => await Shell.Current.GoToAsync("publications");
    async void OnPublish(object? sender, EventArgs e) => await Shell.Current.GoToAsync("publish");

    async void OnLogout(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("Déconnexion", "Voulez-vous vraiment vous déconnecter ?", "Oui", "Non")) return;
        AppState.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
