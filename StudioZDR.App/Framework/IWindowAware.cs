namespace StudioZDR.App.Framework;

public interface IWindowAware
{
	IWindow? ParentWindow { get; set; }
}