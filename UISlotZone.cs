using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagicStoragePlus
{
    public class UISlotZone : UIElement
    {
        public delegate void HoverItemSlot(int slot, ref int hoverSlot);
        public delegate Item GetItemFromSlot(int slot);

        public HoverItemSlot OnHover;
        public GetItemFromSlot GetItem;

        public UIScrollbar ScrollBar = null;

        int padding = 3;

        int numColumns = 10;
        int numRows = 4;

        int hoverSlot = -1;
        float inventoryScale;

        public UISlotZone(HoverItemSlot onHover, GetItemFromSlot getItem, float inventoryScale)
        {
            OnHover = onHover;
            GetItem = getItem;
            this.inventoryScale = inventoryScale;
        }

        public void SetDimensions(int columns, int rows)
        {
            numColumns = columns;
            numRows = rows;
        }

        public void Update()
        {
            hoverSlot = -1;

            if (ScrollBar != null)
                ScrollBar.ViewPosition += (float)UI.ScrollWheelDelta / 250;

            Vector2 origin = UI.GetFullRectangle(this).TopLeft();
            if (UI.Mouse.X <= origin.X || UI.Mouse.Y <= origin.Y)
                return;

            int slotWidth = (int)(Main.inventoryBackTexture.Width * inventoryScale * Main.UIScale);
            int slotHeight = (int)(Main.inventoryBackTexture.Height * inventoryScale * Main.UIScale);
            int slotX = (int)(UI.Mouse.X - origin.X) / (slotWidth + padding);
            int slotY = (int)(UI.Mouse.Y - origin.Y) / (slotHeight + padding);
            if (slotX < 0 || slotX >= numColumns || slotY < 0 || slotY >= numRows)
            {
                return;
            }
            Vector2 slotPos = origin + new Vector2(slotX * (slotWidth + padding * Main.UIScale), slotY * (slotHeight + padding * Main.UIScale));
            if (
                UI.Mouse.X > slotPos.X && UI.Mouse.X < slotPos.X + slotWidth && UI.Mouse.Y > slotPos.Y && UI.Mouse.Y < slotPos.Y + slotHeight)
            {
                OnHover(slotX + numColumns * slotY, ref hoverSlot);
            }
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            if (ScrollBar != null) ScrollBar.ViewPosition = 0f;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Update();

            float slotWidth = Main.inventoryBackTexture.Width * inventoryScale;
            float slotHeight = Main.inventoryBackTexture.Height * inventoryScale;
            Vector2 origin = GetDimensions().Position();
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = inventoryScale;
            Item[] temp = new Item[1];
            for (int k = 0; k < numColumns * numRows; k++)
            {
                Item item = GetItem(k);
                Vector2 drawPos = origin + new Vector2((slotWidth + padding) * (k % numColumns), (slotHeight + padding) * (k / numColumns));
                temp[0] = item;
                ItemSlot.Draw(Main.spriteBatch, temp, ItemSlot.Context.ChestItem, 0, drawPos);
            }
            Main.inventoryScale = oldScale;
        }

        public void DrawText()
        {
            if (hoverSlot >= 0)
            {
                Item hoverItem = GetItem(hoverSlot);
                if (!hoverItem.IsAir)
                {
                    Main.HoverItem = hoverItem.Clone();
                    Main.instance.MouseText(string.Empty);
                }
            }
        }
    }
}