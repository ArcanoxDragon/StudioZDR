using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using StudioZDR.App.Features;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Views;

public partial class MainWindow : BaseWindow<MainWindowViewModel>
{
	#region Static

	private const string FeatureIconPathTemplate = "avares://{0}/Assets/Images/Features/{1}.png";

	public static readonly IValueConverter FeatureIconConverter = new FuncValueConverter<IFeature, Bitmap?>(GetFeatureIconBitmap);

	private static Bitmap? GetFeatureIconBitmap(IFeature? feature)
	{
		if (string.IsNullOrEmpty(feature?.IconName))
			return null;

		var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
		var assemblyName = typeof(MainWindow).Assembly.GetName().Name;
		var uri = new Uri(string.Format(FeatureIconPathTemplate, assemblyName, feature.IconName), UriKind.Absolute);
		var stream = assetLoader?.Open(uri);

		return stream is null ? null : new Bitmap(stream);
	}

	#endregion

	public MainWindow()
	{
		InitializeComponent();
	}
}