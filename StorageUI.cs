using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using MagicStoragePlus.Components;
using MagicStoragePlus.Sorting;
using Terraria.Localization;

namespace MagicStoragePlus
{
    public class StorageUI : UIState
    {
        public bool Crafting = false;

        int numColumns = 10;

        List<Item> items = new List<Item>();

        UISlotZone slotZone;
        UIElement bottomBar = new UIElement();
        UIText capacityText = new UIText("", 0.7f);

        List<UITextButton> sideBar;

        int slotFocus = -1;
        int rightClickTimer = 0;
        const int startMaxRightClickTimer = 20;
        int maxRightClickTimer = startMaxRightClickTimer;

        bool dirty = true;

        public void BuildSortAndFilter()
        {
            sideBar = new List<UITextButton>();

            // @Volatile :SearchBars
            sideBar.Add(new UISearchBar(Language.GetTextValue("Mods.MagicStoragePlus.SearchName"), 0.75f, 1.0f));
            sideBar.Add(new UISearchBar(Language.GetTextValue("Mods.MagicStoragePlus.SearchMod"), 0.75f, 1.0f));

            var searchByName = sideBar[0] as UISearchBar;
            var searchByMod = sideBar[1] as UISearchBar;

            searchByName.OnTextChanged += () => { RefreshItems(); };
            searchByName.OnTabPressed += () => { searchByMod.Focus(); };

            searchByMod.OnTextChanged += () => { RefreshItems(); };
            searchByMod.OnTabPressed += () => { searchByName.Focus(); };

            sideBar.Add(new UIDropDown("⇄ " + Language.GetTextValue("Mods.MagicStoragePlus.Sort"), 0.75f, 1.0f));
            sideBar.Add(new UIDropDown("∇ " + Language.GetTextValue("Mods.MagicStoragePlus.Filter"), 0.75f, 1.0f));

            // @Volatile :SortAndFilter
            var sort = sideBar[2] as UIDropDown;
            var filter = sideBar[3] as UIDropDown;

            sort.TextColor = new Color(252, 180, 151);
            filter.TextColor = new Color(252, 180, 151);

            sort.Action += (bool rightClicked, bool mouseInBounds) =>
            {
                if (!rightClicked && mouseInBounds && filter.Focused) filter.Unfocus();
            };
            filter.Action += (bool rightClicked, bool mouseInBounds) =>
            {
                if (!rightClicked && mouseInBounds && sort.Focused) sort.Unfocus();
            };

            sort.AddOption(new UITextButton("▦ " + Language.GetTextValue("Mods.MagicStoragePlus.SortDefault"), 0.75f, 1.0f));
            // sort.AddOption(new UITextButton("▩ " + Language.GetText("Mods.MagicStoragePlus.SortID"), 0.75f, 1.0f));
            sort.AddOption(new UITextButton("▧ " + Language.GetTextValue("Mods.MagicStoragePlus.SortName"), 0.75f, 1.0f));
            sort.AddOption(new UITextButton("▨ " + Language.GetTextValue("Mods.MagicStoragePlus.SortQuantity"), 0.75f, 1.0f));
            foreach (var o in sort.Options)
                o.Action += (bool rightClicked, bool mouseInBounds) => { if (!rightClicked && mouseInBounds) RefreshItems(); };

            filter.AddOption(new UITextButton("∀ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterAll"), 0.75f, 1.0f));
            filter.AddOption(new UITextButton("⚔ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterWeapons"), 0.75f, 1.0f));
            filter.AddOption(new UITextButton("⛏ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterTools"), 0.75f, 1.0f));
            filter.AddOption(new UITextButton("⛨ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterEquipment"), 0.75f, 1.0f));
            filter.AddOption(new UITextButton("♁ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterPotions"), 0.75f, 1.0f));
            filter.AddOption(new UITextButton("☐ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterPlaceables"), 0.75f, 1.0f));
            filter.AddOption(new UITextButton("★ " + Language.GetTextValue("Mods.MagicStoragePlus.FilterMisc"), 0.75f, 1.0f));
            foreach (var o in filter.Options)
                o.Action += (bool rightClicked, bool mouseInBounds) => { if (!rightClicked && mouseInBounds) RefreshItems(); };
        }

        public void RebuildUI()
        {
            RemoveAllChildren();

            slotZone = new UISlotZone(HoverItemSlot, GetItem, UI.InventoryScale);
            Append(slotZone);

            slotZone.ScrollBar = new UIScrollbar();
            Append(slotZone.ScrollBar);

            sideBar = new List<UITextButton>();
            BuildSortAndFilter();

            sideBar.Add(new UITextButton(Lang.inter[30].Value, 0.75f, 1.0f)); // Deposit all
            var depositAll = sideBar[4];
            depositAll.Action += (bool rightClicked, bool mouseInBounds) =>
            {
                if (!rightClicked && mouseInBounds && TryDepositAll())
                {
                    RefreshItems();
                    Main.PlaySound(7, -1, -1, 1);
                    Recipe.FindRecipes();
                }
            };
            Append(depositAll);

            sideBar.Add(new UITextButton(Lang.inter[31].Value, 0.75f, 1.0f)); // Quick stack
            var quickStack = sideBar[5];
            quickStack.Action += (bool rightClicked, bool mouseInBounds) =>
            {
                if (!rightClicked && mouseInBounds && TryQuickStack(true))
                {
                    RefreshItems();
                    Main.PlaySound(7, -1, -1, 1);
                    Recipe.FindRecipes();
                }
            };
            Append(quickStack);

            sideBar.Add(new UITextButton(Lang.inter[82].Value, 0.75f, 1.0f)); // Restock
            var restock = sideBar[6];
            restock.Action += (bool rightClicked, bool mouseInBounds) =>
            {
                if (!rightClicked && mouseInBounds && TryRestock())
                {
                    RefreshItems();
                    Main.PlaySound(7, -1, -1, 1);
                    Recipe.FindRecipes();
                }
            };
            Append(restock);

            bottomBar.Append(capacityText);
            Append(bottomBar);

            foreach (var s in sideBar) Append(s);
        }

        public override void OnInitialize()
        {
            RebuildUI();
        }

        public override void OnDeactivate()
        {
            (sideBar[0] as UISearchBar)?.Unfocus();
            (sideBar[1] as UISearchBar)?.Unfocus();
            ResetSlotFocus();
        }

        public void Unload()
        {
            items = null;
            slotZone = null;
            bottomBar = null;
            capacityText = null;
            sideBar = null;
        }

        // @Speed: We are calling this every frame. Do we need to?
        public void UpdateUI()
        {
            int padding = 2;

            if (dirty)
            {
                RebuildUI();
                dirty = false;
            }

            float slotWidth = Main.inventoryBackTexture.Width * UI.InventoryScale;
            float slotHeight = Main.inventoryBackTexture.Height * UI.InventoryScale;

            float panelLeft = 72;
            float panelHeight = Main.screenHeight - Main.instance.invBottom - 40f;
            float innerPanelWidth = numColumns * (slotWidth + padding) + padding * 4;

            int rows = (int)(panelHeight / slotHeight) - 1;

            if (Crafting && rows > 4) rows = 4;

            panelHeight = rows * (slotHeight + padding) + padding * 2;
            if (Crafting)
                UI.TrashSlotOffset = new Point16(4, (int)panelHeight);
            else
                UI.TrashSlotOffset = new Point16(4, (int)panelHeight + 7);
            if (Main.recBigList)
                UI.TrashSlotOffset = new Point16(3, 0);

            Left.Set(panelLeft, 0f);
            Top.Set(Main.instance.invBottom, 0f);
            Width.Set(innerPanelWidth + 25, 0f);
            Height.Set(panelHeight, 0f);

            {
                slotZone.Width.Set(innerPanelWidth, 0);
                slotZone.Height.Set(0, 1);

                slotZone.SetDimensions(numColumns, rows);

                int noDisplayRows = (items.Count + numColumns - 1) / numColumns - rows; // @Magic 

                var bar = slotZone.ScrollBar;
                bar.Left.Set(-22, 1f);
                bar.Top.Set(11, 0);
                bar.Height.Set(panelHeight - padding * 10, 0f);
                bar.SetView(1, noDisplayRows + 1);
            }
            {
                float sideBarLeft = innerPanelWidth + 35;

                for (int i = 0; i < sideBar.Count; i++)
                {
                    var element = sideBar[i];
                    int offset = 0;
                    if (i > 2)
                    {
                        var sort = sideBar[2] as UIDropDown; // @Volatile :SortAndFilter
                        if (sort.Focused)
                            offset += sort.OptionsY + sort.OptionDiffY * (sort.Options.Count - 1);
                    }
                    if (i > 3)
                    {
                        var filter = sideBar[3] as UIDropDown; // @Volatile :SortAndFilter
                        if (filter.Focused)
                            offset += filter.OptionsY + filter.OptionDiffY * (filter.Options.Count - 1);
                    }
                    element.Left.Set(sideBarLeft, 0);
                    element.Top.Set(15 + 27 * i + offset, 0);
                }
            }
            {
                bottomBar.Top.Set(6, 1f);

                int numItems = 0;
                int capacity = 0;

                TEStorageHeart heart = StoragePlayer.GetStorageHeart();
                if (heart != null)
                {
                    foreach (var unit in heart.GetStorageUnits())
                    {
                        if (unit is TEStorageUnit)
                        {
                            var storageUnit = unit as TEStorageUnit;
                            numItems += storageUnit.NumItems;
                            capacity += storageUnit.Capacity;
                        }
                    }
                }
                string text = "";
                if (!Crafting) text = "• " + numItems + "/" + capacity + " Items";

                capacityText.SetText(text);
            }
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            if (UI.RightReleased) ResetSlotFocus();

            var searchByName = sideBar[0] as UISearchBar;
            var searchByMod = sideBar[1] as UISearchBar;
            if (slotFocus >= 0)
            {
                searchByName.Unfocus();
                searchByMod.Unfocus();
            }

            var mouse = new Vector2(Main.mouseX, Main.mouseY);
            if (ContainsPoint(mouse))
            {
                Player player = Main.player[Main.myPlayer];
                player.mouseInterface = true;
                player.showItemIcon = false;
                UI.HideItemIconCache();
            }

            UpdateUI();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            slotZone.DrawText();
        }

        public void RefreshItems()
        {
            items.Clear();

            TEStorageHeart heart = StoragePlayer.GetStorageHeart();
            if (heart == null) return;

            // @Volatile :SearchBars
            var searchByName = sideBar[0] as UISearchBar;
            var searchByMod = sideBar[1] as UISearchBar;

            string nameFilter = "";
            if (searchByName.Text != searchByName.HintText) nameFilter = searchByName.Text;
            string modFilter = "";
            if (searchByMod.Text != searchByMod.HintText) modFilter = searchByMod.Text;

            var sort = sideBar[2] as UIDropDown; // @Volatile :SortAndFilter
            var filter = sideBar[3] as UIDropDown; // @Volatile :SortAndFilter

            items.AddRange(Sorting.Items.FilterAndSort(heart.GetStoredItems(), (SortMode)sort.CurrentOption, (FilterMode)filter.CurrentOption, modFilter, nameFilter));
            items.ForEach(x => x.checkMat());

            Recipe.FindRecipes();
        }

        void ResetSlotFocus()
        {
            slotFocus = -1;
            rightClickTimer = 0;
            maxRightClickTimer = startMaxRightClickTimer;
        }

        static void HoverItemSlot(int slot, ref int hoverSlot)
        {
            var sg = MagicStoragePlus.Instance.StorageUI;

            Player player = Main.player[Main.myPlayer];
            int visualSlot = slot;
            slot += sg.numColumns * (int)Math.Round(sg.slotZone.ScrollBar.ViewPosition);
            if (UI.LeftClicked)
            {
                bool changed = false;
                if (!Main.mouseItem.IsAir && (player.itemAnimation == 0 && player.itemTime == 0))
                {
                    if (TryDeposit(Main.mouseItem))
                        changed = true;
                }
                else if (Main.mouseItem.IsAir && slot < sg.items.Count && !sg.items[slot].IsAir)
                {
                    Item toWithdraw = sg.items[slot].Clone();
                    if (toWithdraw.stack > toWithdraw.maxStack)
                        toWithdraw.stack = toWithdraw.maxStack;
                    Main.mouseItem = DoWithdraw(toWithdraw, ItemSlot.ShiftInUse);
                    if (ItemSlot.ShiftInUse)
                        Main.mouseItem = player.GetItem(Main.myPlayer, Main.mouseItem, false, true);
                    changed = true;
                }
                if (changed)
                {
                    sg.RefreshItems();
                    Main.PlaySound(7, -1, -1, 1);
                }
            }

            if (UI.RightClicked && slot < sg.items.Count && (Main.mouseItem.IsAir || ItemData.Matches(Main.mouseItem, sg.items[slot]) && Main.mouseItem.stack < Main.mouseItem.maxStack))
            {
                sg.slotFocus = slot;
            }

            if (slot < sg.items.Count && !sg.items[slot].IsAir)
                hoverSlot = visualSlot;

            if (sg.slotFocus >= 0)
                sg.SlotFocusLogic();
        }

        void SlotFocusLogic()
        {
            if (slotFocus >= items.Count || (!Main.mouseItem.IsAir && (!ItemData.Matches(Main.mouseItem, items[slotFocus]) || Main.mouseItem.stack >= Main.mouseItem.maxStack)))
            {
                ResetSlotFocus();
            }
            else
            {
                if (rightClickTimer <= 0)
                {
                    rightClickTimer = maxRightClickTimer;
                    maxRightClickTimer = maxRightClickTimer * 3 / 4;
                    if (maxRightClickTimer <= 0)
                        maxRightClickTimer = 1;

                    var request = items[slotFocus].Clone();
                    request.stack = 1;

                    var result = DoWithdraw(request);
                    if (Main.mouseItem.IsAir)
                        Main.mouseItem = result;
                    else
                        Main.mouseItem.stack += result.stack;

                    Main.soundInstanceMenuTick.Stop();
                    Main.soundInstanceMenuTick = Main.soundMenuTick.CreateInstance();
                    Main.PlaySound(12, -1, -1, 1);
                    RefreshItems();
                }
                rightClickTimer--;
            }
        }

        public static void DoWithdrawItemForCraft(Recipe self, ref Item item, ref int required)
        {
            if (StoragePlayer.Get.StorageAccess.X < 0) return;

            var instance = MagicStoragePlus.Instance.StorageUI;

            bool changed = false;
            foreach (Item i in instance.items)
            {
                if (required <= 0)
                {
                    if (changed) instance.RefreshItems();
                    return;
                }

                if (item.IsTheSameAs(i) || self.useWood(item.type, i.type) || self.useSand(item.type, i.type) || self.useIronBar(item.type, i.type) || self.usePressurePlate(item.type, i.type) || self.useFragment(item.type, i.type) || self.AcceptedByItemGroups(item.type, i.type))
                {
                    int count = Math.Min(required, item.stack);
                    required -= count;

                    var request = i.Clone();
                    request.stack = count;

                    TEStorageHeart heart = StoragePlayer.GetStorageHeart();
                    if (Main.netMode == 0)
                    {
                        request = heart.TryWithdraw(request);
                    }
                    else
                    {
                        NetHelper.SendWithdraw(heart.ID, request, NetHelper.StorageOp.WithdrawJustRemove);
                        request = new Item();
                    }

                    if (!request.IsAir)
                    {
                        request.TurnToAir();
                        changed = true;
                    }
                }
            }
            if (changed) instance.RefreshItems();
        }

        static Item DoWithdraw(Item item, bool toInventory = false)
        {
            TEStorageHeart heart = StoragePlayer.GetStorageHeart();
            if (Main.netMode == 0)
            {
                return heart.TryWithdraw(item);
            }
            else
            {
                var type = toInventory ? NetHelper.StorageOp.WithdrawToInventory : NetHelper.StorageOp.Withdraw;
                NetHelper.SendWithdraw(heart.ID, item, type);
                return new Item();
            }
        }

        static void DoDeposit(Item item)
        {
            TEStorageHeart heart = StoragePlayer.GetStorageHeart();
            if (Main.netMode == 0)
            {
                heart.DepositItem(item);
            }
            else
            {
                NetHelper.SendDeposit(heart.ID, item);
                item.SetDefaults(0, true);
            }
        }

        static bool TryDeposit(Item item)
        {
            int oldStack = item.stack;
            DoDeposit(item);
            return oldStack != item.stack;
        }

        public static bool TryDepositAll()
        {
            Player player = Main.player[Main.myPlayer];
            TEStorageHeart heart = StoragePlayer.GetStorageHeart();
            bool changed = false;

            if (Main.netMode == 0)
            {
                for (int k = 10; k < 50; k++)
                {
                    if (!player.inventory[k].IsAir && !player.inventory[k].favorited)
                    {
                        int oldStack = player.inventory[k].stack;
                        heart.DepositItem(player.inventory[k]);
                        if (oldStack != player.inventory[k].stack)
                            changed = true;
                    }
                }
            }
            else
            {
                List<Item> items = new List<Item>();
                for (int k = 10; k < 50; k++)
                {
                    if (!player.inventory[k].IsAir && !player.inventory[k].favorited)
                        items.Add(player.inventory[k]);
                }
                NetHelper.SendDepositAll(heart.ID, items);
                foreach (Item item in items)
                    item.SetDefaults(0, true);
                changed = true;
                items.Clear();
            }
            return changed;
        }

        public static bool TryRestock()
        {
            Player player = Main.player[Main.myPlayer];

            bool changed = false;

            for (int k = 0; k < 58; k++)
            {
                var item = player.inventory[k];

                // Skip air, coint slots and coins (code taken from Terraria's restock in chests) 
                if (item != null && !item.IsAir && (k < 50 || k >= 54) && (item.type < 71 || item.type > 74))
                {
                    if (item.maxStack > 1 && item.prefix == 0 && item.stack < item.maxStack)
                    {
                        var request = item.Clone();
                        request.stack = item.maxStack - item.stack;
                        request = DoWithdraw(request, true);
                        if (!request.IsAir)
                        {
                            item.stack += request.stack;
                            request.TurnToAir();
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        public static bool TryQuickStack(bool stackToCurrentlyOpenIfNotSearchForNearbyAccess)
        {
            Player player = Main.player[Main.myPlayer];
            if (player.IsStackingItems()) return false;

            TEStorageHeart heart = StoragePlayer.GetStorageHeart();
            if (!stackToCurrentlyOpenIfNotSearchForNearbyAccess)
            {
                // Vanilla quick stack to all chests is approximately a circle with radius 12.5
                for (int i = -13; i <= 13; i++)
                {
                    for (int j = -13; j <= 13; j++)
                    {
                        int x = (int)player.position.X / 16 + i;
                        int y = (int)player.position.Y / 16 + j;

                        Tile tile = Main.tile[x, y];
                        if (tile == null) continue;

                        if (tile.frameX % 36 == 18) x--;
                        if (tile.frameY % 36 == 18) y--;

                        int tileType = tile.type;
                        ModTile modTile = TileLoader.GetTile(tileType);
                        if (modTile == null || !(modTile is StorageAccess)) continue;

                        if ((player.Center - new Vector2(x, y) * 16).Length() < 200)
                            heart = ((StorageAccess)modTile).GetHeart(x, y);
                    }
                }
            }
            if (heart == null) return false;

            List<Item> items = new List<Item>();
            for (int k = 10; k < 50; k++)
            {
                var item = player.inventory[k];
                if (!item.IsAir && !item.favorited)
                {
                    if (heart.HasItem(item, true))
                        items.Add(player.inventory[k]);
                }
            }

            if (Main.netMode == 0)
            {
                foreach (Item item in items)
                    heart.DepositItem(item);
            }
            else
            {
                NetHelper.SendDepositAll(heart.ID, items);
                foreach (Item item in items)
                    item.SetDefaults(0, true);
            }

            // Play the stash sound
            // The if is here so minimize IL hacking in DrawInventory...
            if (items.Count != 0 && !stackToCurrentlyOpenIfNotSearchForNearbyAccess)
                Main.PlaySound(7, -1, -1, 1, 1f, 0f);
            return items.Count != 0;
        }


        static Item GetItem(int slot)
        {
            var sg = MagicStoragePlus.Instance.StorageUI;
            int index = slot + sg.numColumns * (int)Math.Round(sg.slotZone.ScrollBar.ViewPosition);
            return index < sg.items.Count ? sg.items[index] : new Item();
        }
    }
}