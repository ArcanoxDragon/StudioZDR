using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudioZDR.App.Framework;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaDialogs : IDialogs
{
	private readonly Func<IEnumerable<WindowContext?>> windowContextAccessor;

	public AvaloniaDialogs(Func<IEnumerable<WindowContext?>> windowContextAccessor)
	{
		this.windowContextAccessor = windowContextAccessor;
	}

	private WindowContext? WindowContext => this.windowContextAccessor().FirstOrDefault();

	public async Task<bool> ConfirmAsync(string title, string message)
	{
		// TODO:
		return true;
	}
}