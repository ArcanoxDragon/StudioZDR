using JetBrains.Annotations;
using StudioZDR.App.Features.GuiEditor.ViewModels;

namespace StudioZDR.App.Features.GuiEditor;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal class GuiEditorFeature : FeatureModule<GuiEditorViewModel>
{
	public override string Name         => "GUI Editor";
	public override string Description  => "Edit Metroid Dread GUI composition and skin files (BMSCP and BMSSK files)";
	public override int    DisplayOrder => 2;

	public override string IconKey => "fa-solid fa-rectangles-mixed";

	protected override bool RequiresRomFs => true;
}