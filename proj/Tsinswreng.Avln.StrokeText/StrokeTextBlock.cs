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
using Avalonia.LogicalTree;
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
/// StrokeTextBlock with basic functionality - renders stroked text
/// </summary>
public partial class StrokeTextBlock : Control {

	// Static constructor with property change handlers
	static StrokeTextBlock() {
		TextProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.InvalidateVisual());
		FillProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.InvalidateVisual());
		StrokeProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdatePen());
		StrokeThicknessProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdatePen());
		FontSizeProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.InvalidateVisual());
		FontFamilyProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdateTypeface());
		FontStyleProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdateTypeface());
		FontWeightProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdateTypeface());
		TextWrappingProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.InvalidateVisual());
	}

	private Pen _strokePen;
	private Typeface _typeface;
	private FormattedText? _formattedText;
	private List<string> _wrappedLines = new();
	private double _lineHeight;

	private void UpdateTypeface() {
		_typeface = new Typeface(FontFamily, FontStyle, FontWeight);
		InvalidateVisual();
	}

	private void UpdatePen() {
		_strokePen = new Pen(Stroke, StrokeThickness);
	}

	public StrokeTextBlock() {
		_typeface = new Typeface(FontFamily.Default);
		_strokePen = new Pen(Stroke, StrokeThickness);

		Focusable = true;
		Cursor = new Cursor(StandardCursorType.Ibeam);

		var DfltSty = new Style()
		.Set(FontSizeProperty, new DynamicResourceExtension("ControlContentThemeFontSize"))
		.Set(ForegroundProperty, new DynamicResourceExtension("TextControlForeground"))
		.Set(FillProperty, new DynamicResourceExtension("TextControlForeground"))
		.Set(StrokeThicknessProperty, 1.0)
		.Attach(Styles);
	}

	private void UpdateFormattedText() {
		if (string.IsNullOrEmpty(Text)) {
			_formattedText = null;
			_wrappedLines.Clear();
			return;
		}

		_formattedText = new FormattedText(
			"X", // Single character to measure line height
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			_typeface,
			FontSize,
			Fill
		);

		_lineHeight = _formattedText.Height;
	}

	private void WrapText(double maxWidth) {
		_wrappedLines.Clear();

		if (string.IsNullOrEmpty(Text) || TextWrapping == TextWrapping.NoWrap) {
			_wrappedLines.Add(Text ?? "");
			return;
		}

		var words = Text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		var lines = new List<string>();
		var currentLine = "";
		var currentWidth = 0.0;

		foreach (var word in words) {
			var wordWidth = MeasureTextWidth(word);
			var spaceWidth = currentLine.Length > 0 ? MeasureTextWidth(" ") : 0;

			if (currentWidth + spaceWidth + wordWidth <= maxWidth) {
				// Add word to current line
				if (currentLine.Length > 0) {
					currentLine += " ";
					currentWidth += spaceWidth;
				}
				currentLine += word;
				currentWidth += wordWidth;
			} else {
				// Start new line
				if (currentLine.Length > 0) {
					lines.Add(currentLine);
				}
				currentLine = word;
				currentWidth = wordWidth;
			}
		}

		// Add the last line
		if (currentLine.Length > 0) {
			lines.Add(currentLine);
		}

		_wrappedLines.AddRange(lines);
	}

	private double MeasureTextWidth(string text) {
		if (string.IsNullOrEmpty(text)) return 0;

		var tempText = new FormattedText(
			text,
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			_typeface,
			FontSize,
			Fill
		);

		return tempText.Width;
	}

	public override void Render(DrawingContext dc) {
		if (_formattedText == null) {
			UpdateFormattedText();
		}

		if (_formattedText != null && _wrappedLines.Count > 0) {
			var y = 0.0;

			foreach (var line in _wrappedLines) {
				if (!string.IsNullOrEmpty(line)) {
					var lineText = new FormattedText(
						line,
						CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						_typeface,
						FontSize,
						Fill
					);

					// Draw stroke
					var geometry = lineText.BuildGeometry(new Point(0, y));
					dc.DrawGeometry(null, _strokePen, geometry);

					// Draw fill
					dc.DrawText(lineText, new Point(0, y));
				}

				y += _lineHeight;
			}
		}
	}

	protected override Size MeasureOverride(Size availableSize) {
		if (_formattedText == null) {
			UpdateFormattedText();
		}

		// Wrap text based on available width
		WrapText(availableSize.Width);

		// Calculate total width and height
		var totalWidth = 0.0;
		var totalHeight = _wrappedLines.Count * _lineHeight;

		foreach (var line in _wrappedLines) {
			var lineWidth = MeasureTextWidth(line);
			totalWidth = Math.Max(totalWidth, lineWidth);
		}

		// Constrain to available size
		if (!double.IsInfinity(availableSize.Width)) {
			totalWidth = Math.Min(totalWidth, availableSize.Width);
		}
		if (!double.IsInfinity(availableSize.Height)) {
			totalHeight = Math.Min(totalHeight, availableSize.Height);
		}

		// Ensure valid values
		if (double.IsNaN(totalWidth) || totalWidth < 0) totalWidth = 0;
		if (double.IsNaN(totalHeight) || totalHeight < 0) totalHeight = 0;

		return new Size(totalWidth, totalHeight);
	}
}
}
