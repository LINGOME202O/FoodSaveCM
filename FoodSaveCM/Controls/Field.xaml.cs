namespace FoodSaveCM.Controls;

public partial class Field : ContentView
{
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption), typeof(string), typeof(Field), string.Empty,
        propertyChanged: (b, _, n) => ((Field)b).Lbl.Text = (string)n);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(Field), string.Empty,
        propertyChanged: (b, _, n) => ((Field)b).Input.Placeholder = (string)n);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(Field), string.Empty, BindingMode.TwoWay,
        propertyChanged: (b, _, n) =>
        {
            var f = (Field)b;
            if (f.Input.Text != (string)n) f.Input.Text = (string)n;
        });

    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
        nameof(IsPassword), typeof(bool), typeof(Field), false,
        propertyChanged: (b, _, n) =>
        {
            var f = (Field)b;
            f.Input.IsPassword = (bool)n;
            f.Eye.IsVisible = (bool)n;
        });

    public static readonly BindableProperty KeyboardTypeProperty = BindableProperty.Create(
        nameof(KeyboardType), typeof(Keyboard), typeof(Field), Keyboard.Default,
        propertyChanged: (b, _, n) => ((Field)b).Input.Keyboard = (Keyboard)n);

    public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public Keyboard KeyboardType { get => (Keyboard)GetValue(KeyboardTypeProperty); set => SetValue(KeyboardTypeProperty, value); }

    public Field() => InitializeComponent();

    void OnTextChanged(object? sender, TextChangedEventArgs e) => Text = e.NewTextValue ?? "";

    void OnEyeTapped(object? sender, TappedEventArgs e)
    {
        Input.IsPassword = !Input.IsPassword;
        Eye.Text = Input.IsPassword ? "👁" : "🙈";
    }
}
