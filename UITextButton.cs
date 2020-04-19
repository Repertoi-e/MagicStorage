using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.UI.Chat;
using Terraria.GameContent.UI.Elements;

namespace MagicStoragePlus
{
    public class UITextButton : UIText
    {
        public float MinScale, MaxScale;

        bool scaledUp;
        public bool ScaledUp
        {
            get { return scaledUp; }
            set
            {
                if (value)
                {
                    if (!scaledUp)
                        Main.PlaySound(12, -1, -1, 1, 1f, 0f);
                    scaledUp = true;
                }
                else
                {
                    scaledUp = false;
                }
            }
        }
        public bool ScaleUpBasedOnHover = true;
        public Action<bool, bool> Action;

        float scale;

        public UITextButton(string text, float minScale, float maxScale) : base(text)
        {
            TextColor = Color.White;

            MinScale = scale = minScale;
            MaxScale = maxScale;
        }

        public override bool ContainsPoint(Vector2 point)
        {
            var dim = GetInnerDimensions();
            var size = Main.fontMouseText.MeasureString(Text) * scale;
            float x = dim.X + (int)(size.X / 2f);
            return Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x - size.X / 2f, dim.Y - size.Y / 2f, size.X, size.Y);
        }

        public void Update()
        {
            var mouse = new Vector2(Main.mouseX, Main.mouseY);
            if (UI.LeftClicked || UI.RightClicked)
                Action?.Invoke(UI.RightClicked, ContainsPoint(mouse));
        }

        protected void DrawWithText(SpriteBatch spriteBatch, string text)
        {
            var dim = GetInnerDimensions();
            var size = Main.fontMouseText.MeasureString(text);
            float x = dim.X + (int)(size.X * scale / 2f);

            var color = TextColor * 0.97f * (1f - (255f - Main.mouseTextColor) / 255f * 0.5f);
            color.A = 255;

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, text, new Vector2(x, dim.Y), color, 0f, size / 2f, new Vector2(scale), -1f, 1.5f);

            size *= scale;

            if (ScaledUp)
            {
                scale += 0.05f;
                if (scale > MaxScale) scale = MaxScale;
            }
            else
            {
                scale -= 0.05f;
                if (scale < MinScale) scale = MinScale;
            }

            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x - size.X / 2f, dim.Y - size.Y / 2f, size.X, size.Y))
            {
                if (ScaleUpBasedOnHover) ScaledUp = true;
                Main.player[Main.myPlayer].mouseInterface = true;
            }
            else
            {
                if (ScaleUpBasedOnHover) ScaledUp = false;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Update();
            DrawWithText(spriteBatch, Text);
        }
    }
}
