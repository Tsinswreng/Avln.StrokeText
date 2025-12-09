namespace Tsinswreng.Avln.StrokeText{

using Avalonia;
	using Avalonia.Layout;
	using Avalonia.Media;

public partial class StrokeTextBlock{
	public static readonly StyledProperty<bool> UseVirtualizedRenderProperty =
		AvaloniaProperty.Register<StrokeTextBlock, bool>(nameof(UseVirtualizedRender), false);

	public bool UseVirtualizedRender{
		get => GetValue(UseVirtualizedRenderProperty);
		set => SetValue(UseVirtualizedRenderProperty, value);
	}

public static readonly StyledProperty<string> TextProperty =
	AvaloniaProperty.Register<StrokeTextBlock, string>(nameof(Text), defaultValue: "",
		coerce: (_, v) => v ?? "");

	public string Text {
		get => GetValue(TextProperty);
		set{
			SetValue(TextProperty, value);
		}
	}

	public static readonly StyledProperty<FontStyle> FontStyleProperty
	=AvaloniaProperty.Register<StrokeTextBlock, FontStyle>(nameof(FontStyle), FontStyle.Normal);

	public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
		AvaloniaProperty.Register<StrokeTextBlock, TextWrapping>(nameof(Avalonia.Media.TextWrapping), Avalonia.Media.TextWrapping.NoWrap);

	public TextWrapping TextWrapping{
		get => GetValue(TextWrappingProperty);
		set => SetValue(TextWrappingProperty, value);
	}

	public static readonly StyledProperty<IBrush> ForegroundProperty =
		AvaloniaProperty.Register<StrokeTextBlock, IBrush>(nameof(Foreground), Brushes.Black);

	public IBrush Foreground {
		get => GetValue(ForegroundProperty);
		set => SetValue(ForegroundProperty, value);
	}

	// 注册三个可绑属性
	public static readonly StyledProperty<IBrush> FillProperty =
		AvaloniaProperty.Register<StrokeTextBlock, IBrush>(nameof(Fill), Brushes.Black);

	public static readonly StyledProperty<IBrush> StrokeProperty =
		AvaloniaProperty.Register<StrokeTextBlock, IBrush>(nameof(Stroke), Brushes.Black);

	public static readonly StyledProperty<double> FontSizeProperty =
		AvaloniaProperty.Register<StrokeTextBlock, double>(nameof(FontSize), 16d);

	public IBrush Fill {
		get => GetValue(FillProperty);
		set => SetValue(FillProperty, value);
	}

	public IBrush Stroke {
		get => GetValue(StrokeProperty);
		set => SetValue(StrokeProperty, value);
	}

	public double FontSize {
		get => GetValue(FontSizeProperty);
		set => SetValue(FontSizeProperty, value);
	}

	public static readonly StyledProperty<double> StrokeThicknessProperty =
	AvaloniaProperty.Register<StrokeTextBlock, double>(nameof(StrokeThickness), 2.5);

	public double StrokeThickness {
		get => GetValue(StrokeThicknessProperty);
		set => SetValue(StrokeThicknessProperty, value);
	}

	public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
		AvaloniaProperty.Register<StrokeTextBlock, VerticalAlignment>(nameof(VerticalContentAlignment), VerticalAlignment.Center);

	public VerticalAlignment VerticalContentAlignment {
		get => GetValue(VerticalContentAlignmentProperty);
		set => SetValue(VerticalContentAlignmentProperty, value);
	}

	public static readonly StyledProperty<FontFamily> FontFamilyProperty =
		AvaloniaProperty.Register<StrokeTextBlock, FontFamily>(nameof(FontFamily), FontFamily.Default);

	public FontFamily FontFamily{
		get => GetValue(FontFamilyProperty);
		set => SetValue(FontFamilyProperty, value);
	}

	public static readonly StyledProperty<FontWeight> FontWeightProperty
	=AvaloniaProperty.Register<StrokeTextBlock, FontWeight>(nameof(FontWeight), FontWeight.Normal);

	private static readonly StyledProperty<Typeface> TypefaceProperty
	=AvaloniaProperty.Register<StrokeTextBlock, Typeface>(nameof(Typeface), new Typeface(FontFamily.Default));

	public FontWeight FontWeight{
		get => GetValue(FontWeightProperty);
		set => SetValue(FontWeightProperty, value);
	}

	public FontStyle FontStyle{
		get => GetValue(FontStyleProperty);
		set => SetValue(FontStyleProperty, value);
	}

	public Typeface Typeface{
		get => GetValue(TypefaceProperty);
		private set => SetValue(TypefaceProperty, value);
	}

}

}//~Ns
