namespace Island.Gameplay.Services.Inventory.Items
{
    public enum InventoryItemType
    {
        Hand = -1,
        None = 0,

        //resources
        Dirt = 1,
        Wood = 2,
        Stone = 3,
        RawFish = 4,

        //usable
        CookedFish = 5,
        Potion = 6,
        Soda = 7,

        //tools
        Hammer = 8,
        Shovel = 9,
        Axe = 10,

        //build
        Fireplace = 11,
        Warehouse = 12,
    }
}