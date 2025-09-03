using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace TheAlchemist;

internal static class OracleHooks
{
    internal static void Apply()
    {
        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversationOnAddEvents;
        IL.SSOracleBehavior.Update += SSOracleBehaviorOnUpdate;
        IL.SSOracleBehavior.SeePlayer += SSOracleBehaviorILSeePlayer;
    }

    private static void PebblesConversationOnAddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
    {
        if (self.owner.oracle.room.game.StoryCharacter != Vars.Alchem)
        {
            orig(self);
            return;
        }
        
        var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData);

        if (!save.TryGet("Nuclear-Alchemist-pebblesEncounteredCount",  out int pebblesEncounteredCount))
            pebblesEncounteredCount = 0;

        #region Helpers
        void Say(string text)
        {
            self.events.Add(new Conversation.TextEvent(self, 0, text, 0));
        }
        void Wait(int pauseFrames)
        {
            self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, pauseFrames));
        }
        #endregion

        if (pebblesEncounteredCount == 0)
        {
            if (!self.owner.playerEnteredWithMark)
            {
                Say(".  .  .");
                Say("...is this reaching you?");
                Wait(4);
            }
            else
            {
                Wait(210);
            }
            
            Say(
                "Little courier, on the floor of my chamber, my overseers notified me of your presence when you entered my grounds.");

            Say("Your abilities are curious, I have not seen anything like them.<LINE>" +
                "You must be a purposed organism from one of my fellow Iterators.<LINE>" +
                "But no members of the local group have the knowledge to give one of your kind those abilities.<LINE>");

            Say("...");

            Wait(5);

            Say("I am getting sidetracked. Normally, when visited by one of your kind, I would send them off to the depths, But you evidently have a specific purpose.<LINE>" +
                "So feel free to stay, but do not interrupt my work.");

            Wait(3);

            var sendoff = "And please do not eat any of these pearls.<LINE>";

            if (self.owner.PlayersInRoom.Any(player =>
                    player.TryGetInfo(out var info) && info.Meta.ConsumedPebblesNeuron))
                sendoff += "Or any of my neurons.<LINE>";

            sendoff += "I have many, but they’re still too valuable for me to allow you to consume them.";

            Say(sendoff);
        }
        
        //if (ModManager.MSC && self.owner.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
        //{
        //    Say("Best of luck to you, and your companion. There is nothing else I can do.");
        //    Say("I must resume my work.");
        //    self.owner.CreatureJokeDialog();
        //    return;
        //}
        
        save.Set("Nuclear-Alchemist-pebblesEncounteredCount", pebblesEncounteredCount);
    }

    private static void SSOracleBehaviorOnUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<RainWorldGame>("get_StoryCharacter"),
                i => i.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Artificer"),
                i => i.MatchCallOrCallvirt(out _),
                i => i.MatchBrfalse(out label)
            );
            c.GotoPrev(MoveType.AfterLabel,
                i => i.MatchLdsfld<ModManager>(nameof(ModManager.MSC))
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((SSOracleBehavior beh) =>
            {
                if (beh.oracle.room.game.StoryCharacter == Vars.Alchem)
                {
                    UnityEngine.Debug.Log("Refusing giving Alchemist karma, he's chubby enough as is.");
                    return true;
                }
                
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
    }

    private static void SSOracleBehaviorILSeePlayer(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SSOracleBehaviorAction>("MeetGourmand_Init")
                );
            label = il.DefineLabel(c.Next);
            c.GotoPrev(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<OracleBehavior>("oracle")
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((SSOracleBehavior beh) => beh.player.SlugCatClass == Vars.Alchem);
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
    }
}