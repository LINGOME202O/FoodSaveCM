namespace FoodSaveCM.Models;

public enum ReservationStatus { Upcoming, Past, Cancelled }

public class Reservation : ObservableObject
{
    public string Id { get; set; } = "";
    public string Shop { get; set; } = "";
    public string Title { get; set; } = "Panier anti-gaspi";
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
        set { _status = value; Raise(); Raise(nameof(IsUpcoming)); Raise(nameof(Chip)); Raise(nameof(ChipBg)); }
    }

    public bool IsUpcoming => Status == ReservationStatus.Upcoming;
    public string Icon => Shop.Contains("Boulangerie") ? "🥖" : Shop.Contains("Marché") ? "🧺" : "🛒";
    public string Summary => $"{DateText} . {Title}\n{Count} article{(Count > 1 ? "s" : "")}";
    public string Chip => Status switch
    {
        ReservationStatus.Upcoming => $"À retirer {Pickup.ToLowerInvariant()}",
        ReservationStatus.Past => "Récupéré",
        _ => "Annulée"
    };
    public Color ChipBg => Status == ReservationStatus.Upcoming ? Color.FromArgb("#E8F8EC") : Color.FromArgb("#D9D9D9");
}
