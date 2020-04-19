using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using MagicStoragePlus.Items;

namespace MagicStoragePlus.Components
{
    public class StorageHeart : StorageAccess
    {
        public override ModTileEntity GetTileEntity() => mod.GetTileEntity("TEStorageHeart");
        public override int ItemType(int frameX, int frameY) => mod.ItemType("StorageHeart");
        public override bool HasSmartInteract() => true;

        public override TEStorageHeart GetHeart(int i, int j)
        {
            return (TEStorageHeart)TileEntity.ByPosition[new Point16(i, j)];
        }

        public override bool NewRightClick(int i, int j)
        {
            Player player = Main.player[Main.myPlayer];
            Item item = player.inventory[player.selectedItem];
            if (item.type == mod.ItemType("Locator") || item.type == mod.ItemType("LocatorDisk") || item.type == mod.ItemType("PortableAccess"))
            {
                if (Main.tile[i, j].frameX % 36 == 18)
                    i--;
                if (Main.tile[i, j].frameY % 36 == 18)
                    j--;

                Locator locator = (Locator)item.modItem;
                locator.location = new Point16(i, j);
                if (player.selectedItem == 58)
                    Main.mouseItem = item.Clone();

                Main.NewText("Locator successfully set to: X=" + i + ", Y=" + j);
                return true;
            }
            else
            {
                return base.NewRightClick(i, j);
            }
        }
    }
}