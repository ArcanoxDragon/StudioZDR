using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using MercuryEngine.Data.Formats;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using StudioZDR.App.Configuration;
using StudioZDR.App.Rendering;

namespace StudioZDR.UI.Avalonia.Rendering;

internal class SpriteSheetManager : ISpriteSheetManager, IDisposable
{
	private const string GlobalSpriteSheetName = "global";

	private readonly ConcurrentDictionary<string, SpriteSheet> loadedSpriteSheets  = [];
	private readonly ConcurrentDictionary<string, bool>        failedSprites       = [];
	private readonly Subject<string>                           spriteLoadedSubject = new();
	private readonly Func<string, SpriteSheet>                 spriteSheetFactory;
	private readonly ApplicationSettings                       settings;
	private readonly ILogger<SpriteSheetManager>               logger;

	private bool disposed;

	public SpriteSheetManager(
		IOptionsMonitor<ApplicationSettings> settings,
		ILogger<SpriteSheetManager> logger
	)
	{
		this.spriteSheetFactory = CreateSpriteSheet;
		this.settings = settings.CurrentValue;
		this.logger = logger;
	}

	public IObservable<string> SpriteLoaded => this.spriteLoadedSubject;

	public async IAsyncEnumerable<string> GetAllSpriteSheetNamesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var bmsssBasePath = Path.Join(this.settings.RomFsLocation, "gui", "scripts");

		foreach (string filePath in Directory.EnumerateFiles(bmsssBasePath, "sprites_*.bmsss", SearchOption.AllDirectories))
		{
			var bmsss = new Bmsss();

			await using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				await bmsss.ReadAsync(fileStream, cancellationToken);

			foreach (var spriteSheetName in bmsss.SpriteSheets.Keys)
				yield return spriteSheetName;
		}
	}

	public ISpriteSheet GetSpriteSheet(string spriteSheetName)
		=> this.loadedSpriteSheets.GetOrAdd(spriteSheetName, this.spriteSheetFactory);

	public SKBitmap? GetOrQueueSprite(string spriteName)
	{
		ObjectDisposedException.ThrowIf(this.disposed, this);

		if (this.failedSprites.ContainsKey(spriteName))
			return null;

		try
		{
			var spriteNameParts = spriteName.Split(['/'], 2);
			string spriteSheetName, spriteItemName;

			if (spriteNameParts.Length >= 2)
			{
				spriteSheetName = spriteNameParts[0];
				spriteItemName = spriteNameParts[1];
			}
			else
			{
				spriteSheetName = GlobalSpriteSheetName;
				spriteItemName = spriteName;
			}

			var spriteSheet = this.loadedSpriteSheets.GetOrAdd(spriteSheetName, this.spriteSheetFactory);

			return spriteSheet.GetOrQueueSprite(spriteItemName);
		}
		catch (Exception ex)
		{
			this.failedSprites.TryAdd(spriteName, true);
			this.logger.LogError(ex, "Error loading sprite \"{SpriteName}\"", spriteName);
			return null;
		}
	}

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref this.disposed, true, false))
			return;

		foreach (var spriteSheet in this.loadedSpriteSheets.Values)
		{
			spriteSheet.SpriteLoaded -= OnSpriteSheetSpriteLoaded;
			spriteSheet.Dispose();
		}

		this.loadedSpriteSheets.Clear();
		this.spriteLoadedSubject.Dispose();
	}

	private SpriteSheet CreateSpriteSheet(string name)
	{
		var bmsssPath = Path.Join(this.settings.RomFsLocation, "gui", "scripts", $"sprites_{name.ToLower()}.bmsss");

		if (!File.Exists(bmsssPath))
			throw new ApplicationException($"A sprite sheet named \"{name}\" could not be found in RomFS");

		var bmsss = new Bmsss();

		using (var fileStream = File.Open(bmsssPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			bmsss.Read(fileStream);

		if (!bmsss.SpriteSheets.TryGetValue(name, out var dreadSpriteSheet))
			throw new ApplicationException($"Sprite sheet file \"sprites_{name}.bmsss\" does not contain a sprite sheet named \"{name}\"");

		var spriteSheet = new SpriteSheet(name, this.settings.RomFsLocation!, dreadSpriteSheet, this.logger);

		spriteSheet.SpriteLoaded += OnSpriteSheetSpriteLoaded;

		return spriteSheet;
	}

	private void OnSpriteSheetSpriteLoaded(object? sender, string spriteName)
	{
		if (sender is not SpriteSheet spriteSheet)
			return;

		this.spriteLoadedSubject.OnNext($"{spriteSheet.Name}/{spriteName}");
	}
}