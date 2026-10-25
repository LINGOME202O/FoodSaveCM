using System.Text.RegularExpressions;
using SkiaSharp;
using ZXing.SkiaSharp;

namespace FoodSaveCM.Services;

/// <summary>Lit un QR code à partir d'une photo (appareil photo ou galerie).</summary>
public static class QrScanner
{
    public static async Task<string?> ScanAsync(bool fromCamera)
    {
        FileResult? file;
        if (fromCamera)
        {
            if (!MediaPicker.Default.IsCaptureSupported) return null;
            file = await MediaPicker.Default.CapturePhotoAsync();
        }
        else
        {
            file = await MediaPicker.Default.PickPhotoAsync();
        }
        if (file == null) return null;

        using var stream = await file.OpenReadAsync();
        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap == null) return null;

        var reader = new BarcodeReader { AutoRotate = true };
        reader.Options.TryHarder = true;
        var result = reader.Decode(bitmap);
        return ExtractCode(result?.Text);
    }

    /// <summary>Le QR contient "FOODSAVECM|id|code|commerce" : on en extrait le code de retrait.</summary>
    public static string? ExtractCode(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var parts = text.Split('|');
        if (parts.Length >= 3 && parts[0] == "FOODSAVECM") return parts[2];
        var m = Regex.Match(text, @"FS-\d{6}", RegexOptions.IgnoreCase);
        return m.Success ? m.Value.ToUpperInvariant() : null;
    }
}
