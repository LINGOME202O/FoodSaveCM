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
        ApplyTab(TabUp, UnderUp, _status == ReservationStatus.Upcoming);
        ApplyTab(TabPast, UnderPast, _status == ReservationStatus.Past);
        ApplyTab(TabCancel, UnderCancel, _status == ReservationStatus.Cancelled);
    }

    // Onglet actif : fond transparent, texte vert, trait vert dessous.
    // Onglets inactifs : fond gris, texte noir (comme la maquette).
    static void ApplyTab(Button b, BoxView underline, bool selected)
    {
        b.BackgroundColor = selected ? Colors.Transparent : Color.FromArgb("#8C8C90");
        b.TextColor = selected ? Color.FromArgb("#34C759") : Colors.Black;
        underline.Color = selected ? Color.FromArgb("#34C759") : Colors.Transparent;
    }

    void OnTabUp(object? s, EventArgs e) { _status = ReservationStatus.Upcoming; Refresh(); }
    void OnTabPast(object? s, EventArgs e) { _status = ReservationStatus.Past; Refresh(); }
    void OnTabCancel(object? s, EventArgs e) { _status = ReservationStatus.Cancelled; Refresh(); }

    async void OnShowQr(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Reservation r)
            await Shell.Current.GoToAsync($"confirmation?id={r.Id}");
    }
}
