﻿using SkiaSharp;
using StudioZDR.TegraTextureLib.Extensions;
using BaseMedTestFixture = MercuryEngine.Data.Tests.Infrastructure.BaseTestFixture;

namespace StudioZDR.TegraTextureLib.Tests;

public class BctexTests : BaseMedTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bctex", Path.Join("textures", "gui", "textures")))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(BctexTests.GetTestFiles)), Parallelizable]
	public async Task TestLoadBctexAsync(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BCTEX file: {0}", inFile);

		await using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bctex = new Bctex();

		try
		{
			await bctex.ReadAsync(fileStream);

			await BctexTests.ConvertAndSaveTexturesAsync(bctex, inFile, relativeTo);
		}
		catch (Exception ex)
		{
			await TestContext.Error.WriteLineAsync("Error converting texture:");
			TestContext.Error.WriteLine(ex);
			throw;
		}
	}

	private static async Task ConvertAndSaveTexturesAsync(Bctex bctex, string sourceFilePath, string relativeTo)
	{
		var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileDir = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", "BCTEX", relativePath);

		Directory.CreateDirectory(outFileDir);

		foreach (var (i, texture) in bctex.Textures.Pairs())
		{
			using var bitmap = texture.ToBitmap();

			var outFileNameSuffix = bctex.Textures.Count > 1 ? $".{i}.png" : ".png";
			var outFileName = ( bctex.TextureName ?? sourceFileName ) + outFileNameSuffix;
			var outFilePath = Path.Join(outFileDir, outFileName);
			await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

			bitmap.Encode(outFileStream, SKEncodedImageFormat.Png, 100);
		}
	}

	private static SKColorType GetColorType(XtxImageFormat format)
		=> format switch {
			XtxImageFormat.NvnFormatRGBA8 => SKColorType.Rgba8888,
			// XtxImageFormat.DXT5           => SKColorType.Alpha8,
			_ => SKColorType.Unknown,
		};
}