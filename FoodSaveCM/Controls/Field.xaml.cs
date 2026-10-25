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
        propertyChanged: (b, _, n) => ((Field)b).Input.IsPassword = (bool)n);

    public static readonly BindableProperty ShowEyeProperty = BindableProperty.Create(
        nameof(ShowEye), typeof(bool), typeof(Field), false,
        propertyChanged: (b, _, n) => ((Field)b).Eye.IsVisible = (bool)n);

    public static readonly BindableProperty KeyboardTypeProperty = BindableProperty.Create(
        nameof(KeyboardType), typeof(Keyboard), typeof(Field), Keyboard.Default,
        propertyChanged: (b, _, n) => ((Field)b).Input.Keyboard = (Keyboard)n);

    public static readonly BindableProperty CaptionMarginProperty = BindableProperty.Create(
        nameof(CaptionMargin), typeof(Thickness), typeof(Field), new Thickness(0),
        propertyChanged: (b, _, n) => ((Field)b).Lbl.Margin = (Thickness)n);

    public static readonly BindableProperty CaptionSizeProperty = BindableProperty.Create(
        nameof(CaptionSize), typeof(double), typeof(Field), 16.0,
        propertyChanged: (b, _, n) => ((Field)b).Lbl.FontSize = (double)n);

    public static readonly BindableProperty InputFontSizeProperty = BindableProperty.Create(
        nameof(InputFontSize), typeof(double), typeof(Field), 14.0,
        propertyChanged: (b, _, n) => ((Field)b).Input.FontSize = (double)n);

    public static readonly BindableProperty FieldHeightProperty = BindableProperty.Create(
        nameof(FieldHeight), typeof(double), typeof(Field), 46.0,
        propertyChanged: (b, _, n) => ((Field)b).Box.HeightRequest = (double)n);

    public static readonly BindableProperty CenterTextProperty = BindableProperty.Create(
        nameof(CenterText), typeof(bool), typeof(Field), true,
        propertyChanged: (b, _, n) => ((Field)b).Input.HorizontalTextAlignment = (bool)n ? TextAlignment.Center : TextAlignment.Start);

    public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public bool ShowEye { get => (bool)GetValue(ShowEyeProperty); set => SetValue(ShowEyeProperty, value); }
    public Keyboard KeyboardType { get => (Keyboard)GetValue(KeyboardTypeProperty); set => SetValue(KeyboardTypeProperty, value); }
    public Thickness CaptionMargin { get => (Thickness)GetValue(CaptionMarginProperty); set => SetValue(CaptionMarginProperty, value); }
    public double CaptionSize { get => (double)GetValue(CaptionSizeProperty); set => SetValue(CaptionSizeProperty, value); }
    public double InputFontSize { get => (double)GetValue(InputFontSizeProperty); set => SetValue(InputFontSizeProperty, value); }
    public double FieldHeight { get => (double)GetValue(FieldHeightProperty); set => SetValue(FieldHeightProperty, value); }
    public bool CenterText { get => (bool)GetValue(CenterTextProperty); set => SetValue(CenterTextProperty, value); }

    public Field() => InitializeComponent();

    void OnTextChanged(object? sender, TextChangedEventArgs e) => Text = e.NewTextValue ?? "";

    void OnEyeTapped(object? sender, TappedEventArgs e)
    {
        Input.IsPassword = !Input.IsPassword;
        Eye.Text = Input.IsPassword ? "👁" : "🙈";
    }
}
