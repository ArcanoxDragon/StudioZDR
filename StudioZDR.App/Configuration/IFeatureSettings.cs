using System.Text.Json.Serialization;

namespace StudioZDR.App.Configuration;

public interface IFeatureSettings<in T>
where T : IFeatureSettings<T>
{
	static abstract string                Key                  { get; }
	static abstract JsonSerializerContext SerializationContext { get; }

	void CopyTo(T other);
}