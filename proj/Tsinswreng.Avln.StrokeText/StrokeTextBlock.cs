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
		TextProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.InvalidateVisual();
			x.InvalidateMeasure();
		});
		FillProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.InvalidateVisual());
		StrokeProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdatePen());
		StrokeThicknessProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => x.UpdatePen());
		FontSizeProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.InvalidateVisual();
			x.InvalidateMeasure();
		});
		FontFamilyProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.UpdateTypeface();
			x.InvalidateMeasure();
		});
		FontStyleProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.UpdateTypeface();
			x.InvalidateMeasure();
		});
		FontWeightProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.UpdateTypeface();
			x.InvalidateMeasure();
		});
		TextWrappingProperty.Changed.AddClassHandler<StrokeTextBlock>((x, _) => {
			x.InvalidateVisual();
			x.InvalidateMeasure();
		});
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

		if (string.IsNullOrEmpty(Text)) {
			return;
		}

		if (TextWrapping == TextWrapping.NoWrap) {
			_wrappedLines.Add(Text);
			return;
		}

		// Split text by lines first (preserve explicit line breaks)
		var rawLines = Text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

		foreach (var rawLine in rawLines) {
			if (string.IsNullOrEmpty(rawLine)) {
				// Empty line (from \n\n or similar)
				_wrappedLines.Add("");
				continue;
			}

			// Now wrap this line if it's too long
			var words = rawLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var wrappedLinesForThisLine = new List<string>();

			foreach (var word in words) {
				var wordWidth = MeasureTextWidth(word);

				// If the word itself is wider than maxWidth, break it into smaller chunks
				if (wordWidth > maxWidth) {
					// Break long words character by character
					var remainingWord = word;
					while (!string.IsNullOrEmpty(remainingWord)) {
						var chunk = GetMaxFittingText(remainingWord, maxWidth);
						if (!string.IsNullOrEmpty(chunk)) {
							wrappedLinesForThisLine.Add(chunk);
							remainingWord = remainingWord.Substring(chunk.Length);
						} else {
							// If even a single character doesn't fit, add it anyway
							wrappedLinesForThisLine.Add(remainingWord.Substring(0, 1));
							remainingWord = remainingWord.Substring(1);
						}
					}
				} else {
					// Check if word fits on current line
					if (wrappedLinesForThisLine.Count == 0 ||
						MeasureTextWidth(wrappedLinesForThisLine[wrappedLinesForThisLine.Count - 1] + " " + word) > maxWidth) {
						// Start new line
						wrappedLinesForThisLine.Add(word);
					} else {
						// Add to current line
						wrappedLinesForThisLine[wrappedLinesForThisLine.Count - 1] += " " + word;
					}
				}
			}

			_wrappedLines.AddRange(wrappedLinesForThisLine);
		}
	}

	private string GetMaxFittingText(string text, double maxWidth) {
		if (string.IsNullOrEmpty(text)) return "";

		// Binary search for the maximum length that fits
		var low = 1;
		var high = text.Length;
		var bestFit = "";

		while (low <= high) {
			var mid = (low + high) / 2;
			var substring = text.Substring(0, mid);
			var width = MeasureTextWidth(substring);

			if (width <= maxWidth) {
				bestFit = substring;
				low = mid + 1;
			} else {
				high = mid - 1;
			}
		}

		return bestFit;
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
			// Use Foreground property for text color
			var fillBrush = Foreground ?? Brushes.Black;

			foreach (var line in _wrappedLines) {
				if (!string.IsNullOrEmpty(line)) {
					var lineText = new FormattedText(
						line,
						CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						_typeface,
						FontSize,
						fillBrush
					);

					// Draw stroke
					var geometry = lineText.BuildGeometry(new Point(0, y));
					dc.DrawGeometry(null, _strokePen, geometry);

					// Draw fill
					dc.DrawText(lineText, new Point(0, y));
				}
				// Always advance Y position for each line, including empty lines (line breaks)
				y += _lineHeight;
			}
		}
	}

	protected override Size MeasureOverride(Size availableSize) {
		if (_formattedText == null) {
			UpdateFormattedText();
		}

		// Only wrap text if we have a finite width constraint
		if (!double.IsInfinity(availableSize.Width) && TextWrapping != TextWrapping.NoWrap) {
			WrapText(availableSize.Width);
		} else {
			// No wrapping - treat as single line
			_wrappedLines.Clear();
			_wrappedLines.Add(Text ?? "");
		}

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
