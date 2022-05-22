using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StudioZDR.UI.Avalonia.Views.Controls
{
	public partial class GroupBox : ContentControl
	{
		public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<GroupBox, object?>(nameof(Header), null);

		public GroupBox()
		{
			InitializeComponent();
		}

		public object? Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}