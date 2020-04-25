using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace MagicStoragePlus
{
    class UICheckbox : UIText
    {
        public static Texture2D checkboxTexture = null;
        public static Texture2D checkmarkTexture;

        public event EventHandler OnSelectedChanged;

        bool selected = false;
        bool disabled = false;
        string hoverText;

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    OnSelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public UICheckbox(string text, string hoverText, float textScale = 1, bool large = false) : base(text, textScale, large)
        {
            if (checkboxTexture == null)
            {
                checkboxTexture = MagicStoragePlus.Instance.GetTexture("checkBox");
                checkmarkTexture = MagicStoragePlus.Instance.GetTexture("checkMark");
            }

            this.hoverText = hoverText;

            Left.Pixels += 20;
            text = "   " + text;
            SetText(text);
            OnClick += UICheckbox_onLeftClick;
            Recalculate();
        }

        void UICheckbox_onLeftClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (disabled) return;
            Selected = !Selected;
        }

        public void SetDisabled(bool disabled = true)
        {
            this.disabled = disabled;
            if (disabled)
                Selected = false;
            TextColor = disabled ? Color.Gray : Color.White;
        }

        public void SetHoverText(string hoverText)
        {
            this.hoverText = hoverText;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            CalculatedStyle innerDimensions = base.GetInnerDimensions();
            Vector2 pos = new Vector2(innerDimensions.X, innerDimensions.Y - 5);

            spriteBatch.Draw(checkboxTexture, pos, null, disabled ? Color.Gray : Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            if (Selected)
                spriteBatch.Draw(checkmarkTexture, pos, null, disabled ? Color.Gray : Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (IsMouseHovering)
            {
                Main.hoverItemName = hoverText;
            }
        }
    }
}
