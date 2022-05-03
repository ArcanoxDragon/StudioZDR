using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StudioZDR.App.Features;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Framework;

public class FeatureViewLocator : IDataTemplate
{
	private static readonly Assembly AppAssembly = typeof(IFeature).Assembly;
	private static readonly Assembly UiAssembly  = typeof(FeatureViewLocator).Assembly;

	private static readonly string AppAssemblyName = AppAssembly.GetName().Name!;
	private static readonly string UiAssemblyName  = UiAssembly.GetName().Name!;

	public IControl Build(object? data)
	{
		var name = data?.GetType().FullName!.Replace("ViewModel", "View").Replace(AppAssemblyName, UiAssemblyName);
		var type = name is null ? null : Type.GetType(name);

		if (type == null)
			return new TextBlock { Text = "Not Found: " + name };

		return (Control) Activator.CreateInstance(type)!;
	}

	public bool Match(object? data) => data is ViewModelBase;
}