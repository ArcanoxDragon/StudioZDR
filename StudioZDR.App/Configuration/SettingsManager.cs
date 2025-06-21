using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using StudioZDR.App.Features.GuiEditor.Configuration;
using StudioZDR.App.Utility;

namespace StudioZDR.App.Configuration;

internal class SettingsManager(ILogger<SettingsManager> logger) : IConfigureOptions<ApplicationSettings>, IOptionsChangeTokenSource<ApplicationSettings>, ISettingsManager
{
	private const string SettingsFileName = "settings.json";

	private ApplicationSettings settings    = new();
	private SettingsChangeToken changeToken = new();
	private bool                isLoaded;

	public string Name => Options.DefaultName;

	public void Configure(ApplicationSettings options)
	{
		EnsureLoaded();

		// Copy all settings from our private instance to the options instance
		this.settings.CopyTo(options);
	}

	public void Modify(Action<ApplicationSettings> modifyAction)
	{
		EnsureLoaded();
		modifyAction(this.settings);
		Save();
		OnChanged();
	}

	public async Task ModifyAsync(Action<ApplicationSettings> modifyAction)
	{
		await EnsureLoadedAsync();
		modifyAction(this.settings);
		await SaveAsync();
		OnChanged();
	}

	public IChangeToken GetChangeToken()
		=> this.changeToken;

	private void EnsureLoaded()
	{
		if (this.isLoaded)
			return;

		var settingsFilePath = GetSettingsPath();

		if (File.Exists(settingsFilePath))
		{
			try
			{
				using var fileStream = File.Open(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				var settings = (ApplicationSettings?) JsonSerializer.Deserialize(fileStream, typeof(ApplicationSettings), SettingsJsonContext.Default);

				this.settings = settings ?? new ApplicationSettings();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to load application settings from \"{FilePath}\"", settingsFilePath);
			}
		}

		this.isLoaded = true;
	}

	private async ValueTask EnsureLoadedAsync()
	{
		if (this.isLoaded)
			return;

		var settingsFilePath = GetSettingsPath();

		if (File.Exists(settingsFilePath))
		{
			try
			{
				await using var fileStream = File.Open(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				var settings = (ApplicationSettings?) await JsonSerializer.DeserializeAsync(fileStream, typeof(ApplicationSettings), SettingsJsonContext.Default);

				this.settings = settings ?? new ApplicationSettings();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to load application settings from \"{FilePath}\"", settingsFilePath);
			}
		}

		this.isLoaded = true;
	}

	private void Save()
	{
		var settingsFilePath = GetSettingsPath(create: true);

		try
		{
			using var fileStream = File.Open(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);

			JsonSerializer.Serialize(fileStream, this.settings, typeof(ApplicationSettings), SettingsJsonContext.Default);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to save application settings to \"{FilePath}\"", settingsFilePath);
		}
	}

	private async ValueTask SaveAsync()
	{
		var settingsFilePath = GetSettingsPath(create: true);

		try
		{
			await using var fileStream = File.Open(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);

			await JsonSerializer.SerializeAsync(fileStream, this.settings, typeof(ApplicationSettings), SettingsJsonContext.Default);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to save application settings to \"{FilePath}\"", settingsFilePath);
		}
	}

	private void OnChanged()
	{
		var previousChangeToken = Interlocked.Exchange(ref this.changeToken, new SettingsChangeToken());
		previousChangeToken.NotifyOfChange();
	}

	private static string GetSettingsPath(bool create = false)
	{
		var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var ourAppDataFolder = Path.Join(appDataFolder, ZdrConstants.Application.AppDataFolderName);

		if (create)
			Directory.CreateDirectory(ourAppDataFolder);

		return Path.Join(ourAppDataFolder, SettingsFileName);
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ApplicationSettings))]
[JsonSerializable(typeof(GuiEditorSettings))]
internal partial class SettingsJsonContext : JsonSerializerContext;