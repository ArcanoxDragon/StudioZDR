using JetBrains.Annotations;
using StudioZDR.App.Features.SaveEditor.ViewModels;

namespace StudioZDR.App.Features.SaveEditor;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class SaveEditorFeature : FeatureModule<SaveEditorViewModel>
{
	public override string Name         => "Save Editor";
	public override string Description  => "Edit Metroid Dread profile/save files (BMSSV files)";
	public override int    DisplayOrder => 1;

	public override string IconKey => "fa-solid fa-floppy-disk-pen";
}