using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Terraria;
using Terraria.DataStructures;

using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.UI;
using MagicStoragePlus.Components;

namespace MagicStoragePlus
{
    public static class UI
    {
        public static float InventoryScale = 0.755f;
        public static Point16 TrashSlotOffset = new Point16(0, 0);

        public static bool LeftClicked => currentMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
        public static bool RightClicked => currentMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released;

        public static bool LeftReleased => currentMouse.LeftButton == ButtonState.Released;
        public static bool RightReleased => currentMouse.RightButton == ButtonState.Released;

        public static int ScrollWheelDelta => oldMouse.ScrollWheelValue - currentMouse.ScrollWheelValue;

        public static Vector2 Mouse => new Vector2(Main.mouseX, Main.mouseY);

        static FieldInfo itemIconCacheTimeInfo;
        static MouseState currentMouse, oldMouse;

        public static void Initialize()
        {
            itemIconCacheTimeInfo = typeof(Main).GetField("_itemIconCacheTime", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void HideItemIconCache()
        {
            itemIconCacheTimeInfo.SetValue(null, 0);
        }

        public static void Update()
        {
            oldMouse = currentMouse;
            currentMouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
        }

        public static void ShowStorage(bool crafting)
        {
            var i = MagicStoragePlus.Instance;
            i.StorageUI.Crafting = crafting;
            i.StorageUI.RefreshItems();
            i.StorageUI.UpdateUI();
            i.Interface.SetState(i.StorageUI);
        }

        public static void Hide()
        {
            MagicStoragePlus.Instance.Interface.SetState(null);
        }

        public static Rectangle GetFullRectangle(UIElement element)
        {
            Vector2 vector = new Vector2(element.GetDimensions().X, element.GetDimensions().Y);
            Vector2 position = new Vector2(element.GetDimensions().Width, element.GetDimensions().Height) + vector;

            vector = Vector2.Transform(vector, Main.UIScaleMatrix);
            position = Vector2.Transform(position, Main.UIScaleMatrix);

            Rectangle result = new Rectangle((int)vector.X, (int)vector.Y, (int)(position.X - vector.X), (int)(position.Y - vector.Y));
            int width = Main.spriteBatch.GraphicsDevice.Viewport.Width;
            int height = Main.spriteBatch.GraphicsDevice.Viewport.Height;

            result.X = Utils.Clamp(result.X, 0, width);
            result.Y = Utils.Clamp(result.Y, 0, height);
            result.Width = Utils.Clamp(result.Width, 0, width - result.X);
            result.Height = Utils.Clamp(result.Height, 0, height - result.Y);

            return result;
        }
    }
}