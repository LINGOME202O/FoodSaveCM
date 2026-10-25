namespace FoodSaveCM.Models;

/// <summary>Chiffres du tableau de bord commerçant.</summary>
public class MerchantStats
{
    public int Published { get; set; }
    public int Reservations { get; set; }
    public int Pending { get; set; }
    public int PickedUp { get; set; }
    public decimal Revenue { get; set; }
    public decimal PaidOnline { get; set; }
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    public List<string> TopProducts { get; set; } = new();
}
