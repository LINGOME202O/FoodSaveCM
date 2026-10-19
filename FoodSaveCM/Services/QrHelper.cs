// Phase C - Logique QR et gestion etat anti-gaspillage
using QRCoder;

namespace FoodSaveCM.Services;

public static class QrHelper
{
    public static ImageSource Generate(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(12);
        return ImageSource.FromStream(() => new MemoryStream(png));
    }
}
