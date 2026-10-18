using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class CartPage : ContentPage
{
    public CartPage()
    {
        InitializeComponent();
        List.ItemsSource = AppState.Cart;
    }

    protected override void OnAppearing() { base.OnAppearing(); AppState.CartChanged += UpdateTotal; UpdateTotal(); }
    protected override void OnDisappearing() { base.OnDisappearing(); AppState.CartChanged -= UpdateTotal; }

    void UpdateTotal()
    {
        TotalLabel.Text = $"{AppState.CartTotal:N0} FCFA";
        Footer.IsVisible = AppState.Cart.Count > 0;
    }

    async void OnReserveAll(object? sender, EventArgs e)
    {
        string? lastId = null;
        var failed = new List<string>();

        foreach (var item in AppState.Cart.ToList())
        {
            var r = AppState.Reserve(item.Product, item.Quantity);
            if (r != null) lastId = r.Id; else failed.Add(item.Product.Name);
        }
        AppState.Cart.Clear();
        AppState.RaiseCartChanged();

        if (failed.Count > 0) await DisplayAlert("Stock insuffisant", "Non réservé : " + string.Join(", ", failed), "OK");
        if (lastId != null) await Shell.Current.GoToAsync($"confirmation?id={lastId}");
    }
}
