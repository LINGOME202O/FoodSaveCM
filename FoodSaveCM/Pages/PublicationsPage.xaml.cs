using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class PublicationsPage : ContentPage
{
    public PublicationsPage() => InitializeComponent();

    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }

    void Refresh() => List.ItemsSource = AppState.Products.Where(p => p.IsMine).ToList();

    void OnDelete(object? sender, EventArgs e)
    {
        if ((sender as SwipeItem)?.BindingContext is Product p)
        {
            AppState.Products.Remove(p);
            Refresh();
        }
    }

    async void OnPublish(object? sender, EventArgs e) => await Shell.Current.GoToAsync("publish");
}
