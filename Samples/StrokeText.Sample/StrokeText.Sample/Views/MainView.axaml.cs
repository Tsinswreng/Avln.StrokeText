using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Ngaq.Ui.StrokeText;

namespace StrokeText.Sample.Views;

public partial class MainView : UserControl {
	StrokeTextBlock Txt(){
		var R = new StrokeTextBlock{
			FontSize = 24,
			Foreground = Brushes.White,
			Stroke = Brushes.Black,
			StrokeThickness = 5
		};
		return R;
	}
	public MainView() {
		var Sp = new StackPanel();
		Content = Sp;
		{var o = Sp;
			var uri = new Uri("avares://StrokeText.Sample/Assets/Bg.png");
			using var stream = AssetLoader.Open(uri);
			var bitmap = new Bitmap(stream);
			o.Background = new ImageBrush(bitmap);
		}
		{
			{
				var o = Txt();
				o.Text = "NoWrap:\nABCabc123一二三";
				Sp.Children.Add(o);
			}
			{
				var o = Txt();
				o.Text = "\nWrap:\nABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三ABCabc123一二三";
				o.TextWrapping = TextWrapping.Wrap;
				Sp.Children.Add(o);
			}
		}

	}
}
