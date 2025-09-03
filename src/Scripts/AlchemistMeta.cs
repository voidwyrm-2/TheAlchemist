using System;
using SlugBase.SaveData;

namespace TheAlchemist;

internal struct AlchemistMeta
{
    internal bool ConsumedPebblesNeuron = false;

    public AlchemistMeta() { }

    private static Func<string, string> CreateKey(int playerNumber)
    {
        return name => $"p{playerNumber}-meta-{name}";
    }

    internal static (AlchemistMeta Meta, bool Loaded) LoadFromSave(SlugBaseSaveData save, int playerNumber)
    {
        var key = CreateKey(playerNumber);
        var loaded = false;
        AlchemistMeta meta = new();

        if (save == null)
            return (meta, false);

        if (save.TryGet(key("ConsumedPebblesNeuron"), out bool consumed))
        {
            meta.ConsumedPebblesNeuron = consumed;
            loaded = true;
        }
        
        return (meta, loaded);
    }

    internal void CheckForMetaItem(Player player, PhysicalObject obj)
    {
        if (obj is SSOracleSwarmer && player.room.abstractRoom.name.StartsWith("SS_"))
            ConsumedPebblesNeuron = true;
    }
}