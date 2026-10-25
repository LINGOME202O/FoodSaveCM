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
                CancelBtn.IsVisible = _r.IsUpcoming;
                ReviewBtn.IsVisible = _r.Status == ReservationStatus.Past && !AppState.HasReview(_r.Id);
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

    async void OnPay(object? sender, EventArgs e)
    {
        if (_r != null) await Shell.Current.GoToAsync($"payment?id={_r.Id}");
    }

    async void OnReview(object? sender, EventArgs e)
    {
        if (_r == null) return;
        var choice = await DisplayActionSheet($"Noter {_r.Shop}", "Annuler", null,
            "⭐⭐⭐⭐⭐  5 - Excellent", "⭐⭐⭐⭐  4 - Très bien", "⭐⭐⭐  3 - Correct", "⭐⭐  2 - Décevant", "⭐  1 - Mauvais");
        if (choice == null || choice == "Annuler") return;
        var stars = int.Parse(choice.Split(' ', StringSplitOptions.RemoveEmptyEntries).First(x => x.All(char.IsDigit)));
        var comment = await DisplayPromptAsync("Votre avis", "Un commentaire (facultatif) :", "Envoyer", "Passer", "Pain frais, accueil sympa...");
        if (AppState.AddReview(_r, stars, comment ?? ""))
        {
            ReviewBtn.IsVisible = false;
            await Ui.Show("Merci pour votre avis ! ⭐");
        }
    }

    async void OnMyReservations(object? sender, EventArgs e) => await Shell.Current.GoToAsync("reservations");

    async void OnCancel(object? sender, EventArgs e)
    {
        if (_r == null) return;
        if (await DisplayAlert("Annuler la réservation", $"Annuler votre réservation chez {_r.Shop} ?", "Oui, annuler", "Non"))
        {
            AppState.Cancel(_r);
            await Ui.Show("Réservation annulée");
            await Shell.Current.GoToAsync("reservations");
        }
    }
}
