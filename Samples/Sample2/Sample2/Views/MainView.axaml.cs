using Avalonia.Controls;
using Tsinswreng.Avln.StrokeText;
namespace Sample2.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        Content = new StrokeTextBlock{
			Text = "ABCabc123一二三"
		};
    }
}
