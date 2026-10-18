using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Controls;

public partial class ProductCard : ContentView
{
    public ProductCard() => InitializeComponent();

    Product? P => BindingContext as Product;

    async void OnOpen(object? sender, TappedEventArgs e)
    {
        if (P == null) return;
        await Shell.Current.GoToAsync($"detail?id={P.Id}");
    }

    async void OnAdd(object? sender, EventArgs e)
    {
        if (P == null) return;
        if (AppState.AddToCart(P)) await Ui.Show($"{P.Name} ajouté au panier 🛒");
        else await Ui.Show("Ce produit est épuisé");
    }

    void OnFav(object? sender, TappedEventArgs e)
    {
        if (P != null) AppState.ToggleFavorite(P);
    }
}
