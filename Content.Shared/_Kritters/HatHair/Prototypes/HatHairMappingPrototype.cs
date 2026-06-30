using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared._Kritters.HatHair.Prototypes;

[Prototype("HatHairMapping")]
public sealed partial class HatHairMappingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<MarkingPrototype> SourceHair { get; private set; }

    [DataField]
    public ProtoId<MarkingPrototype>? ReplacementHair { get; private set; }
}
