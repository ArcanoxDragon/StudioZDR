using System.Text;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;

namespace StudioZDR.App.Extensions;

internal static class BinaryFormatExtensions
{
	private static readonly Encoding DefaultEncoding = new UTF8Encoding();

	public static T DeepClone<T>(this T obj)
	where T : IBinaryField, new()
	{
		T cloned = new();

		using MemoryStream stream = new((int) obj.GetSize(0));

		// Write source object
		{
			var heapManager = new HeapManager();
			var writeContext = new WriteContext(heapManager);
			using var writer = new BinaryWriter(stream, DefaultEncoding, true);

			obj.Write(writer, writeContext);
		}

		stream.Seek(0, SeekOrigin.Begin);

		// Read into cloned object
		{
			var heapManager = new HeapManager();
			var readContext = new ReadContext(heapManager);
			using var reader = new BinaryReader(stream, DefaultEncoding);

			cloned.Read(reader, readContext);
		}

		return cloned;
	}
}