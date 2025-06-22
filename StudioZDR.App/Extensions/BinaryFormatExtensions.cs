using MercuryEngine.Data.Core.Framework;

namespace StudioZDR.App.Extensions;

internal static class BinaryFormatExtensions
{
	public static T DeepClone<T>(this T format)
	where T : BinaryFormat<T>, new()
	{
		T cloned = new();
		using MemoryStream stream = new((int) format.Size);

		format.Write(stream);
		stream.Seek(0, SeekOrigin.Begin);
		cloned.Read(stream);

		return cloned;
	}
}