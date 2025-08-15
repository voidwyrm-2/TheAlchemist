using ImprovedInput;
using UnityEngine;

namespace TheAlchemist;

using static Vars;
using static Plugin;

public static class PlayerHooks
{
    private static int _playerCount;
    
    internal static void Apply()
    {
        On.Player.ctor += PlayerInit;
        On.Player.CanBeSwallowed += PlayerAllowItemSwallowing;
        On.Player.Update += PlayerUpdate;
    }

    private static void PlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.dead && self.IsAlchem())
        {
            var info = self.GetInfo();

            if (self.IsPressed(EatItemInStomachKey) && self.objectInStomach != null)
            {
                info.StomachEatTicker++;
                
                self.Blink(2);

                if (info.StomachEatTicker >= 33)
                {
                    self.Blink(2);
                    
                    var originalMatter = info.Matter;

                    info.StomachEatTicker = 0;

                    var obj = self.objectInStomach;

                    self.objectInStomach = null;

                    info.Matter += obj.GetMatterValueForItem();

                    var vel = new Vector2(Random.value, Random.value) * 2;
                    var vel2 = new Vector2(Random.value * 2, Random.value) * 3;
                    var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel, color, null, 30, 50));
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel2, color, null, 37, 60));

                    Logger.LogDebug($"Stomach item (which was a {obj.type}) consumed, matter was {originalMatter}, it's now {info.Matter}");
                }
            }
            else
            {
                info.StomachEatTicker = 0;
            }
            
            if (self.IsPressed(ConvertFoodToMatterKey) && self.playerState.foodInStomach > 0)
            {
                info.FoodConvertTicker++;
                
                self.Blink(2);

                if (info.FoodConvertTicker >= 33)
                {
                    self.Blink(2);
                    
                    var originalMatter = info.Matter;

                    info.FoodConvertTicker = 0;
                    
                    self.SubtractFood(1);
                    info.Matter += 20;

                    var vel = new Vector2(Random.value, Random.value) * 2;
                    var vel2 = new Vector2(Random.value * 2, Random.value) * 3;
                    var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel, color, null, 30, 50));
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel2, color, null, 37, 60));

                    Logger.LogDebug($"Consumed 1 Slugcat food, matter was {originalMatter}, it's now {info.Matter}");
                }
            }
            else
            {
                info.FoodConvertTicker = 0;
            }
        }
    }

    private static bool PlayerAllowItemSwallowing(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (self.IsAlchem() && obj is Spear)
            return true;
        
        return orig(self, obj);
    }

    private static void PlayerInit(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (self.IsAlchem())
        {
            Logger.LogInfo($"Alchemist Player found with id {_playerCount}, initializing...");

            AlchemistInfo info = new(self);
            Alchemists.Add(self, info);
            InfoList.Add(info);

            Logger.LogInfo($"Player {_playerCount} initialized");
            _playerCount++;
        }
    }
}