using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioZDR.App.Configuration;

public class ApplicationSettings : IJsonOnSerializing, IJsonOnDeserialized
{
	private readonly Dictionary<string, RealizedSettings> realizedFeatureSettings = [];

	public string? RomFsLocation
	{
		get;
		set
		{
			field = value;
			IsRomFsLocationValid = !string.IsNullOrEmpty(value) && Directory.Exists(value);
		}
	}

	public string? OutputLocation
	{
		get;
		set
		{
			field = value;
			IsOutputLocationValid = !string.IsNullOrEmpty(value) && Directory.Exists(value);
		}
	}

	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Dictionary<string, JsonElement> FeatureSettings { get; } = new();

	[JsonIgnore]
	[MemberNotNullWhen(true, nameof(RomFsLocation))]
	public bool IsRomFsLocationValid { get; private set; }

	[JsonIgnore]
	[MemberNotNullWhen(true, nameof(OutputLocation))]
	public bool IsOutputLocationValid { get; private set; }

	public T GetFeatureSettings<T>()
	where T : class, IFeatureSettings<T>, new()
	{
		// First, try to get an instance from our "realized instances" dictionary.
		if (this.realizedFeatureSettings.TryGetValue(T.Key, out var realizedSettings) && realizedSettings.SettingsType == typeof(T))
			return (T) realizedSettings.Value;

		T settings;

		if (FeatureSettings.TryGetValue(T.Key, out var settingsElement))
		{
			// Try to deserialize the JSON element into "T".
			// Upon failure, we simply instantiate a new "T".

			try
			{
				settings = (T?) settingsElement.Deserialize(typeof(T), T.SerializationContext) ?? new T();
			}
			catch
			{
				// Ignore all JSON exceptions and "fail safe" by instantiating a new T.
				settings = new T();
			}
		}
		else
		{
			// Not present in JSON - initialize empty instance.
			settings = new T();
		}

		// Put the instance in our "realized instances" dictionary. The members of that dictionary
		// will be re-serialized into JsonElements whenever this class is serialized, so that changes
		// made to the "settings" instance we return will be included in the serialized JSON.
		this.realizedFeatureSettings[T.Key] = new RealizedSettings<T>(settings);

		return settings;
	}

	public void CopyTo(ApplicationSettings other)
	{
		other.RomFsLocation = RomFsLocation;
		other.OutputLocation = OutputLocation;

		// Serialize our own "realized" settings into JsonElements and copy those into the other settings.
		// Also copy the values of any of our "realized" settings into matching other settings.
		OnSerializing();

		other.FeatureSettings.Clear();

		foreach (var (key, settingsElement) in FeatureSettings)
			other.FeatureSettings.Add(key, settingsElement);

		// Try to copy values from our settings to any "realized" instances on the other settings
		foreach (var (_, otherRealized) in other.realizedFeatureSettings)
			otherRealized.CopyFrom(this);
	}

	#region JSON Hooks

	public void OnSerializing()
	{
		// Serialize all "realized" feature settings instances back into JsonElement instances.

		foreach (var (key, (settingsType, jsonContext, instance)) in this.realizedFeatureSettings)
			FeatureSettings[key] = JsonSerializer.SerializeToElement(instance, settingsType, jsonContext);
	}

	public void OnDeserialized()
	{
		// Clear out our "realized" feature settings instances so that they will be deserialized
		// anew from the FeatureSettings dictionary.

		this.realizedFeatureSettings.Clear();
	}

	#endregion

	#region Helper Types

	private abstract record RealizedSettings(Type SettingsType, JsonSerializerContext JsonContext, object Value)
	{
		public abstract void CopyFrom(ApplicationSettings source);
	}

	private sealed record RealizedSettings<T>(T ValueAsT) : RealizedSettings(typeof(T), T.SerializationContext, ValueAsT)
	where T : class, IFeatureSettings<T>, new()
	{
		/// <summary>
		/// Helper method for populating target "realized" settings instances from source settings
		/// </summary>
		public override void CopyFrom(ApplicationSettings source)
		{
			T sourceFeatureSettings = source.GetFeatureSettings<T>();

			sourceFeatureSettings.CopyTo(ValueAsT);
		}
	}

	#endregion
}