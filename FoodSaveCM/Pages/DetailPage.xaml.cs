using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

[QueryProperty(nameof(ProductId), "id")]
public partial class DetailPage : ContentPage
{
    Product? _p;
    IDispatcherTimer? _timer;

    public string ProductId
    {
        set
        {
            _p = AppState.Products.FirstOrDefault(x => x.Id.ToString() == value);
            BindingContext = _p;
            if (_p != null)
            {
                Title = "Détail";
                ContentsBox.IsVisible = _p.Contents.Count > 0;
                SectionTitle.Text = _p.IsBasket ? "Contenu du panier" : "Description";
                ReserveBtn.Text = _p.IsBasket ? "Réserver ce panier" : "Réserver ce produit";
                UpdateCountdown();
                var (avg, count) = AppState.ShopRating(_p.Shop);
                RatingLabel.Text = count == 0 ? "Pas encore d'avis" : $"⭐ {avg:0.0}/5 ({count} avis) · {_p.Shop}";
            }
        }
    }

    public DetailPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += (_, _) => UpdateCountdown();
        _timer.Start();
    }

    protected override void OnDisappearing() { base.OnDisappearing(); _timer?.Stop(); }

    void UpdateCountdown()
    {
        if (_p == null) return;
        var end = DateTime.Today.AddHours(_p.PickupEndHour);
        var left = end - DateTime.Now;
        CountdownLabel.Text = left.TotalMinutes > 0
            ? $"⏳ Fin du retrait dans {(int)left.TotalHours}h {left.Minutes:00}min"
            : "⌛ Le créneau de retrait du jour est terminé";
    }

    async void OnReserve(object? sender, EventArgs e)
    {
        if (_p == null) return;
        var r = AppState.Reserve(_p);
        if (r == null) { await Ui.Show("Désolé, ce produit est épuisé"); return; }
        await Shell.Current.GoToAsync($"confirmation?id={r.Id}");
    }

    async void OnAddToCart(object? sender, EventArgs e)
    {
        if (_p == null) return;
        await Ui.Show(AppState.AddToCart(_p) ? "Ajouté au panier 🛒" : "Produit épuisé");
    }

    void OnFav(object? sender, TappedEventArgs e) { if (_p != null) AppState.ToggleFavorite(_p); }

    async void OnRoute(object? sender, EventArgs e)
    {
        if (_p == null) return;
        try
        {
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(_p.Lat, _p.Lon,
                new MapLaunchOptions { Name = _p.Shop, NavigationMode = NavigationMode.Driving });
        }
        catch { await Ui.Show("Impossible d'ouvrir l'application de cartes"); }
    }

    async void OnShare(object? sender, EventArgs e)
    {
        if (_p == null) return;
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "FoodSaveCM",
            Text = $"🍀 {_p.Name} à {_p.PriceText} chez {_p.Shop} ({_p.Address}) – retrait {_p.PickupText}. Sauvons la nourriture avec FoodSaveCM !"
        });
    }
}
