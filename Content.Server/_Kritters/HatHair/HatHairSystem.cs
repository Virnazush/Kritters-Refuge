using Content.Server._Kritters.HatHair.Components;
using Content.Shared._Kritters.HatHair.Prototypes;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Server._Kritters.HatHair;

public sealed class HatHairSystem : EntitySystem
{
    [Dependency] private readonly MarkingManager _markings = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ClothingComponent, ClothingGotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<ClothingComponent, ClothingGotUnequippedEvent>(OnClothingUnequipped);
    }

    private void OnClothingEquipped(Entity<ClothingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (args.Clothing.InSlotFlag != SlotFlags.HEAD
            || HasComp<HatHairSwapComponent>(args.Wearer)
            || !TryComp<HumanoidAppearanceComponent>(args.Wearer, out var humanoid)
            || !humanoid.MarkingSet.TryGetCategory(MarkingCategories.Hair, out var originalHair)
            || originalHair.Count == 0)
        {
            return;
        }

        var sourceHair = originalHair[0];
        if (!TryResolveReplacement(sourceHair.MarkingId, out var replacementId))
            return;

        var swap = EnsureComp<HatHairSwapComponent>(args.Wearer);
        swap.SourceHeadwear = ent.Owner;
        swap.OriginalHair = CloneMarkings(originalHair);

        if (replacementId == null)
        {
            humanoid.MarkingSet.RemoveCategory(MarkingCategories.Hair);
            Dirty(args.Wearer, humanoid);
            return;
        }

        if (!_proto.TryIndex<MarkingPrototype>(replacementId.Value, out var replacementProto)
            || replacementProto.MarkingCategory != MarkingCategories.Hair
            || !_markings.CanBeApplied(humanoid.Species, humanoid.Sex, replacementProto, _proto))
        {
            Log.Warning($"Kritters: Invalid or missing hat hair replacement for {sourceHair.MarkingId} on {ToPrettyString(args.Wearer)}.");
            RemComp<HatHairSwapComponent>(args.Wearer);
            return;
        }

        var replacement = replacementProto.AsMarking();
        for (var i = 0; i < replacement.MarkingColors.Count && i < sourceHair.MarkingColors.Count; i++)
        {
            replacement.SetColor(i, sourceHair.MarkingColors[i]);
        }

        humanoid.MarkingSet.RemoveCategory(MarkingCategories.Hair);
        humanoid.MarkingSet.AddBack(MarkingCategories.Hair, replacement);
        Dirty(args.Wearer, humanoid);
    }

    private void OnClothingUnequipped(Entity<ClothingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!TryComp<HatHairSwapComponent>(args.Wearer, out var swap)
            || swap.SourceHeadwear != ent.Owner
            || !TryComp<HumanoidAppearanceComponent>(args.Wearer, out var humanoid))
        {
            return;
        }

        humanoid.MarkingSet.RemoveCategory(MarkingCategories.Hair);
        foreach (var marking in swap.OriginalHair)
        {
            humanoid.MarkingSet.AddBack(MarkingCategories.Hair, new Marking(marking));
        }

        RemComp<HatHairSwapComponent>(args.Wearer);
        Dirty(args.Wearer, humanoid);
    }

    private bool TryResolveReplacement(string sourceHair, out ProtoId<MarkingPrototype>? replacement)
    {
        foreach (var mapping in _proto.EnumeratePrototypes<HatHairMappingPrototype>())
        {
            if (mapping.SourceHair.Id == sourceHair)
            {
                replacement = mapping.ReplacementHair;
                return true;
            }
        }

        foreach (var group in _proto.EnumeratePrototypes<HatHairGroupPrototype>())
        {
            foreach (var hair in group.SourceHairs)
            {
                if (hair.Id != sourceHair)
                    continue;

                replacement = group.ReplacementHair;
                return true;
            }
        }

        replacement = default;
        return false;
    }

    private static List<Marking> CloneMarkings(IReadOnlyList<Marking> markings)
    {
        var cloned = new List<Marking>(markings.Count);
        foreach (var marking in markings)
        {
            cloned.Add(new Marking(marking));
        }

        return cloned;
    }
}
