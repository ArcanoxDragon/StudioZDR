#if DEBUG
using System.Reflection.Metadata;
using StudioZDR.UI.Avalonia.Utility;

[assembly: MetadataUpdateHandler(typeof(HotReloadHelper))]

namespace StudioZDR.UI.Avalonia.Utility;

internal static class HotReloadHelper
{
	public static event Action<Type[]?>? MetadataUpdated;

	internal static void ClearCache(Type[]? updatedTypes) { }

	internal static void UpdateApplication(Type[]? updatedTypes)
	{
		MetadataUpdated?.Invoke(updatedTypes);
	}
}
#endif