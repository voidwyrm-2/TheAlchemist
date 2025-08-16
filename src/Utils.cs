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

    internal static bool IsSwallowable(this AbstractCreature creature) =>
        SwallowableCreatures.Contains(creature.creatureTemplate.TopAncestor().type);
    
    internal static int GetMatterValueForObject(AbstractPhysicalObject obj)
    {
        return obj switch
        {
            AbstractSpear spear => GetMatterValueForSpear(spear),
            AbstractCreature creature => GetMatterValueForCreature(creature.creatureTemplate.TopAncestor().type),
            _ => GetMatterValueForType(obj.type)
        };
    }

    private static int GetMatterValueForSpear(AbstractSpear spear)
    {
        if (spear.explosive)
            return 50;

        if (spear.electric)
            return 60;
            
        return 20;
    }

    private static int GetMatterValueForCreature(CreatureTemplate.Type type)
    {
        if (type == CreatureTemplate.Type.Fly)
            return 7;
        
        if (type == CreatureTemplate.Type.GreenLizard)
            return 30;
        
        if (type == CreatureTemplate.Type.PinkLizard)
            return 40;
        
        if (type == CreatureTemplate.Type.RedLizard)
            return 100;

        return 0;
    }

    private static int GetMatterValueForType(AbstractPhysicalObject.AbstractObjectType type)
    {
        if (type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
            return 7;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.Rock)
            return 4;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
            return 30;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
            return 10;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.Lantern)
            return 15;
                
        if (type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            return 40;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            return 80;
        
        if (type == DLCSharedEnums.AbstractObjectType.SingularityBomb)
            return 600;
        
        return 0;
    }
}