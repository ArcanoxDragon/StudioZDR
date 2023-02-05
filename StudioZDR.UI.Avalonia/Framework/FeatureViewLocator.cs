using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Framework;

public class FeatureViewLocator : IDataTemplate
{
	private const string AppNamespaceName = $"{nameof(StudioZDR)}.{nameof(StudioZDR.App)}";
	private const string UiNamespaceName  = $"{nameof(StudioZDR)}.{nameof(StudioZDR.UI)}.{nameof(StudioZDR.UI.Avalonia)}";

	public IControl Build(object? data)
	{
		var name = data?.GetType().FullName!.Replace("ViewModel", "View").Replace(AppNamespaceName, UiNamespaceName);
		var type = name is null ? null : Type.GetType(name);

		if (type == null)
			return new TextBlock { Text = "Not Found: " + name };

		return (Control) Activator.CreateInstance(type)!;
	}

	public bool Match(object? data) => data is ViewModelBase;
}