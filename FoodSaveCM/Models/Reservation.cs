using System.Text.RegularExpressions;

namespace FoodSaveCM.Models;

public enum ReservationStatus { Upcoming, Past, Cancelled }

public class Reservation : ObservableObject
{
    public string Id { get; set; } = "";
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public string Shop { get; set; } = "";
    public string ShopSub { get; set; } = "";
    public string Headline { get; set; } = "";
    public string Title { get; set; } = "1x Panier anti-gaspi";
    public string DateText { get; set; } = "";
    public string Pickup { get; set; } = "";
    public string Address { get; set; } = "";
    public string Code { get; set; } = "";
    public int Count { get; set; } = 1;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public decimal Total { get; set; }
    public decimal Saved { get; set; }

    ReservationStatus _status = ReservationStatus.Upcoming;
    public ReservationStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Raise(); Raise(nameof(IsUpcoming)); Raise(nameof(CanPay)); Raise(nameof(Chip)); Raise(nameof(ChipBg)); Raise(nameof(ChipFg));
        }
    }

    public string PaymentMethod { get; set; } = "";
    public string PaymentRef { get; set; } = "";

    bool _isPaid;
    public bool IsPaid
    {
        get => _isPaid;
        set { _isPaid = value; Raise(); Raise(nameof(PaymentText)); Raise(nameof(CanPay)); }
    }

    public string PaymentText => IsPaid
        ? $"💳 Payé via {PaymentMethod} (réf. {PaymentRef})"
        : $"💰 À payer sur place : {Total:0} FCFA";
    public bool CanPay => !IsPaid && IsUpcoming;

    public bool IsUpcoming => Status == ReservationStatus.Upcoming;
    public string DisplayShop => string.IsNullOrEmpty(ShopSub) ? Shop : $"{Shop} - {ShopSub}";
    public string Icon => Shop.Contains("Boulangerie") ? "🥖" : Shop.Contains("Marché") ? "🧺" : "🛒";
    public string IconFile => Shop.Contains("Boulangerie") ? "ic_store.png" : "ic_cart.png";
    public string Summary => $"{DateText} . {Title}\n{Count} article{(Count > 1 ? "s" : "")}";
    public string PickupShort => Regex.Replace(Pickup.Replace("Aujourd'hui ", "").Replace("Hier ", ""), @"\s*-\s*", " - ");

    public string Chip => Status switch
    {
        ReservationStatus.Upcoming => $"A retirer {Pickup.ToLowerInvariant()}",
        ReservationStatus.Past => "Récupéré",
        _ => "Annulée"
    };
    public Color ChipBg => Status == ReservationStatus.Upcoming ? Color.FromArgb("#DDF3E3") : Color.FromArgb("#D4D4D4");
    public Color ChipFg => Status == ReservationStatus.Upcoming ? Color.FromArgb("#1B1B1B") : Color.FromArgb("#8E8E93");
}
