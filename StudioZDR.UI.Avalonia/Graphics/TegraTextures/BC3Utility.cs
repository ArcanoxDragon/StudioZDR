using SkiaSharp;

namespace StudioZDR.UI.Avalonia.Graphics.TegraTextures;

internal static class BC3Utility
{
	public static unsafe SKBitmap DecodeBC3(byte[] data, int width, int height)
	{
		var xTiles = ( width + 3 ) / 4;  // Integer ceil
		var yTiles = ( height + 3 ) / 4; // Integer ceil
		var bitmapWidth = xTiles * 4;
		var bitmapHeight = yTiles * 4;
		var bitmap = new SKBitmap(new SKImageInfo(bitmapWidth, bitmapHeight, SKColorType.Rgba8888));
		var pixelsPtr = (uint*) bitmap.GetPixels().ToPointer();
		var lut = new SKColor[4];
		Span<byte> tileBuffer = new byte[64];
		Span<byte> alphaBuffer = stackalloc byte[8];
		var thisPixel = stackalloc byte[4];

		for (var y = 0; y < yTiles; y++)
		for (var x = 0; x < xTiles; x++)
		{
			var inputDataIndex = 16 * ( y * xTiles + x );

			alphaBuffer[0] = data[inputDataIndex];
			alphaBuffer[1] = data[inputDataIndex + 1];

			BC3Utility.CalculateAlpha(alphaBuffer);

			var alphaLow = BC3Utility.GetInt32(data, inputDataIndex + 2);
			var alphaHigh = BC3Utility.GetInt16(data, inputDataIndex + 6);
			var alphaChannel = (uint) alphaLow | (ulong) alphaHigh << 32;

			DecodeTile(tileBuffer, inputDataIndex + 8);

			var tileIndex = 0;

			for (var tY = 0; tY < 4; tY++)
			for (var tX = 0; tX < 4; tX++)
			{
				var outputIndex = ( x * 4 + tX + ( y * 4 + tY ) * xTiles * 4 );
				var thisAlpha = alphaBuffer[(int) ( alphaChannel >> tY * 12 + tX * 3 & 7 )];

				thisPixel[2] = tileBuffer[tileIndex];
				thisPixel[1] = tileBuffer[tileIndex + 1];
				thisPixel[0] = tileBuffer[tileIndex + 2];
				thisPixel[3] = thisAlpha;
				pixelsPtr[outputIndex] = *( (uint*) thisPixel );

				tileIndex += 4;
			}
		}

		return bitmap;

		void DecodeTile(Span<byte> buffer, int offset)
		{
			var c0 = GetInt16(data, offset);
			var c1 = GetInt16(data, offset + 2);

			// Decode LUT
			lut[0] = DecodeRGB565(c0);
			lut[1] = DecodeRGB565(c1);
			lut[2] = CalculateLUT(lut[0], lut[1]);
			lut[3] = CalculateLUT(lut[1], lut[0]);

			// Decode tile data
			var indices = GetInt32(data, offset + 4);
			var lutIndexShift = 0;
			var outputIndex = 0;

			for (var y = 0; y < 4; y++)
			for (var x = 0; x < 4; x++)
			{
				var lutIndex = indices >> lutIndexShift & 3;

				lutIndexShift += 2;

				var lutColor = lut[lutIndex];

				buffer[outputIndex++] = lutColor.Blue;
				buffer[outputIndex++] = lutColor.Green;
				buffer[outputIndex++] = lutColor.Red;
				buffer[outputIndex++] = lutColor.Alpha;
			}
		}
	}

	private static SKColor DecodeRGB565(int value)
	{
		var b = ( value & 0x1F ) << 3;
		var g = ( value >> 5 & 0x3F ) << 2;
		var r = ( value >> 11 & 0x1F ) << 3;

		return new SKColor(
			(byte) ( r | r >> 5 ),
			(byte) ( g | g >> 6 ),
			(byte) ( b | b >> 5 )
		);
	}

	private static SKColor CalculateLUT(SKColor lut0, SKColor lut1)
		=> new(
			(byte) ( ( 2 * lut0.Red + lut1.Red ) / 3 ),
			(byte) ( ( 2 * lut0.Green + lut1.Green ) / 3 ),
			(byte) ( ( 2 * lut0.Blue + lut1.Blue ) / 3 )
		);

	private static void CalculateAlpha(Span<byte> data)
	{
		var first = data[0];
		var second = data[1];

		if (first > second)
		{
			for (var i = 2; i < 8; i++)
				data[i] = (byte) ( first + ( second - first ) * ( i - 1 ) / 7 );
		}
		else
		{
			for (var i = 2; i < 6; i++)
				data[i] = (byte) ( first + ( second - first ) * ( i - 1 ) / 5 );

			data[6] = 0;
			data[7] = 255;
		}
	}

	private static int GetInt16(Span<byte> data, int offset)
		=> data[offset] | data[offset + 1] << 8;

	private static int GetInt32(Span<byte> data, int offset)
		=> data[offset] |
		   data[offset + 1] << 8 |
		   data[offset + 2] << 16 |
		   data[offset + 3] << 24;
}