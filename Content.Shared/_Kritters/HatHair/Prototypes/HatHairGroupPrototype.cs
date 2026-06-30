using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared._Kritters.HatHair.Prototypes;

[Prototype("HatHairGroup")]
public sealed partial class HatHairGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public List<ProtoId<MarkingPrototype>> SourceHairs { get; private set; } = new();

    [DataField]
    public ProtoId<MarkingPrototype>? ReplacementHair { get; private set; }
}
