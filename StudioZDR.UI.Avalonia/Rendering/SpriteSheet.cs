using System.Collections.Concurrent;
using MercuryEngine.Data.Types.DreadTypes;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using StudioZDR.UI.Avalonia.Graphics.TegraTextures;

namespace StudioZDR.UI.Avalonia.Rendering;

internal class SpriteSheet : IDisposable
{
	private static readonly SemaphoreSlim SpriteLoadThrottler = new(4, 4);

	private readonly Lock                                       spriteSheetLock = new();
	private readonly ConcurrentDictionary<string, SpriteHolder> loadedSprites   = [];
	private readonly Func<string, SpriteHolder>                 spriteFactory;
	private readonly string                                     romfsPath;
	private readonly ILogger?                                   logger;

	private SKBitmap? spriteSheetTexture;
	private bool      disposed;

	public SpriteSheet(string name, string romfsPath, GUI__CSpriteSheet dreadSpriteSheet, ILogger? logger)
	{
		this.spriteFactory = CreateSpriteHolder;
		this.romfsPath = romfsPath;
		this.logger = logger;

		Name = name;
		DreadSpriteSheet = dreadSpriteSheet;
	}

	public event EventHandler<string>? SpriteLoaded;

	public string            Name             { get; }
	public GUI__CSpriteSheet DreadSpriteSheet { get; }

	public SKBitmap? GetOrQueueSprite(string spriteName)
	{
		ObjectDisposedException.ThrowIf(this.disposed, this);

		return this.loadedSprites.GetOrAdd(spriteName, this.spriteFactory).Bitmap;
	}

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref this.disposed, true, false))
			return;

		this.spriteSheetTexture?.Dispose();
		this.spriteSheetTexture = null;

		foreach (var sprite in this.loadedSprites.Values)
			sprite.Dispose();

		this.loadedSprites.Clear();
	}

	private SpriteHolder CreateSpriteHolder(string spriteName)
	{
		var spriteHolder = new SpriteHolder(spriteName);

		// Result of Task.Run is intentionally ignored - it will occur entirely unobserved
		// Any errors will be logged via our logger if we were provided one
		Task.Run(() => LoadSpriteAsync(spriteHolder));

		return spriteHolder;
	}

	private async Task LoadSpriteAsync(SpriteHolder spriteHolder)
	{
		try
		{
			await SpriteLoadThrottler.WaitAsync();

			if (DreadSpriteSheet.Items?.SingleOrDefault(s => s.Value?.ID == spriteHolder.Name) is not { Value: { } spriteItem })
				throw new ApplicationException($"Sprite sheet does not contain a sprite named \"{spriteHolder.Name}\"");

			var spriteSheetTexture = await GetSpriteSheetTextureAsync();
			var uvs = spriteItem.TexUVs ?? new GUI__CSpriteSheetItem__STexUV();
			var uvOffset = uvs.Offset ?? new Vector2();
			var uvSize = uvs.Scale ?? new Vector2();

			var sourceX = (int) ( uvOffset.X * spriteSheetTexture.Width );
			var sourceY = (int) ( uvOffset.Y * spriteSheetTexture.Height );
			var sourceWidth = (int) ( Math.Min(1, uvSize.X) * spriteSheetTexture.Width );
			var sourceHeight = (int) ( Math.Min(1, uvSize.Y) * spriteSheetTexture.Height );
			var sourceRect = new SKRectI(sourceX, sourceY, sourceX + sourceWidth, sourceY + sourceHeight);
			var destRect = new SKRectI(0, 0, sourceWidth, sourceHeight);

			var spriteBitmapInfo = spriteSheetTexture.Info with {
				Width = sourceRect.Width,
				Height = sourceRect.Height,
			};
			var spriteBitmap = new SKBitmap(spriteBitmapInfo);

			using (var canvas = new SKCanvas(spriteBitmap))
				canvas.DrawBitmap(spriteSheetTexture, sourceRect, destRect);

			spriteHolder.Bitmap = spriteBitmap;
			SpriteLoaded?.Invoke(this, spriteHolder.Name);
		}
		catch (Exception ex)
		{
			this.logger?.LogError(ex, "Error loading sprite \"{SpriteName}\"", spriteHolder.Name);
		}
		finally
		{
			SpriteLoadThrottler.Release();
			GC.Collect();
		}
	}

	private async Task<SKBitmap> GetSpriteSheetTextureAsync()
	{
		lock (this.spriteSheetLock)
		{
			if (this.spriteSheetTexture != null)
				return this.spriteSheetTexture;
		}

		var bctexPath = Path.Join(this.romfsPath, "textures", DreadSpriteSheet.TexturePath);
		var bctex = new Bctex();

		await using (var fileStream = File.Open(bctexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			await bctex.ReadAsync(fileStream).ConfigureAwait(false);

		var bitmap = bctex.Textures.First().ToBitmap();

		lock (this.spriteSheetLock)
			return this.spriteSheetTexture = bitmap;
	}

	private sealed record SpriteHolder(string Name) : IDisposable
	{
		public SKBitmap? Bitmap { get; set; }

		public void Dispose()
		{
			Bitmap?.Dispose();
			Bitmap = null;
		}
	}
}