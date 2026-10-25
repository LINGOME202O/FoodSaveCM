using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class DashboardPage : ContentPage
{
    public DashboardPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var s = AppState.GetMerchantStats();
        PublishedLabel.Text = s.Published.ToString();
        ReservationsLabel.Text = s.Reservations.ToString();
        PendingLabel.Text = s.Pending.ToString();
        PickedLabel.Text = s.PickedUp.ToString();
        RevenueLabel.Text = $"💰 Chiffre d'affaires : {s.Revenue:0} FCFA";
        OnlineLabel.Text = $"💳 Dont payé en ligne : {s.PaidOnline:0} FCFA";
        RatingLabel.Text = s.RatingCount == 0 ? "⭐ Pas encore d'avis" : $"⭐ Note moyenne : {s.Rating:0.0}/5 ({s.RatingCount} avis)";
        TopLabel.Text = s.TopProducts.Count == 0 ? "Aucune réservation pour l'instant." : string.Join("\n", s.TopProducts.Select((t, i) => $"{i + 1}. {t}"));
    }

    async void OnValidate(object? sender, EventArgs e) => await Shell.Current.GoToAsync("validate");
}
