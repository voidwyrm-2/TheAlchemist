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

    internal static string Format(this AbstractPhysicalObject obj) => obj switch
    {
        AbstractSpear spear => $"(Spear, explosive? {spear.explosive}, electric? {spear.electric})",
        AbstractCreature crit => $"(Creature, template.type? {crit.creatureTemplate.type}, meatLeft? {crit.state.meatLeft}, dead? {crit.state.dead}), swallowable? {crit.IsSwallowable()}",
        _ => $"(Object, type? {obj.type})"
    };

    internal static bool IsSwallowable(this AbstractCreature creature) =>
        SwallowableCreatures.Contains(creature.creatureTemplate.type);
    
    internal static int GetMatterValueForObject(AbstractPhysicalObject obj)
    {
        return obj switch
        {
            AbstractSpear spear => GetMatterValueForSpear(spear),
            AbstractCreature crit => crit.creatureTemplate.meatPoints * FoodPipMatterCost,
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