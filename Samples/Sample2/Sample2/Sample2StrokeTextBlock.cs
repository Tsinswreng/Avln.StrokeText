namespace Tsinswreng.Avln.StrokeText{
using VAlign = Avalonia.Layout.VerticalAlignment;
using HAlign = Avalonia.Layout.HorizontalAlignment;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Styling;

internal static class ExtnStyle{
	public static Style Set(
		this Style z, AvaloniaProperty property, object? value
	){
		z.Setters.Add(new Setter(property, value));
		return z;
	}
	public static Style Attach(
		this Style z
		,Styles Styles
		,Action<Style>? FnInit = null
	){
		FnInit?.Invoke(z);
		Styles.Add(z);
		return z;
	}
}

/// <summary>
/// TODO 直ᵈ把StrokeTextEdit置于ScrollViewer中旹 未顯者則直被裁掉 亦無法滾動
/// 這個控件直接放到ScrollViewer中滾動不生效、先套在別的佈局容器裏再放ScrollViewer裏滾動纔生效
/// </summary>
public partial class StrokeTextBlock : Control {

	// 静态构造里加回调
	static StrokeTextBlock() {
		TextProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) =>{
			x.RebuildLayout();
		});
		FillProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.InvalidateVisual());
		StrokeProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdatePen());
		StrokeThicknessProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdatePen());
		FontSizeProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.RebuildLayout());
		ForegroundProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.Fill = x.Foreground;
		});
		TextWrappingProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.RebuildLayout());
		FontFamilyProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.RebuildTypeface());
		FontStyleProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.RebuildTypeface());
		FontWeightProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.RebuildTypeface());
		UseVirtualizedRenderProperty.Changed.AddClassHandler<StrokeTextBlock>((x,_)=>{});//TODO
		ViewportProperty.Changed.AddClassHandler<StrokeTextBlock>((x, e) =>{
			x._viewport = (Rect)e.NewValue!;
			x.InvalidateVisual();      // 只重绘，不重新排版
		});
	}


	/* 可視區域（邏輯座標）*/
	private Rect _viewport = new Rect();

	internal static readonly AttachedProperty<Rect> ViewportProperty =
		AvaloniaProperty.RegisterAttached<StrokeTextBlock, Control, Rect>("Viewport");

	internal static void SetViewport(Control c, Rect r) => c.SetValue(ViewportProperty, r);
	internal static Rect GetViewport(Control c) => c.GetValue(ViewportProperty);

	private readonly List<TextLine> _lines = new();
	private int _caretIndex;

	private Pen _strokePen;

	private void UpdateTypeface(){
		Typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Normal);
	}


	private void UpdatePen() => _strokePen = new Pen(Stroke, StrokeThickness);

	public StrokeTextBlock() {
		//_typeface = new Typeface("Microsoft YaHei");
		Typeface = new Typeface(FontFamily.Default);
		_strokePen = new Pen(Stroke, StrokeThickness);
		UpdatePen();

		Focusable = true;

		Cursor = new Cursor(StandardCursorType.Ibeam);
		this.GetPropertyChangedObservable(BoundsProperty)
		.Subscribe(
			_ => RebuildLayout()
		);

		var a = new DynamicResourceExtension("TextControlForeground");
		var DfltSty = new Style()
		.Set(FontSizeProperty, new DynamicResourceExtension("ControlContentThemeFontSize"))
		.Set(ForegroundProperty, new DynamicResourceExtension("TextControlForeground"))
		.Set(FillProperty, new DynamicResourceExtension("TextControlForeground"))
		//.Set(StrokeProperty, new DynamicResourceExtension("TextControlForeground"))
		//.Set(StrokeProperty, new InvertForegroundBrushExtension())
		.Set(StrokeThicknessProperty, 1.0)
		//.Set(FontFamilyProperty, new DynamicResourceExtension("ContentControlThemeFontFamily"))
		.Set(VerticalContentAlignmentProperty, VAlign.Center)
		.Attach(Styles);
		if(Application.Current?.Resources.TryGetValue("SystemControlForegroundBaseHighBrush", out var brush)??false){
			if(brush is IBrush b){
				DfltSty.Set(ForegroundProperty, b);
			}
		}
	}


	private void RebuildTypeface(){
		var z = this;
		Typeface = new Typeface(FontFamily, z.FontStyle, FontWeight);
		RebuildLayout();   // 布局依赖字形度量，必须刷新
	}

	private FormattedText CreateFormattedText(string txt) =>
		new(txt, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
			Typeface, FontSize, Fill);


	// 在类里补一行字段
	private double _topOffset = 0;

	public override void Render(DrawingContext dc) {
		RenderAll(dc);
	}

	void InitContent(){
		// if(this is not ContentControl z){
		// 	return;
		// }

	}

	public void RenderAll(DrawingContext dc) {
		if (_lines.Count == 0){
			return;
		}

		double y = _topOffset;                 // 不再直接 Padding.Top
		foreach (var line in _lines) {
			var fmt = CreateFormattedText(line.Text);
			var origin = new Point(Padding.Left, y);

			var geo = fmt.BuildGeometry(origin);
			dc.DrawGeometry(null, new Pen(Stroke, StrokeThickness), geo);
			dc.DrawText(fmt, origin);

			y += fmt.Height;
		}
		if (IsFocused) DrawCaret(dc);
	}

	private Vector[] OutlineOffsets =>
		new Vector[]{ new(-StrokeThickness, -StrokeThickness),
		new( StrokeThickness, -StrokeThickness),
		new(-StrokeThickness,  StrokeThickness),
		new( StrokeThickness,  StrokeThickness) };

	private void DrawCaret(DrawingContext dc) {
		var (line, off) = FindCaretLine();
		if (line < 0){
			return;
		}
		var fmt = CreateFormattedText(_lines[line].Text[..off]);
		double x = Padding.Left + fmt.Width;
		double y = Padding.Top;
		for (int i = 0; i < line; i++){
			y += CreateFormattedText(_lines[i].Text).Height;
		}
		dc.DrawLine(new Pen(Fill, 1), new Point(x, y), new Point(x, y + fmt.Height));
	}

	private bool _needsReLayout = true;

	/*
	告訴布局系統「我需要多大」
	重寫 MeasureOverride(Size availableSize)，返回控件希望佔用的尺寸。
	如果裡面還有子元素，記得遞歸調用 child.Measure(...)。
	 */

	protected override Size MeasureOverride(Size availableSize){
		double width = availableSize.Width;
		if (double.IsInfinity(width)) {
			width = 1; // 给一个默认宽度，或者根据文本估算
		}

		if (_needsReLayout) {
			_needsReLayout = false;
			RebuildLayout(width);
		}

		var height = _lines.Count == 0
			? CreateFormattedText("A").Height
			: _lines.Sum(l => CreateFormattedText(l.Text).Height);

		return new Size(width, height + Padding.Top + Padding.Bottom);
	}



	/*
	告訴布局系統「我怎麼擺」
	重寫 ArrangeOverride(Size finalSize)，把子元素或自己的繪圖區域安排到最終矩形。
	最後必須返回實際使用的 finalSize。
	 */
	protected override Size ArrangeOverride(Size finalSize) {
		if (finalSize.Width > 0 && _needsReLayout) {
			_needsReLayout = false;
			RebuildLayout(finalSize.Width);
			//InvalidateMeasure(); //恐觸死循環
		}
		return finalSize;
	}


	private void RebuildLayout() {
		_lines.Clear();
		if (string.IsNullOrEmpty(Text)) {
			InvalidateVisual();
			return;
		}

		var maxWidth = Bounds.Width - Padding.Left - Padding.Right;
		if (maxWidth <= 0) {
			InvalidateVisual();
			return;
		}

		var text = Text.AsMemory();
		int start = 0;
		while (start < text.Length) {
			int len = BreakLine(text.Slice(start), maxWidth);
			_lines.Add(new TextLine {
				Text = text.Slice(start, len).ToString(),
				Start = start,
				Length = len
			});
			start += len;
		}
		InvalidateVisual();
	}

	private void RebuildLayout(double maxWidth) {
		_lines.Clear();
		if (string.IsNullOrEmpty(Text)) return;
		maxWidth -= Padding.Left + Padding.Right;
		if (maxWidth <= 0) return;

		var text = Text.AsMemory();
		int start = 0;
		while (start < text.Length) {
			int len = BreakLine(text.Slice(start), maxWidth);
			_lines.Add(new TextLine { Text = text.Slice(start, len).ToString() });
			start += len;
		}
	}

	/* -------------- 光标定位 -------------- */
	private (int line, int off) FindCaretLine() {
		int acc = 0;
		for (int i = 0; i < _lines.Count; i++) {
			int next = acc + _lines[i].Length;
			if (_caretIndex <= next)
				return (i, _caretIndex - acc);
			acc = next;
		}
		return (_lines.Count - 1, _lines[^1].Text.Length);
	}

	/* -------------- 简单 Padding -------------- */
	private Thickness _padding = new(0);
	public Thickness Padding {
		get => _padding;
		set { _padding = value; RebuildLayout(); }
	}

	// 简单英文/中文断行，生产环境可换成 TextLayout
	private int BreakLine(ReadOnlyMemory<char> slice, double maxWidth) {
		if (slice.Length == 0) return 0;
		if (TextWrapping == Avalonia.Media.TextWrapping.NoWrap) return slice.Length;
		var fmt = CreateFormattedText(slice.Span.ToString());
		if (fmt.Width <= maxWidth) return slice.Length;

		int low = 0, high = slice.Length;
		while (low < high) {
			int mid = low + high + 1 >> 1;
			fmt = CreateFormattedText(slice.Span.Slice(0, mid).ToString());
			if (fmt.Width <= maxWidth)
				low = mid;
			else
				high = mid - 1;
		}
		return low == 0 ? 1 : low;
	}

}


record TextLine {
	public string Text { get; init; }
	public int Start { get; init; }
	public int Length { get; init; }
}


}//~Ns


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

