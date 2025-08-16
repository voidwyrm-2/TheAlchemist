using System;
using ImprovedInput;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheAlchemist;

using static Vars;
using static Plugin;

public static class PlayerHooks
{
    internal static void Apply()
    {
        On.Player.ctor += PlayerInit;
        On.Player.CanBeSwallowed += PlayerAllowItemSwallowing;
        On.Player.Update += PlayerUpdate;
        On.ShelterDoor.Close += SavePlayerData;
        On.Player.EatMeatUpdate += Veganism;
    }

    private static void Veganism(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
    {
        if (self.IsAlchem() && self.grasps[graspIndex].grabbed is Creature creature && (creature.State.meatLeft == 0 || self.FoodInStomach == self.MaxFoodInStomach))
            return;
        
        orig(self, graspIndex);
    }

    private static void SavePlayerData(On.ShelterDoor.orig_Close orig, ShelterDoor self)
    {
        foreach (var info in InfoMap.Values)
        {
            Alchemists.Remove(info.Owner);
            
            if (info.Saved || !self.room.game.IsStorySession)
                continue;
                
            Logger.LogInfo($"Saving info for player {info.PlayerNumber}");
            var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
            info.Save(save);
            Logger.LogInfo($"Saved info for player {info.PlayerNumber}");
        }
        
        InfoMap.Clear();

        orig(self);
    }

    private static void PlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.dead && self.IsAlchem())
        {
            var info = self.GetInfo();

            if (self.IsPressed(ConvertToMatterKey) && (self.objectInStomach != null || self.FoodInStomach > 0))
            {
                info.StomachEatTicker++;
                
                self.Blink(2);

                if (info.StomachEatTicker >= 33)
                {
                    self.Blink(4);
                    
                    info.StomachEatTicker = 0;
                    
                    var originalMatter = info.Matter;

                    if (self.objectInStomach != null)
                    {
                        var obj = self.objectInStomach;

                        self.objectInStomach = null;

                        info.Matter += Utils.GetMatterValueForObject(obj);
                        
                        Logger.LogDebug($"Consumed stomach item (which was a {obj.type}); matter was {originalMatter}, it's now {info.Matter}");
                    }
                    else
                    {
                        self.SubtractFood(1);
                        info.Matter += 20;
                        Logger.LogDebug($"Consumed 1 food pip; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    
                    var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);

                    var vel = new Vector2(Random.value, Random.value) * 2;
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel, color, null, 30, 50));
                    
                    var vel2 = new Vector2(Random.value * 2, Random.value) * 3;
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel2, color, null, 37, 60));
                }
            }
            else
            {
                info.StomachEatTicker = 0;
            }
            
            if (self.IsPressed(ConvertMatterToFoodKey) && self.FoodInStomach < self.MaxFoodInStomach && info.Matter >= FoodPipMatterCost)
            {
                info.MatterToFoodTicker++;
                
                self.Blink(2);

                if (info.MatterToFoodTicker >= 33)
                {
                    self.Blink(4);
                    
                    info.MatterToFoodTicker = 0;
                    
                    var originalMatter = info.Matter;
                    
                    info.Matter -= FoodPipMatterCost;
                    
                    self.AddFood(1);
                    
                    Logger.LogDebug($"Added 1 food pip for {FoodPipMatterCost} matter; matter was {originalMatter}, it's now {info.Matter}");
                    
                    var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);

                    var vel = new Vector2(Random.value, Random.value) * 2;
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel, color, null, 40, 70));
                    
                    //self.room.AddObject(new Lightning(self.room, 2f + Random.value * 3, false));
                    //self.room.AddObject(new KarmicArmor.EnergyStrand(4 + (int)(Random.value * 10), 0.5f + Random.value * 2)
                    //{
                    //    pos = self.mainBodyChunk.pos
                    //});
                }
            }
            else
            {
                info.MatterToFoodTicker = 0;
            }

            if (self.IsPressed(SynthesisKey))
            {
                var digit = $"{Input.inputString}".Trim().Trim('n');

                if (digit.Length > 0 && digit[0] >= '0' && digit[0] <= '9')
                {
                    info.SynthCode += digit[0];
                    Logger.LogDebug($"Digit added: '{digit}', synthCode is now '{info.SynthCode}'");
                }
            }
            else if (self.objectInStomach == null && info.SynthCode.Length > 0)
            {
                var stringCode = info.SynthCode;
                info.SynthCode = "";
                
                int synthCode;

                try
                {
                    synthCode = int.Parse(stringCode);
                }
                catch (FormatException e)
                {
                    Logger.LogError($"Cannot parse synthesis code: {e.Message}");
                    return;
                }
                catch (OverflowException e)
                {
                    Logger.LogError($"Cannot parse synthesis code: {e.Message}");
                    return;
                }

                var type = synthCode switch
                {
                    0 => AbstractPhysicalObject.AbstractObjectType.Rock,
                    1 => AbstractPhysicalObject.AbstractObjectType.Spear,
                    2 => AbstractPhysicalObject.AbstractObjectType.ScavengerBomb,
                    3 => DLCSharedEnums.AbstractObjectType.SingularityBomb,
                    _ => null
                };

                Logger.LogDebug($"synthesis code: '{synthCode}', valid? {type != null}");

                if (type != null)
                {
                    AbstractPhysicalObject obj;

                    var world = self.room.world;
                    var pos = self.room.GetWorldCoordinate(self.mainBodyChunk.pos);
                    var id = self.room.game.GetNewID();

                    if (type == AbstractPhysicalObject.AbstractObjectType.Spear)
                        obj = new AbstractSpear(world, null, pos, id, false);
                    else
                        obj = new AbstractPhysicalObject(world, type, null, pos, id);

                    
                    var cost = Utils.GetMatterValueForObject(obj);

                    if (info.Matter >= cost)
                    {
                        var originalMatter = info.Matter;
                        
                        info.Matter -= cost;
                        self.objectInStomach = obj;
                        
                        Logger.LogDebug($"Created a {type} for {cost} matter; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    else
                    {
                        Logger.LogDebug($"Not enough matter to create {type}; need {cost}, but have {info.Matter}");
                    }
                }
            }
        }
    }

    private static bool PlayerAllowItemSwallowing(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (self.IsAlchem() && self.objectInStomach == null)
        {
            if (obj is Spear)
                return true;
            
            if (obj.abstractPhysicalObject is AbstractCreature creature && creature.IsSwallowable() && (creature.state.meatLeft == 0 || self.FoodInStomach == self.MaxFoodInStomach))
                return true;

            if (self.FoodInStomach == self.MaxFoodInStomach && obj is IPlayerEdible)
                return true;
        }

        return orig(self, obj);
    }

    private static void PlayerInit(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (self.IsAlchem())
        {
            var playerNumber = self.playerState.playerNumber;
            
            Logger.LogInfo($"Alchemist Player found with id {playerNumber}, initializing...");

            if (InfoMap.ContainsKey(playerNumber))
            {
                InfoMap.Remove(playerNumber);
                Logger.LogInfo($"Player {playerNumber} was already initialized, previous info cleared");
            }

            var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
            var result = AlchemistInfo.LoadFromSave(save, self);
            Alchemists.Add(self, result.Info);
            InfoMap.Add(playerNumber, result.Info);

            if (result.Loaded)
                Logger.LogInfo($"Player {playerNumber} info loaded from save");
            else
                Logger.LogInfo($"Player {playerNumber} info initialized");
        }
    }
}