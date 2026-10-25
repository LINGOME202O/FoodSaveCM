using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

[QueryProperty(nameof(ReservationId), "id")]
public partial class PaymentPage : ContentPage
{
    Reservation? _r;
    string _method = "Orange Money";

    public string ReservationId
    {
        set
        {
            _r = AppState.Reservations.FirstOrDefault(x => x.Id == value);
            BindingContext = _r;
        }
    }

    public PaymentPage()
    {
        InitializeComponent();
        Select("Orange Money");
    }

    void Select(string method)
    {
        _method = method;
        Mark(OrangeBtn, method == "Orange Money");
        Mark(MtnBtn, method == "MTN MoMo");
    }

    static void Mark(Button b, bool selected)
    {
        b.BackgroundColor = selected ? Color.FromArgb("#009B4D") : Color.FromArgb("#8E8E93");
        b.TextColor = Colors.White;
    }

    void OnOrange(object? s, EventArgs e) => Select("Orange Money");
    void OnMtn(object? s, EventArgs e) => Select("MTN MoMo");

    async void OnPay(object? sender, EventArgs e)
    {
        if (_r == null) return;
        ErrorLabel.IsVisible = false;
        PayBtn.IsEnabled = false;
        Busy.IsVisible = Busy.IsRunning = true;

        await Task.Delay(1800); // simulation de la validation par l'opérateur

        var (ok, message) = AppState.Pay(_r, _method, PhoneEntry.Text ?? "", PinEntry.Text ?? "");
        Busy.IsRunning = Busy.IsVisible = false;
        PayBtn.IsEnabled = true;

        if (!ok)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
            return;
        }

        await DisplayAlert("Paiement réussi ✅",
            $"{_r.Total:0} FCFA payés via {_method}.\nRéférence : {message}\nPrésentez votre QR code au commerçant pour retirer votre commande.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
