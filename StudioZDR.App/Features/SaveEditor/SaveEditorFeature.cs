using Material.Icons;
using StudioZDR.App.Features.SaveEditor.ViewModels;

namespace StudioZDR.App.Features.SaveEditor;

public class SaveEditorFeature : FeatureModule<SaveEditorViewModel>
{
	public override string Name        => "Save Editor";
	public override string Description => "Edit Metroid Dread profile/save files (BMSSV files)";

	public override MaterialIconKind IconKind => MaterialIconKind.ContentSaveEdit;
}