using ImprovedInput;
using MoreSlugcats;
using UnityEngine;

namespace TheAlchemist;

using static Vars;

public static class Utils
{
    internal static PlayerKeybind RegisterKeybind(string id, string name, KeyCode kbp, KeyCode gpp) =>
        PlayerKeybind.Register("nuclear.Alchemist:" + id, "The Alchemist", name, kbp, gpp);
    
    internal static bool IsAlchem(this Player self) => self.SlugCatClass == Alchem;
    
    internal static AlchemistInfo GetInfo(this Player self) =>
        Alchemists.GetValue(self, _ => new AlchemistInfo());
    
    internal static int GetMatterValueForItem(this AbstractPhysicalObject.AbstractObjectType self)
    {
        if (self == AbstractPhysicalObject.AbstractObjectType.Rock)
            return 10;
        if (self == AbstractPhysicalObject.AbstractObjectType.Spear)
            return 40;
        if (self == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            return 100;
        if (self == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            return 200;
        if (self == DLCSharedEnums.AbstractObjectType.SingularityBomb)
            return 600;
        return 0;
    }
}