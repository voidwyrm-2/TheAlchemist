using System;
using ImprovedInput;
using RWCustom;
using Smoke;
using UnityEngine;

namespace TheAlchemist;

using static Vars;
using static Plugin;

public static class PlayerHooks
{
    internal static void Apply()
    {
        On.Player.ctor += PlayerInit;
        On.Player.Update += OnUpdate;
        On.ShelterDoor.Close += SavePlayerData;
        On.Player.PermaDie += OnPermaDie;
        On.Player.MovementUpdate += OnMovementUpdate;
        On.Player.NewRoom += OnNewRoom;
        On.Player.Jump += OnJump;
    }

    private static void OnJump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (self.IsAlchem() && self.TryGetInfo(out var info) && info.HyperspeedActive && self.jumpBoost != 0)
            self.jumpBoost *= 1.75f;
    }

    private static void OnNewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
    {
        {
            if (self.IsAlchem() && self.TryGetInfo(out var info) && info.HyperspeedSmoke != null)
            {
                info.HyperspeedSmoke.RemoveFromRoom();
                info.HyperspeedSmoke = null;
            }
        }

        orig(self, newRoom);
    }

    private static void OnMovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.dead && self.IsAlchem() && self.TryGetInfo(out var info))
        {
            if (info.HyperspeedActive && self.input[0].AnyDirectionalInput && self.canJump > 0 &&
                self.firstChunk.vel != Vector2.zero &&
                (self.bodyMode == Player.BodyModeIndex.Default || self.bodyMode == Player.BodyModeIndex.Stand))
            {
                if (info.HyperspeedMatterTicker == NitrousMatterTicks)
                {
                    info.Matter -= HyperspeedMatterCost;
                    info.HyperspeedMatterTicker = 0;
                }
                else
                {
                    info.HyperspeedMatterTicker++;
                }

                //var a = Custom.RNV();
                
                //self.room.AddObject(new Spark(self.bodyChunks[1].pos + a * 5f,
                //    (a - self.firstChunk.vel).normalized * self.firstChunk.vel.magnitude * 1.3f, Color.white, null, 6,
                //    12));

                if (self.room.ViewedByAnyCamera(self.mainBodyChunk.pos, 300f))
                {
                    var catColor = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);
                    info.HyperspeedSmoke?.EmitSmoke(self.mainBodyChunk.pos, Custom.RNV(), catColor, 25);
                    info.HyperspeedSmoke?.EmitSmoke(self.mainBodyChunk.pos, Custom.RNV(), catColor, 30);
                }
            }
        }
    }

    private static void OnPermaDie(On.Player.orig_PermaDie orig, Player self)
    {
        if (self.IsAlchem() && self.TryGetInfo(out var info))
        {
            info.Matter -= MatterLostOnDeath;
            
            if (info.Matter < 0)
                info.Matter = 0;
        }
            
        orig(self);
    }

    private static void SavePlayerData(On.ShelterDoor.orig_Close orig, ShelterDoor self)
    {
        if (self.room.game.IsStorySession)
        {
            foreach (var info in InfoMap.Values)
            {
                Alchemists.Remove(info.Owner);

                if (info.Saved)
                    continue;

                Logger.LogInfo($"Saving info for player {info.PlayerNumber}");
                var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState
                    .miscWorldSaveData);
                info.Save(save);
                Logger.LogInfo($"Saved info for player {info.PlayerNumber}");
            }
        }

        InfoMap.Clear();

        orig(self);
    }

    private static void OnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.IsAlchem() && self.TryGetInfo(out var info))
        {
            if (info.SynthCodeKeyCooldown > 0)
                info.SynthCodeKeyCooldown--;

            if (self.dead)
            {
                info.ObjectMatterTicker = 0;
                info.SynthCode = "";
                info.FoodMatterTicker = 0;
                info.HyperspeedMatterTicker = 0;
                
                if (info.HyperspeedSmoke != null)
                {
                    info.HyperspeedSmoke.RemoveFromRoom();
                    info.HyperspeedSmoke = null;
                }
            }
            else
            {
                if (self.IsPressed(ModKey) && info.SynthCodeKeyCooldown == 0)
                {
                    var inputString = Input.inputString.Trim();

                    if (inputString.Length > 0)
                    {
                        char? digit = inputString[0].Unshift();

                        //Logger.LogDebug($"inputstring was '{digit}'");

                        if (digit >= '0' && digit <= '9')
                        {
                            info.SynthCode += digit;
                            Logger.LogDebug($"Digit added: '{digit}', synthCode is now '{info.SynthCode}'");
                        }

                        info.SynthCodeKeyCooldown = 2;
                    }
                }
                else if (!self.IsPressed(ModKey) && self.FreeHand() > -1 && info.SynthCode.Length > 0)
                {
                    var stringCode = info.SynthCode;
                    info.SynthCode = "";

                    uint synthCode;

                    try
                    {
                        synthCode = uint.Parse(stringCode);
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

                    AbstractPhysicalObject obj = null;

                    if (synthCode < SynthItems.Length)
                    {
                        var world = self.room.world;
                        var pos = self.abstractCreature.pos;
                        var id = self.room.game.GetNewID();
                        obj = SynthItems[synthCode](world, pos, id);
                    }

                    Logger.LogDebug($"synthesis code: '{synthCode}', valid? {obj != null}");

                    if (obj != null)
                    {
                        var cost = Utils.GetMatterValueForAbstractObject(obj);

                        if (info.Matter >= cost)
                        {
                            var originalMatter = info.Matter;

                            self.room.abstractRoom.AddEntity(obj);

                            try
                            {
                                obj.RealizeInRoom();
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    obj.Abstractize(obj.pos);
                                }
                                finally
                                {
                                    self.room.abstractRoom.RemoveEntity(obj);
                                }

                                Logger.LogError(
                                    $"Error while realizing synthesized object (info: code? {synthCode}, obj? {obj.Format()}, matter? {info.Matter}): {e.Message}");
                                return;
                            }

                            self.SlugcatGrab(obj.realizedObject, self.FreeHand());

                            self.SpawnMatterToObjectEffect(obj.realizedObject.firstChunk.pos);

                            info.Matter -= cost;

                            Logger.LogDebug(
                                $"Created a {obj.Format()} for {cost} matter; matter was {originalMatter}, it's now {info.Matter}");
                        }
                        else
                        {
                            Logger.LogDebug(
                                $"Not enough matter to create {obj.Format()}; need {cost}, but have {info.Matter}");
                        }
                    }
                }

                if (self.IsPressed(ObjectMatterKey))
                {
                    /*
                    if (self.IsPressed(ModKey) && self.FreeHand() > -1 && info.LastConsumed != null)
                    {
                        info.ObjectMatterTicker++;

                        self.Blink(2);

                        var obj = info.LastConsumed;

                        if (info.ObjectMatterTicker >= ObjectToMatterTicks || self.Debug())
                        {
                            self.Blink(4);

                            info.ObjectMatterTicker = 0;

                            var cost = Utils.GetMatterValueForAbstractObject(obj);

                            if (info.Matter >= cost)
                            {
                                var originalMatter = info.Matter;

                                self.room.abstractRoom.AddEntity(obj);

                                try
                                {
                                    obj.RealizeInRoom();
                                }
                                catch (Exception e)
                                {
                                    try
                                    {
                                        obj.Abstractize(obj.pos);
                                    }
                                    finally
                                    {
                                        self.room.abstractRoom.RemoveEntity(obj);
                                    }

                                    Logger.LogError(
                                        $"Error while realizing synthesized object (info: code? {synthCode}, obj? {obj.Format()}, matter? {info.Matter}): {e.Message}");
                                    return;
                                }

                                self.SlugcatGrab(obj.realizedObject, self.FreeHand());

                                self.SpawnMatterToObjectEffect(obj.realizedObject.firstChunk.pos);

                                info.Matter -= cost;

                                Logger.LogDebug(
                                    $"Created a {obj.Format()} for {cost} matter; matter was {originalMatter}, it's now {info.Matter}");
                            }
                            else
                            {
                                Logger.LogDebug(
                                    $"Not enough matter to create {obj.Format()}; need {cost}, but have {info.Matter}");
                            }
                        }
                    }
                    else
                    */
                    if (self.OccupiedHand() > -1 || self.Debug())
                    {
                        info.ObjectMatterTicker++;

                        self.Blink(2);

                        if (info.ObjectMatterTicker >= ObjectToMatterTicks || self.Debug())
                        {
                            self.Blink(4);

                            info.ObjectMatterTicker = 0;

                            var originalMatter = info.Matter;

                            if (self.Debug())
                            {
                                info.Matter += 100;
                            }
                            else if (self.OccupiedHand() > -1)
                            {
                                var index = self.OccupiedHand();
                                var obj = self.grasps[index].grabbed;

                                info.Meta.CheckForMetaItem(self, obj);

                                var matter = Utils.GetMatterValueForObject(obj);
                                
                                if (obj.abstractPhysicalObject is not AbstractCreature && matter > 0)
                                    info.LastConsumed = obj.abstractPhysicalObject.type;

                                self.SpawnObjectToMatterEffect(obj.firstChunk.pos);
                                self.ReleaseGrasp(index);
                                obj.RemoveFromRoom();
                                obj.Destroy();

                                if (matter > 0)
                                    info.Matter += matter;

                                Logger.LogDebug(
                                    $"Consumed stomach item (which was a {obj.abstractPhysicalObject.Format()}) for {matter} matter; matter was {originalMatter}, it's now {info.Matter}");
                            }
                        }
                    }
                    else
                    {
                        info.ObjectMatterTicker = 0;
                    }
                }

                if (self.IsPressed(FoodMatterKey))
                {
                    if (self.IsPressed(ModKey) && self.FoodInStomach < self.MaxFoodInStomach && info.Matter >= FoodPipMatterCost)
                    {
                        info.FoodMatterTicker++;

                        self.Blink(2);

                        if (info.FoodMatterTicker >= FoodMatterTicks)
                        {
                            self.Blink(4);

                            info.FoodMatterTicker = 0;

                            var originalMatter = info.Matter;

                            info.Matter -= FoodPipMatterCost;

                            self.AddFood(1);

                            Logger.LogDebug(
                                $"Added 1 food pip for {FoodPipMatterCost} matter; matter was {originalMatter}, it's now {info.Matter}");

                            self.SpawnFoodToMatterEffect(self.mainBodyChunk.pos);
                        }
                    }
                    else if (self.FoodInStomach > 0)
                    {
                        info.FoodMatterTicker++;

                        self.Blink(2);

                        if (info.FoodMatterTicker >= FoodMatterTicks)
                        {
                            self.Blink(4);

                            info.FoodMatterTicker = 0;

                            var originalMatter = info.Matter;

                            self.SubtractFood(1);
                            info.Matter += 20;
                            self.SpawnFoodToMatterEffect(self.mainBodyChunk.pos);
                            Logger.LogDebug(
                                $"Consumed 1 food pip; matter was {originalMatter}, it's now {info.Matter}");
                        }
                    }
                    else
                    {
                        info.FoodMatterTicker = 0;
                    }
                }

                if (self.IsPressed(HyperspeedKey) && info.Matter >= HyperspeedMatterCost)
                {
                    if (!info.HyperspeedActive)
                    {
                        info.State.Save(self);
                        info.HyperspeedActive = true;

                        const float groundIncreaseFac = 2.5f;
                        const float poleIncreaseFac = 2.7f;

                        self.slugcatStats.runspeedFac *= groundIncreaseFac;
                        self.slugcatStats.poleClimbSpeedFac *= poleIncreaseFac;
                        self.slugcatStats.corridorClimbSpeedFac *= poleIncreaseFac;
                        self.slugcatStats.loudnessFac *= groundIncreaseFac;
                    }

                    if (info.HyperspeedSmoke == null && info.HyperspeedActive)
                        info.HyperspeedSmoke = new FireSmoke(self.room);
                }
                else if (info.HyperspeedActive)
                {
                    info.State.Load(self);
                    info.HyperspeedActive = false;

                    if (info.HyperspeedSmoke != null)
                    {
                        info.HyperspeedSmoke.RemoveFromRoom();
                        info.HyperspeedSmoke = null;
                    }
                }
            }
        }
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

            AlchemistInfo info;
            bool loaded;

            if (self.room.game.IsStorySession)
            {
                var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
                (info, loaded) = AlchemistInfo.LoadFromSave(save, self);
            }
            else
            {
                info = new AlchemistInfo(self);
                loaded = false;
            }

            Alchemists.Add(self, info);
            InfoMap.Add(playerNumber, info);

            if (loaded)
                Logger.LogInfo($"Player {playerNumber} info loaded from save");
            else
                Logger.LogInfo($"Player {playerNumber} info initialized");
        }
    }
}