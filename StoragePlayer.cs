using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using MagicStoragePlus.Components;

namespace MagicStoragePlus
{
    public class StoragePlayer : ModPlayer
    {
        public static Player GetVanilla => Main.player[Main.myPlayer];
        public static StoragePlayer Get => Main.player[Main.myPlayer].GetModPlayer<StoragePlayer>();

        public Point16 StorageAccess = new Point16(-1, -1);

        public int timeSinceOpen = 1;
        public bool remoteAccess = false;

        public override void UpdateDead()
        {
            if (player.whoAmI == Main.myPlayer)
                CloseStorage();
        }

        public override void ResetEffects()
        {
            if (player.whoAmI != Main.myPlayer)
                return;

            if (timeSinceOpen < 1)
            {
                Main.playerInventory = true;

                player.talkNPC = -1;
                timeSinceOpen++;
            }

            if (StorageAccess.X >= 0 && StorageAccess.Y >= 0 && (player.chest != -1 || !Main.playerInventory || player.sign > -1 || player.talkNPC > -1))
            {
                CloseStorage();
                Recipe.FindRecipes();
            }
            else if (StorageAccess.X >= 0 && StorageAccess.Y >= 0)
            {
                int playerX = (int)(player.Center.X / 16f);
                int playerY = (int)(player.Center.Y / 16f);

                if (!remoteAccess && (playerX < StorageAccess.X - Player.tileRangeX || playerX > StorageAccess.X + Player.tileRangeX + 1 || playerY < StorageAccess.Y - Player.tileRangeY || playerY > StorageAccess.Y + Player.tileRangeY + 1))
                {
                    Main.PlaySound(11, -1, -1, 1);
                    CloseStorage();
                    Recipe.FindRecipes();
                }
                else if (!(TileLoader.GetTile(Main.tile[StorageAccess.X, StorageAccess.Y].type) is StorageAccess))
                {
                    Main.PlaySound(11, -1, -1, 1);
                    CloseStorage();
                    Recipe.FindRecipes();
                }
            }
        }

        public void OpenStorage(Point16 point, bool remote = false)
        {
            StorageAccess = point;
            remoteAccess = remote;
            UI.ShowStorage(IsInCrafting());
        }

        public void CloseStorage()
        {
            StorageAccess = new Point16(-1, -1);
            Main.blockInput = false;
            UI.Hide();
        }

        public static void GetItem(Item item, bool toMouse)
        {
            Player player = Main.player[Main.myPlayer];
            if (toMouse && Main.playerInventory && Main.mouseItem.IsAir)
            {
                Main.mouseItem = item;
                item = new Item();
            }
            else if (toMouse && Main.playerInventory && Main.mouseItem.type == item.type)
            {
                int total = Main.mouseItem.stack + item.stack;
                if (total > Main.mouseItem.maxStack)
                    total = Main.mouseItem.maxStack;

                int difference = total - Main.mouseItem.stack;
                Main.mouseItem.stack = total;
                item.stack -= difference;
            }
            if (!item.IsAir)
            {
                item = player.GetItem(Main.myPlayer, item, false, true);
                if (!item.IsAir)
                    player.QuickSpawnClonedItem(item, item.stack);
            }
        }

        public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
        {
            if (context != ItemSlot.Context.InventoryItem && context != ItemSlot.Context.InventoryCoin && context != ItemSlot.Context.InventoryAmmo)
                return false;

            if (StorageAccess.X < 0 || StorageAccess.Y < 0)
                return false;

            Item item = inventory[slot];
            if (item.favorited || item.IsAir)
                return false;

            int oldType = item.type;
            int oldStack = item.stack;

            if (Main.netMode == 0)
            {
                GetStorageHeart().DepositItem(item);
            }
            else
            {
                NetHelper.SendDeposit(GetStorageHeart().ID, item);
                item.SetDefaults(0, true);
            }
            if (item.type != oldType || item.stack != oldStack)
            {
                Main.PlaySound(7, -1, -1, 1, 1f, 0f);
                MagicStoragePlus.Instance.StorageUI.RefreshItems();
            }
            return true;
        }

        public bool IsInCrafting()
        {
            if (StorageAccess.X < 0 || StorageAccess.Y < 0)
                return false;
            Tile tile = Main.tile[StorageAccess.X, StorageAccess.Y];
            return tile != null && tile.type == mod.TileType("CraftingAccess");
        }

        public static StorageAccess GetStorageAccess()
        {
            var player = Get;
            if (!Main.playerInventory || player.StorageAccess.X < 0 || player.StorageAccess.Y < 0)
                return null;
            ModTile result = TileLoader.GetTile(Main.tile[player.StorageAccess.X, player.StorageAccess.Y].type);
            if (result == null || !(result is StorageAccess))
                return null;
            return result as StorageAccess;
        }

        public static TEStorageHeart GetStorageHeart()
        {
            var player = Get;
            if (player.StorageAccess.X < 0 || player.StorageAccess.Y < 0)
                return null;

            Tile tile = Main.tile[player.StorageAccess.X, player.StorageAccess.Y];
            if (tile == null)
                return null;

            int tileType = tile.type;
            ModTile modTile = TileLoader.GetTile(tileType);
            if (modTile == null || !(modTile is StorageAccess))
                return null;
            return ((StorageAccess)modTile).GetHeart(player.StorageAccess.X, player.StorageAccess.Y);
        }

        public static TECraftingAccess GetCraftingAccess()
        {
            var player = Get;
            if (player.StorageAccess.X < 0 || player.StorageAccess.Y < 0 || !TileEntity.ByPosition.ContainsKey(player.StorageAccess))
                return null;
            return TileEntity.ByPosition[player.StorageAccess] as TECraftingAccess;
        }
    }
}