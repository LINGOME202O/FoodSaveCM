using System.Windows.Input;
using FoodSaveCM.Services;

namespace FoodSaveCM.Models;

public class CartItem : ObservableObject
{
    public Product Product { get; }
    public ICommand Increase { get; }
    public ICommand Decrease { get; }
    public ICommand Remove { get; }

    int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = Math.Clamp(value, 1, Math.Max(1, Product.Stock));
            Raise(); Raise(nameof(Total)); Raise(nameof(TotalText));
            AppState.RaiseCartChanged();
        }
    }

    public decimal Total => Product.Price * Quantity;
    public string TotalText => $"{Total:N0} FCFA";

    public CartItem(Product p)
    {
        Product = p;
        Increase = new Command(() => Quantity++);
        Decrease = new Command(() => { if (Quantity > 1) Quantity--; });
        Remove = new Command(() => { AppState.Cart.Remove(this); AppState.RaiseCartChanged(); });
    }
}
