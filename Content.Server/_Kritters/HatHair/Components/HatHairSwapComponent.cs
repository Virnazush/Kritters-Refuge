using Content.Shared.Humanoid.Markings;

namespace Content.Server._Kritters.HatHair.Components;

[RegisterComponent]
public sealed partial class HatHairSwapComponent : Component
{
    [DataField]
    public EntityUid SourceHeadwear;

    [DataField]
    public List<Marking> OriginalHair = new();
}
