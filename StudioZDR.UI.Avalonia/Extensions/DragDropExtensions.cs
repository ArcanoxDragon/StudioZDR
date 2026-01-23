using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Logging;

// ReSharper disable ConvertToExtensionBlock (crashes C# compiler on .NET 9)

namespace StudioZDR.UI.Avalonia.Extensions;

internal static class DragDropExtensions
{
	public static void SetupDragDrop(this Control control, Func<Visual, bool> draggablePredicate,
									 Action<Visual, DataTransfer> configureData,
									 EventHandler<DragEventArgs>? onDragOver,
									 EventHandler<DragEventArgs>? onDrop,
									 DragDropEffects allowedEffects,
									 double minDragDistance = 5.0)
	{
		var dataFactory = (Visual draggedElement) => {
			var dataTransfer = new DataTransfer();

			configureData(draggedElement, dataTransfer);

			return dataTransfer;
		};

		control.SetupDragDrop(draggablePredicate, dataFactory, onDragOver, onDrop, allowedEffects, minDragDistance);
	}

	public static void SetupDragDrop(this Control control, Func<Visual, bool> draggablePredicate,
									 Func<Visual, IDataTransfer?> dataFactory,
									 EventHandler<DragEventArgs>? onDragOver,
									 EventHandler<DragEventArgs>? onDrop,
									 DragDropEffects allowedEffects,
									 double minDragDistance = 5.0)
	{
		// Drag-in-progress handler
		control.AddHandler(DragDrop.DragOverEvent, onDragOver);

		// Drop completed handler
		control.AddHandler(DragDrop.DropEvent, onDrop);

		// Main drag handlers (starts the drag operation if the pointer is moved at least
		// a certain amount after being pressed on a valid target)
		var canStartDrag = false;
		var downPosition = new Point(-1, -1);

		control.AddHandler(InputElement.PointerPressedEvent, void (_, e) => {
			canStartDrag = false;

			if (!e.Properties.IsLeftButtonPressed)
				return;
			if (e.Source is not Visual sourceElement)
				return;
			if (!draggablePredicate(sourceElement))
				return;

			downPosition = e.GetPosition(null);
			canStartDrag = true;
		}, handledEventsToo: true);

		control.AddHandler(InputElement.PointerReleasedEvent, void (_, _) => {
			canStartDrag = false;
		}, handledEventsToo: true);

		control.AddHandler(InputElement.PointerCaptureLostEvent, void (_, _) => {
			canStartDrag = false;
		}, handledEventsToo: true);

		control.AddHandler(InputElement.PointerMovedEvent, async void (_, e) => {
			try
			{
				if (!canStartDrag)
					return;

				var moveDistance = Point.Distance(downPosition, e.GetPosition(null));

				if (moveDistance < minDragDistance)
					return;

				canStartDrag = false; // Prevent repeated drag starts
				await DragDropExtensions.PerformDragAsync(dataFactory, e, allowedEffects);
			}
			catch (Exception ex)
			{
				var logger = Logger.TryGet(LogEventLevel.Error, nameof(DragDropExtensions));

				logger.GetValueOrDefault().Log(e.Source, "Error in DragDrop operation: {Exception}", ex);
			}
		}, handledEventsToo: true);
	}

	private static async Task PerformDragAsync(Func<Visual, IDataTransfer?> dataFactory, PointerEventArgs e, DragDropEffects allowedEffects)
	{
		// Type and nullability are asserted above prior to calling this method
		var dragData = dataFactory((Visual) e.Source!);

		if (dragData is null)
			// Drag canceled due to null data
			return;

		await DragDrop.DoDragDropAsync(e, dragData, allowedEffects);
	}
}