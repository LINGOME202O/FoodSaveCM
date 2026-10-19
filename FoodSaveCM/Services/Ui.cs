// Fonctionnalitée ajoutée 2 - Mode sombre : économie batterie et confort visuel pour consultation le soir des invendus du jour
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace FoodSaveCM.Services;

public static class Ui
{
    public static Task Show(string message)
        => MainThread.InvokeOnMainThreadAsync(() => Toast.Make(message, ToastDuration.Short, 14).Show());
}
