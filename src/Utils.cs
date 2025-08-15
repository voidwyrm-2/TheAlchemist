using ImprovedInput;
using UnityEngine;

namespace TheAlchemist;

using static Vars;

public static class Utils
{
    internal static PlayerKeybind RegisterKeybind(string id, string name, KeyCode kbp, KeyCode gpp) =>
        PlayerKeybind.Register("nuclear.Alchemist:" + id, "The Alchemist", name, kbp, gpp);
    
    internal static bool IsAlchem(this Player self) => self.SlugCatClass == Alchem;
    
    internal static AlchemistInfo GetInfo(this Player self) =>
        Alchemists.GetValue(self, _ => new AlchemistInfo(self));
    
    internal static int GetMatterValueForItem(this AbstractPhysicalObject self)
    {
        var type = self.type;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.Rock)
            return 5;

        if (type == AbstractPhysicalObject.AbstractObjectType.Spear)
        {
            var spear = (self as AbstractSpear)!;

            if (spear.explosive)
                return 50;

            if (spear.electric)
                return 80;
            
            return 30;
        }
                
        if (type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            return 70;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            return 100;
        
        if (type == DLCSharedEnums.AbstractObjectType.SingularityBomb)
            return 800;
        
        return 0;
    }
}