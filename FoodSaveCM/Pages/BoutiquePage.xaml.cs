using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class BoutiquePage : ContentPage
{
    int _sort = 3; // meilleures promos par défaut

    public BoutiquePage() => InitializeComponent();

    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }

    void Refresh()
    {
        var items = AppState.Sort(AppState.Products.Where(p => p.IsBasket && p.Stock > 0), _sort).ToList();
        List.ItemsSource = items;
        CountLabel.Text = $"{items.Count} panier(s) disponible(s) près de vous · tri : {AppState.SortLabels[_sort]}";
    }

    async void OnSort(object? sender, EventArgs e)
    {
        var choice = await DisplayActionSheet("Trier par", "Annuler", null, AppState.SortLabels);
        var idx = Array.IndexOf(AppState.SortLabels, choice);
        if (idx >= 0) { _sort = idx; Refresh(); }
    }

    async void OnShops(object? sender, EventArgs e) => await Shell.Current.GoToAsync("shops");
}
