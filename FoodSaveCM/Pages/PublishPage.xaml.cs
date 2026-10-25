using System.Text.RegularExpressions;
using FoodSaveCM.Models;
using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class PublishPage : ContentPage
{
    string? _photoPath;

    public PublishPage()
    {
        InitializeComponent();
    }

    static decimal Money(string? s)
    {
        var digits = Regex.Replace(s ?? "", @"[^\d]", "");
        return decimal.TryParse(digits, out var v) ? v : 0;
    }

    async void OnPhoto(object? sender, TappedEventArgs e)
    {
        try
        {
            var file = await MediaPicker.Default.PickPhotoAsync();
            if (file == null) return;
            var dest = Path.Combine(FileSystem.AppDataDirectory, $"pub_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");
            using (var src = await file.OpenReadAsync())
            using (var dst = File.Create(dest))
                await src.CopyToAsync(dst);
            _photoPath = dest;
            Preview.Source = ImageSource.FromFile(dest);
            Preview.IsVisible = true;
            PhotoHint.IsVisible = false;
        }
        catch { await Ui.Show("Impossible de charger la photo"); }
    }

    void ShowError(string m) { ErrorLabel.Text = m; ErrorLabel.IsVisible = true; }

    async void OnPublish(object? sender, EventArgs e)
    {
        var name = NameField.Text?.Trim() ?? "";
        var qty = (int)Money(QtyField.Text);
        var oldP = Money(OldPriceField.Text);
        var price = Money(PriceField.Text);
        var slot = string.IsNullOrWhiteSpace(SlotField.Text) ? "Aujourd'hui 17h-19h" : SlotField.Text.Trim();

        if (name.Length < 3) { ShowError("Donnez un nom au panier."); return; }
        if (qty < 1) { ShowError("Indiquez la quantité disponible."); return; }
        if (price <= 0) { ShowError("Indiquez le prix réduit."); return; }
        if (oldP > 0 && price >= oldP) { ShowError("Le prix réduit doit être inférieur au prix d'origine."); return; }

        var matches = Regex.Matches(slot, @"(\d{1,2})\s*h");
        var endHour = matches.Count > 0 ? int.Parse(matches[^1].Groups[1].Value) : 20;

        var product = new Product
        {
            Id = AppState.Products.Max(x => x.Id) + 1,
            Name = name,
            Details = "Publié par vous",
            Description = string.IsNullOrWhiteSpace(DescField.Text) ? "Invendu à sauver !" : DescField.Text.Trim(),
            Image = _photoPath ?? "pain.jpg",
            Category = "Paniers", ShopSub = "Invendus du jour",
            Price = price, OldPrice = oldP,
            Shop = $"Boutique de {AppState.FirstName}",
            Address = "Douala", Lat = 4.0511, Lon = 9.7043, Distance = 0.5,
            PickupText = slot, PickupEndHour = endHour,
            IsBasket = true, IsMine = true, Stock = qty
        };
        AppState.Products.Insert(0, product);

        await DisplayAlert("Publié ✅", "Votre invendu est maintenant visible par les clients proches.", "Super !");
        await Shell.Current.GoToAsync("publications");
    }
}
