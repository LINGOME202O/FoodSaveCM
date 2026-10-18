using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

[QueryProperty(nameof(ReservationId), "id")]
public partial class ConfirmationPage : ContentPage
{
    Reservation? _r;

    public string ReservationId
    {
        set
        {
            _r = AppState.Reservations.FirstOrDefault(x => x.Id == value);
            BindingContext = _r;
            if (_r != null)
            {
                try { QrImage.Source = QrHelper.Generate($"FOODSAVECM|{_r.Id}|{_r.Code}|{_r.Shop}"); }
                catch { /* QR non généré */ }
            }
        }
    }

    public ConfirmationPage() => InitializeComponent();

    async void OnRoute(object? sender, EventArgs e)
    {
        if (_r == null) return;
        try
        {
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(_r.Lat, _r.Lon,
                new MapLaunchOptions { Name = _r.Shop, NavigationMode = NavigationMode.Driving });
        }
        catch { await Ui.Show("Impossible d'ouvrir l'application de cartes"); }
    }

    async void OnMyReservations(object? sender, EventArgs e) => await Shell.Current.GoToAsync("reservations");
}
