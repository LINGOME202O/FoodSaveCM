using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace FoodSaveCM.Services;

public static class Ui
{
    public static Task Show(string message)
        => MainThread.InvokeOnMainThreadAsync(() => Toast.Make(message, ToastDuration.Short, 14).Show());
}
