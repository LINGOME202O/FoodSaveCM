using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class ReservationsPage : ContentPage
{
    ReservationStatus _status = ReservationStatus.Upcoming;

    public ReservationsPage() => InitializeComponent();

    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }

    void Refresh()
    {
        List.ItemsSource = AppState.Reservations.Where(r => r.Status == _status).ToList();
        Style(TabUp, _status == ReservationStatus.Upcoming);
        Style(TabPast, _status == ReservationStatus.Past);
        Style(TabCancel, _status == ReservationStatus.Cancelled);
    }

    static void Style(Button b, bool selected)
    {
        b.BackgroundColor = selected ? Color.FromArgb("#009B4D") : Color.FromArgb("#8E8E93");
        b.TextColor = Colors.White;
        b.FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
    }

    void OnTabUp(object? s, EventArgs e) { _status = ReservationStatus.Upcoming; Refresh(); }
    void OnTabPast(object? s, EventArgs e) { _status = ReservationStatus.Past; Refresh(); }
    void OnTabCancel(object? s, EventArgs e) { _status = ReservationStatus.Cancelled; Refresh(); }

    async void OnShowQr(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is Reservation r)
            await Shell.Current.GoToAsync($"confirmation?id={r.Id}");
    }

    async void OnCancel(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Reservation r) return;
        if (await DisplayAlert("Annuler la réservation", $"Annuler votre réservation chez {r.Shop} ?", "Oui, annuler", "Non"))
        {
            AppState.Cancel(r);
            Refresh();
            await Ui.Show("Réservation annulée");
        }
    }
}
