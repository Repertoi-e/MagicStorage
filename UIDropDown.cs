using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.GameContent.UI.Elements;

namespace MagicStoragePlus
{
    public class UIDropDown : UITextButton
    {
        public int OptionsX = 10, OptionsY = 22;
        public int OptionDiffY = 24;

        public int CurrentOption { get; private set; } = 0;

        public bool Focused { get; private set; }

        public event Action OnFocus;
        public event Action OnUnfocus;

        public List<UITextButton> Options { get; private set; } = new List<UITextButton>();

        Color DefaultTextColor;
        UIText SelectedBullet = new UIText("•", 1);

        public UIDropDown(string hintText, float scale, float maxScale) : base(hintText, scale, maxScale)
        {
            Action += (bool rightClick, bool mouseInBounds) =>
            {
                if (!rightClick && mouseInBounds)
                {
                    if (Focused) Unfocus();
                    else Focus();
                }
            };
            Append(SelectedBullet);
        }

        public void AddOption(UITextButton o)
        {
            Options.Add(o);
            Append(o);

            DefaultTextColor = o.TextColor;
            o.Action += (bool rightClicked, bool mouseInBounds) =>
            {
                if (!rightClicked && mouseInBounds)
                {
                    Options[CurrentOption].TextColor = DefaultTextColor;
                    Options[CurrentOption].ScaleUpBasedOnHover = true;

                    CurrentOption = Options.FindIndex(x => x == o);
                    Debug.Assert(CurrentOption != -1);

                    Options[CurrentOption].ScaleUpBasedOnHover = false;
                    Options[CurrentOption].ScaledUp = true;
                }
            };
            o.Top.Set(OptionsY + (Options.Count - 1) * OptionDiffY, 0);
            Unfocus();

            o.Left.Set(-1000, 0);
        }

        public void Focus()
        {
            if (!Focused)
            {
                foreach (var o in Options)
                    o.Left.Set(OptionsX, 0);
                OnFocus?.Invoke();

                ScaleUpBasedOnHover = false;
                ScaledUp = true;

                Focused = true;
            }
        }

        public void Unfocus()
        {
            if (Focused)
            {
                foreach (var o in Options)
                    o.Left.Set(-10000, 0);
                OnUnfocus?.Invoke();

                ScaleUpBasedOnHover = true;

                Focused = false;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Update();
            DrawWithText(spriteBatch, Text);

            SelectedBullet.Left.Set(Focused ? 0 : -10000, 0);
            SelectedBullet.Top.Set(OptionsY + CurrentOption * OptionDiffY - 13, 0);
        }
    }
}