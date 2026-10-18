using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class ShopsPage : ContentPage
{
    public ShopsPage()
    {
        InitializeComponent();
        List.ItemsSource = AppState.Shops.OrderBy(s => s.Distance).ToList();
    }

    async void OnGo(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Shop s) return;
        try
        {
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(s.Lat, s.Lon,
                new MapLaunchOptions { Name = s.Name, NavigationMode = NavigationMode.Driving });
        }
        catch { await Ui.Show("Impossible d'ouvrir l'application de cartes"); }
    }
}
