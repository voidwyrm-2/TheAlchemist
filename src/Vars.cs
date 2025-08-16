using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImprovedInput;

namespace TheAlchemist;

internal static class Vars
{
    internal static readonly ConditionalWeakTable<Player, AlchemistInfo> Alchemists = new();
    internal static readonly Dictionary<int, AlchemistInfo> InfoMap = new();
    
    internal static SlugcatStats.Name Alchem = new("nuclear.Alchemist");

    internal const int FoodPipMatterCost = 20;

    internal static HashSet<CreatureTemplate.Type> SwallowableCreatures = new(new []
    {
        CreatureTemplate.Type.Fly,
        CreatureTemplate.Type.GreenLizard,
        CreatureTemplate.Type.PinkLizard,
        CreatureTemplate.Type.RedLizard,
    });
        
    internal static PlayerKeybind ConvertToMatterKey;
    internal static PlayerKeybind ConvertMatterToFoodKey;
    internal static PlayerKeybind SynthesisKey;
}