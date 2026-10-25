using FoodSaveCM.Models;
using SQLite;

namespace FoodSaveCM.Services;

/// <summary>Ouvre (et crée au besoin) la base SQLite locale de l'application.</summary>
public class DatabaseService
{
    public SQLiteConnection Db { get; }

    public DatabaseService()
    {
        try { SQLitePCL.Batteries_V2.Init(); } catch { /* déjà initialisé */ }

        var path = Path.Combine(FileSystem.AppDataDirectory, "foodsavecm.db3");
        Db = new SQLiteConnection(path);

        Db.CreateTable<UserEntity>();
        Db.CreateTable<ProductEntity>();
        Db.CreateTable<ReservationEntity>();
        Db.CreateTable<CartEntity>();
        Db.CreateTable<FavoriteEntity>();
        Db.CreateTable<PaymentEntity>();
        Db.CreateTable<ReviewEntity>();
    }
}
