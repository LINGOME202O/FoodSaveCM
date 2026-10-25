using SQLite;

namespace FoodSaveCM.Models;

// ======================================================================
//  Tables de la base de données SQLite (foodsavecm.db3)
// ======================================================================

[Table("Users")]
public class UserEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed(Unique = true)] public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Salt { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Products")]
public class ProductEntity
{
    [PrimaryKey] public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Details { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public string Shop { get; set; } = "";
    public string ShopSub { get; set; } = "";
    public string Address { get; set; } = "";
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double Distance { get; set; }
    public string PickupText { get; set; } = "";
    public int PickupEndHour { get; set; } = 20;
    public bool IsBasket { get; set; }
    public int OwnerId { get; set; }           // 0 = produit du catalogue, sinon id du commerçant qui l'a publié
    public int Stock { get; set; }
    public string ContentsText { get; set; } = ""; // lignes séparées par '|'
}

[Table("Reservations")]
public class ReservationEntity
{
    [PrimaryKey] public string Id { get; set; } = "";
    [Indexed] public int UserId { get; set; }
    public int ProductId { get; set; }
    public string Shop { get; set; } = "";
    public string ShopSub { get; set; } = "";
    public string Headline { get; set; } = "";
    public string Title { get; set; } = "";
    public string DateText { get; set; } = "";
    public string Pickup { get; set; } = "";
    public string Address { get; set; } = "";
    [Indexed] public string Code { get; set; } = "";
    public int Count { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public decimal Total { get; set; }
    public decimal Saved { get; set; }
    public int Status { get; set; }            // 0 = à venir, 1 = passée (récupérée), 2 = annulée
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string PaymentRef { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Carts")]
public class CartEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

[Table("Favorites")]
public class FavoriteEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int UserId { get; set; }
    public int ProductId { get; set; }
}

[Table("Payments")]
public class PaymentEntity
{
    [PrimaryKey] public string Id { get; set; } = "";     // référence du paiement (PAY-XXXXXXXX)
    [Indexed] public int UserId { get; set; }
    [Indexed] public string ReservationId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Method { get; set; } = "";               // Orange Money / MTN MoMo
    public string PhoneMasked { get; set; } = "";
    public string Status { get; set; } = "Payé";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Reviews")]
public class ReviewEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public int UserId { get; set; }
    [Indexed] public string ReservationId { get; set; } = "";
    [Indexed] public string Shop { get; set; } = "";
    public int Stars { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
