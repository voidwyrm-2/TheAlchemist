using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImprovedInput;

namespace TheAlchemist;

using static Utils;

internal static class Vars
{
    internal static readonly ConditionalWeakTable<Player, AlchemistInfo> Alchemists = new();
    internal static readonly Dictionary<int, AlchemistInfo> InfoMap = new();
    
    internal static SlugcatStats.Name Alchem = new("nuclear.Alchemist");

    internal const int FoodPipMatterCost = 20;

    internal const int MatterLostOnDeath = 80;

    internal static readonly HashSet<CreatureTemplate.Type> NotSwallowableCreatures = new(new []
    {
        CreatureTemplate.Type.Leech,
        CreatureTemplate.Type.Slugcat,
        CreatureTemplate.Type.TempleGuard
    });

    internal static readonly Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject>[] SynthItems = new[]
    {
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.Rock),
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb),
        DefaultSpearSynth(false, false),
        DefaultSpearSynth(true, false),
        DefaultSpearSynth(false, true),
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.Lantern),
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.DataPearl),
        DefaultSynth(DLCSharedEnums.AbstractObjectType.SingularityBomb)
    };
        
    internal static PlayerKeybind ConvertToMatterKey;
    internal static PlayerKeybind ConvertMatterToFoodKey;
    internal static PlayerKeybind SynthesisKey;
}