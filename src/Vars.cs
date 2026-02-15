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
    internal const int FoodMatterTicks = 28;
    internal const int NitrousMatterTicks = 3;
    
    internal const int FoodPipMatterCost = 20;
    internal const int KarmaMatterCost = 1000;
    internal const int HyperspeedMatterCost = 3;
    internal const int MatterLostOnDeath = 80;

    internal static readonly HashSet<CreatureTemplate.Type> NotSwallowableCreatures = new(new []
    {
        CreatureTemplate.Type.Leech,
        CreatureTemplate.Type.Slugcat,
        CreatureTemplate.Type.TempleGuard
    });

    internal static readonly Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject>[] SynthItems = {
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.Rock), // 0
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb), // 1
        DefaultSpearSynth(false, false), // 2
        DefaultSpearSynth(true, false), // 3
        DefaultSpearSynth(false, true), // 4
        DefaultSpearSynth(3f), // 5
        DefaultSynth(AbstractPhysicalObject.AbstractObjectType.Lantern), // 6
        ConsumableSynth(AbstractPhysicalObject.AbstractObjectType.FlareBomb), // 7
        (world, coord, id) => // 8
            new DataPearl.AbstractDataPearl(world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, coord, id, -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc),
        DefaultSynth(DLCSharedEnums.AbstractObjectType.SingularityBomb) // 9
    };

    internal static PlayerKeybind ModKey;
    internal static PlayerKeybind ObjectMatterKey;
    internal static PlayerKeybind FoodMatterKey;
    internal static PlayerKeybind KarmaMatterKey;
    internal static PlayerKeybind HyperspeedKey;
}