using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StudioZDR.UI.Avalonia.Features.SaveEditor.Views
{
	public partial class InventoryView : UserControl
	{
		public InventoryView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
