#if true
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
// using Avalonia.Skia;
// using SkiaSharp;

//undone. only for test
namespace Ngaq.Ui.Views.TestStroke {

	public class MainWindow2 : Window {
		public MainWindow2() {
			Width = 450;
			Height = 200;
			Title = "Avalonia 文字描邊（可複製）";

			// 背景隨便放個漸層，方便驗證描邊效果
			Background = new LinearGradientBrush {
				StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
				EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
				GradientStops =
				{
					new GradientStop(Colors.White, 0),
					new GradientStop(Colors.Yellow, 0.5),
					new GradientStop(Colors.White, 1)
				}
			};

			// 要顯示的文字
			const string txt = "Hello 中文描邊";

			// 字體相關參數
			var font = new Typeface("Arial", FontStyle.Normal, FontWeight.Normal);
			double fontSize = 60;

			// 2. 正确顺序：culture、flowDirection、typeface、size、text、brush
			var formatted = new FormattedText(
				txt,
				CultureInfo.CurrentUICulture,
				FlowDirection.LeftToRight,
				font,
				60,
				Brushes.Black
			);
			var textGeometry = formatted.BuildGeometry(new Point(20, 40));

			// 2. 自定義控制項：負責「只畫描邊」
			var outline = new OutlineControl {
				Geometry = textGeometry,
				Stroke = Brushes.Black,
				StrokeThickness = 3,
				[Canvas.LeftProperty] = 0d,
				[Canvas.TopProperty] = 0d
			};

			// 3. 再放一個完全重疊、透明的 TextBlock，負責「能被選中／複製」
			var ghostText = new SelectableTextBlock {
				Text = txt,
				FontSize = fontSize,
				FontFamily = new FontFamily("Arial"),
				Foreground = Brushes.Transparent, // 肉眼看不見
				[Canvas.LeftProperty] = 20d,
				[Canvas.TopProperty] = 40d,
				SelectionForegroundBrush = Brushes.White,
			};

			// 4. 用 Canvas 把兩層疊起來
			Content = new Canvas {
				Children = { outline, ghostText }
			};
		}
	}

	// ============ 僅做「描邊」的自繪控制項 ============
	public class OutlineControl : Control {
		public static readonly StyledProperty<Geometry> GeometryProperty =
			AvaloniaProperty.Register<OutlineControl, Geometry>(nameof(Geometry));

		public static readonly StyledProperty<IBrush> StrokeProperty =
			AvaloniaProperty.Register<OutlineControl, IBrush>(nameof(Stroke));

		public static readonly StyledProperty<double> StrokeThicknessProperty =
			AvaloniaProperty.Register<OutlineControl, double>(nameof(StrokeThickness));

		public Geometry Geometry {
			get => GetValue(GeometryProperty);
			set => SetValue(GeometryProperty, value);
		}

		public IBrush Stroke {
			get => GetValue(StrokeProperty);
			set => SetValue(StrokeProperty, value);
		}

		public double StrokeThickness {
			get => GetValue(StrokeThicknessProperty);
			set => SetValue(StrokeThicknessProperty, value);
		}

		// 關鍵：override Render，直接畫邊框
		public override void Render(DrawingContext context) {
			var g = Geometry;
			if (g == null) return;

			var pen = new Pen(Stroke, StrokeThickness);
			context.DrawGeometry(null, pen, g); // 第一個參數是 Fill，這裡給 null 只畫邊
		}
	}
}

#endif
