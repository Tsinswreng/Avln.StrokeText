namespace Tsinswreng.Avln.StrokeText;

using Avalonia;
using Avalonia.Media.TextFormatting;

public partial class StrokeTextBlock{
/* -------------- 布局+折行 -------------- */
	//有時未能自動換行
	private void RebuildLayoutTextLayout() {
		_lines.Clear();
		if (string.IsNullOrEmpty(Text)) {
			InvalidateVisual();
			return;
		}

		double maxWidth = Bounds.Width - Padding.Left - Padding.Right;
		if (maxWidth <= 0) {
			InvalidateVisual();
			return;
		}

		var text = Text.AsMemory();
		int start = 0;

		// 用 TextLayout 一次性做完自动换行
		var tl = new TextLayout(
			text.Span.ToString(),
			Typeface,
			FontSize,
			Fill,
			Avalonia.Media.TextAlignment.Left,
			TextWrapping = this.TextWrapping
		);

		foreach (var tlLine in tl.TextLines) {
			int len = 0;
			foreach (var run in tlLine.TextRuns) {
				if (run is ShapedTextRun strun)
					len += strun.Length;
			}
			if (len <= 0 || start + len > text.Length) break;   // 加这一行

			var slice = text.Slice(start, len);
			_lines.Add(new TextLine {
				Text = slice.ToString(),
				Start = start,
				Length = len
			});
			start += len;
		}
		InvalidateVisual();
	}

	//有時未能自動換行
	private void RebuildLayoutTextLayout(double maxWidth) {
		_lines.Clear();
		if (string.IsNullOrEmpty(Text)) return;

		maxWidth -= Padding.Left + Padding.Right;
		if (maxWidth <= 0) return;

		var text = Text.AsMemory();
		int start = 0;

		var tl = new TextLayout(
			text.Span.ToString(),
			Typeface,
			FontSize,
			Fill,
			Avalonia.Media.TextAlignment.Left,
			textWrapping: this.TextWrapping,
			maxWidth: maxWidth
			//TextWrapping = this.TextWrapping
		){

		}
		;

		foreach (var tlLine in tl.TextLines) {
			int len = 0;
			foreach (var run in tlLine.TextRuns) {
				if (run is ShapedTextRun strun)
					len += strun.Length;
			}
			if (len <= 0 || start + len > text.Length) break;   // 加这一行

			var slice = text.Slice(start, len);
			_lines.Add(new TextLine {
				Text = slice.ToString(),
				Start = start,
				Length = len
			});
			start += len;
		}
	}


}
