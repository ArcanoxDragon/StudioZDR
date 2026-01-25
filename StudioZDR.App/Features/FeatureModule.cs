using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using StudioZDR.App.Configuration;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features;

public abstract class FeatureModule : Module, IFeature, INotifyPropertyChanged
{
	private IOptionsMonitor<ApplicationSettings>? appSettingsMonitor;
	private IDisposable?                          appSettingMonitorSubscription;

	public abstract string Name         { get; }
	public abstract string Description  { get; }
	public abstract int    DisplayOrder { get; }

	public abstract Type ViewModelType
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		get;
	}

	public virtual string IconKey => "fa-solid fa-block";

	public virtual bool IsAvailable => !RequiresRomFs || HasValidRomFs;

	public virtual string? UnavailableMessage
	{
		get
		{
			if (RequiresRomFs && !HasValidRomFs)
				return "A valid RomFS location has not been set";

			return null;
		}
	}

	protected virtual bool RequiresRomFs => false;

	protected bool HasValidRomFs
	{
		get
		{
			if (this.appSettingsMonitor is not { CurrentValue: var settings })
				return false;

			if (string.IsNullOrEmpty(settings.RomFsLocation))
				return false;

			return Directory.Exists(settings.RomFsLocation);
		}
	}

	protected sealed override void Load(ContainerBuilder builder)
	{
		builder.RegisterBuildCallback(OnContainerBuilt);

		ConfigureServices(builder);

		// Add view model type as a service
		builder.RegisterType(ViewModelType);

		// Add self as a service after all other services for this feature
		builder.RegisterInstance(this).As<IFeature>();
	}

	protected virtual void ConfigureServices(ContainerBuilder builder) { }

	protected virtual void NotifyWhenSettingsChanged()
	{
		NotifyPropertyChanged(nameof(HasValidRomFs));

		if (RequiresRomFs)
		{
			NotifyPropertyChanged(nameof(IsAvailable));
			NotifyPropertyChanged(nameof(UnavailableMessage));
		}
	}

	private void OnContainerBuilt(ILifetimeScope scope)
	{
		this.appSettingMonitorSubscription?.Dispose();
		this.appSettingsMonitor = scope.Resolve<IOptionsMonitor<ApplicationSettings>>();
		this.appSettingMonitorSubscription = this.appSettingsMonitor.OnChange(OnAppSettingsChanged);
	}

	private void OnAppSettingsChanged(ApplicationSettings settings)
		=> NotifyWhenSettingsChanged();

	#region INotifyPropertyChanged

	public event PropertyChangedEventHandler? PropertyChanged;

	private void NotifyPropertyChanged([CallerMemberName] string? callerMemberName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerMemberName));

	#endregion
}

public abstract class FeatureModule<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TViewModel
> : FeatureModule
where TViewModel : ViewModelBase
{
	public override Type ViewModelType
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		get => typeof(TViewModel);
	}
}