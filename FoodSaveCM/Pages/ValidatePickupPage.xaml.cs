using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class ValidatePickupPage : ContentPage
{
    public ValidatePickupPage() => InitializeComponent();

    void Show(string message, bool ok)
    {
        ResultLabel.Text = message;
        ResultLabel.TextColor = ok ? Color.FromArgb("#1B5E20") : Color.FromArgb("#D32F2F");
        ResultBox.IsVisible = true;
    }

    void OnValidate(object? sender, EventArgs e)
    {
        var (ok, message) = AppState.ValidatePickup(CodeEntry.Text ?? "");
        Show(message, ok);
        if (ok) CodeEntry.Text = "";
    }

    async void OnScanCamera(object? sender, EventArgs e) => await Scan(true);
    async void OnScanGallery(object? sender, EventArgs e) => await Scan(false);

    async Task Scan(bool camera)
    {
        try
        {
            var code = await QrScanner.ScanAsync(camera);
            if (code == null)
            {
                Show("Aucun QR code FoodSaveCM détecté. Réessayez avec une photo plus nette.", false);
                return;
            }
            CodeEntry.Text = code;
            OnValidate(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Show("Impossible d'utiliser l'appareil photo : " + ex.Message, false);
        }
    }
}
