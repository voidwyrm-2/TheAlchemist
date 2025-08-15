using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImprovedInput;
using UnityEngine;

namespace TheAlchemist;

internal static class Vars
{
    internal static readonly ConditionalWeakTable<Player, AlchemistInfo> Alchemists = new();
    internal static readonly List<AlchemistInfo> InfoList = new();
    
    internal static readonly List<FLabel> MatterLabels = new();
    
    internal static SlugcatStats.Name Alchem = new("nuclear.Alchemist");
        
    internal static PlayerKeybind EatItemInStomachKey;
    
    
}