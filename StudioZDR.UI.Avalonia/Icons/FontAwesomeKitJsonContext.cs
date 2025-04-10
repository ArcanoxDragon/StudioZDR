using System.Text.Json.Serialization;
using StudioZDR.UI.Avalonia.Icons.Models;

namespace StudioZDR.UI.Avalonia.Icons;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, FontAwesomeKitIcon>))]
internal partial class FontAwesomeIconsJsonContext : JsonSerializerContext;