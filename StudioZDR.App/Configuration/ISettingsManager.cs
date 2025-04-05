namespace StudioZDR.App.Configuration;

public interface ISettingsManager
{
	void Modify(Action<ApplicationSettings> modifyAction);
	Task ModifyAsync(Action<ApplicationSettings> modifyAction);
}