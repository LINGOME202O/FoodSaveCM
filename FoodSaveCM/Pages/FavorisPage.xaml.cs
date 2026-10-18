using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class FavorisPage : ContentPage
{
    public FavorisPage() => InitializeComponent();

    protected override void OnAppearing() { base.OnAppearing(); AppState.FavoritesChanged += Refresh; Refresh(); }
    protected override void OnDisappearing() { base.OnDisappearing(); AppState.FavoritesChanged -= Refresh; }

    void Refresh() => List.ItemsSource = AppState.Products.Where(p => p.IsFavorite).ToList();
}
