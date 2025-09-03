using System;
using ImprovedInput;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheAlchemist;

using static Vars;

public static class Utils
{
    internal static PlayerKeybind RegisterKeybind(string id, string name, KeyCode kbp, KeyCode gpp) =>
        PlayerKeybind.Register("nuclear.Alchemist:" + id, "The Alchemist", name, kbp, gpp);
    
    internal static bool IsAlchem(this Player self) => self.SlugCatClass == Alchem;
    
    internal static bool TryGetInfo(this Player self, out AlchemistInfo info) =>
        Alchemists.TryGetValue(self, out info);
    
    internal static AlchemistInfo GetInfo(this Player self) =>
        Alchemists.TryGetValue(self, out var info) ? info : null;

    internal static int OccupiedHand(this Player self)
    {
        for (var i = 0; i < self.grasps.Length; i++)
        {
            if (self.grasps[i] != null)
                return i;
        }

        return -1;
    }

    internal static void SpawnObjectToMatterEffect(this Player self, Vector2 pos)
    {
        var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);
        
        self.room.AddObject(new AlchemistPingCircle(self, pos, 0f, 6f, 0f, 65, true, color)
		{
			maxThickness = 7f,
			radDamping = 0.03f,
			maxAlpha = 0.6f
		});
        
        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk, false, 0.75f, 0.5f + Random.value * 0.5f);
    }
    
    internal static void SpawnMatterToObjectEffect(this Player self, Vector2 pos)
    {
        var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);
        
        self.room.AddObject(new AlchemistPingCircle(self, pos, 0f, 6f, 0f, 65, true, color)
        {
            maxThickness = 7f,
            radDamping = 0.03f,
            maxAlpha = 0.6f
        });
        
        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk, false, 0.75f, 0.5f + Random.value * 0.5f);
        self.room.PlaySound(SoundID.Zapper_Zap, self.mainBodyChunk, false, 0.75f, 0.5f + Random.value * 0.2f);
    }
    
    internal static void SpawnFoodToMatterEffect(this Player self, Vector2 pos)
    {
        var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);

        var vx = 0.1f + Random.value * 0.8f;
        var vy = 0.5f + Random.value * 0.3f;
        var lifetime = (int)(1f + Random.value * 1.3f);
        self.room.AddObject(new Spark(pos, new Vector2(vx, vy), color, null, lifetime, lifetime + 4));
        
        self.room.PlaySound(SoundID.Snail_Pop, self.mainBodyChunk, false, 0.75f, 1.5f + Random.value);
    }

    internal static Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject> DefaultSynth(AbstractPhysicalObject.AbstractObjectType type)
    {
        return (world, pos, id) => new AbstractPhysicalObject(world, type, null, pos, id);;
    }
    
    internal static Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject> MscSynth(AbstractPhysicalObject.AbstractObjectType type)
    {
        return (world, pos, id) => new AbstractPhysicalObject(world, type, null, pos, id);;
    }
    
    internal static Func<World, WorldCoordinate, EntityID, AbstractPhysicalObject> DefaultSpearSynth(bool explosive, bool electric)
    {
        return (world, pos, id) => new AbstractSpear(world, null, pos, id, explosive, electric);
    }

    internal static string Format(this AbstractPhysicalObject obj) => obj switch
    {
        AbstractSpear spear => $"(Spear, explosive? {spear.explosive}, electric? {spear.electric})",
        AbstractCreature crit => $"(Creature, template.type? {crit.creatureTemplate.type}, meatLeft? {crit.state.meatLeft}, dead? {crit.state.dead}), swallowable? {crit.IsConvertable()}",
        _ => $"(Object, type? {obj.type})"
    };

    internal static bool IsConvertable(this AbstractCreature creature) =>
        !NotSwallowableCreatures.Contains(creature.creatureTemplate.type);
    
    internal static int GetMatterValueForObject(PhysicalObject obj)
    {
        if (obj is IPlayerEdible edible)
            return edible.FoodPoints;

        return GetMatterValueForAbstractObject(obj.abstractPhysicalObject);
    }
    
    internal static int GetMatterValueForAbstractObject(AbstractPhysicalObject obj)
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
        if (type == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer || type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)
            return -10000;
        
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
        
        if (type == AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer)
            return 40;
        
        if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            return 80;
        
        if (type == DLCSharedEnums.AbstractObjectType.SingularityBomb)
            return 600;
        
        return 0;
    }
}