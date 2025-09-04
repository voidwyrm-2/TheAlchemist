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

    internal const int ObjectToMatterTicks = 28;
    internal const int FoodToMatterTicks = 28;
    internal const int NitrousMatterTicks = 3;
    
    internal const int FoodPipMatterCost = 20;
    internal const int NitrousMatterCost = 1;
    internal const int MatterLostOnDeath = 80;

    internal static readonly HashSet<CreatureTemplate.Type> NotSwallowableCreatures = new(new []
    {
        CreatureTemplate.Type.Leech,
        CreatureTemplate.Type.Slugcat,
        CreatureTemplate.Type.TempleGuard
    });

    internal static readonly Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject>[] SynthItems = {
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.Rock),
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb),
        DefaultSpearSynth(false, false),
        DefaultSpearSynth(true, false),
        DefaultSpearSynth(false, true),
        DefaultSpearSynth(3f),
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.Lantern),
        //(world, coord, id) =>
        //    new DataPearl.AbstractDataPearl(world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, coord, id, 0, 0, null, DataPearl.AbstractDataPearl.DataPearlType.Misc),
        DefaultSynth(DLCSharedEnums.AbstractObjectType.SingularityBomb)
    };
        
    internal static PlayerKeybind ConvertToMatterKey;
    internal static PlayerKeybind ConvertMatterToFoodKey;
    internal static PlayerKeybind SynthesisKey;
    internal static PlayerKeybind NitrousKey;
}