namespace TheAlchemist;

internal class AlchemistInfo
{
    internal int Matter = 0;
    internal int StomachEatTicker = 0;
    internal int FoodConvertTicker = 0;
    internal readonly Player Owner;

    internal AlchemistInfo(Player owner)
    {
        Owner = owner;
    }
}