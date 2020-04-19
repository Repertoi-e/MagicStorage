using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Terraria;

namespace MagicStoragePlus
{
    public class UISearchBar : UITextButton
    {
        public int MaxLength = 30;

        public bool UnfocusOnEnter = true;
        public bool UnfocusOnTab = true;

        public string HintText;

        public bool Focused { get; private set; }

        // Use this to filter certain characters or words
        public Func<string, bool> AllowTextEntered;

        public event Action OnFocus;
        public event Action OnUnfocus;
        public event Action OnTextChanged;
        public event Action OnTabPressed;
        public event Action OnEnterPressed;

        public UISearchBar(string hintText, float scale, float maxScale) : base(hintText, scale, maxScale)
        {
            HintText = hintText;
            Action += (bool rightClick, bool mouseInBounds) =>
            {
                if (mouseInBounds) Focus();
                else Unfocus();
            };
        }

        public void Focus()
        {
            if (!Focused)
            {
                Main.clrInput();
                Main.blockInput = true;
                Focused = true;

                if (Text == HintText)
                    SetText("");

                ScaleUpBasedOnHover = false;
                ScaledUp = true;

                OnFocus?.Invoke();
            }
        }

        public void Unfocus()
        {
            if (Focused)
            {
                Main.blockInput = false;
                Focused = false;

                if (Text.Length == 0)
                {
                    TextColor = Color.White;
                    base.SetText(HintText);
                }

                ScaleUpBasedOnHover = true;

                // Keep mouse in interface for 1 frame when we unfocus to prevent player from shooting items
                Player player = Main.player[Main.myPlayer];
                player.mouseInterface = true;

                OnUnfocus?.Invoke();
            }
        }

        public new void SetText(string text)
        {
            if (text.ToString().Length > MaxLength)
                text = text.ToString().Substring(0, MaxLength);
            if (AllowTextEntered != null)
            {
                if (AllowTextEntered.Invoke(text))
                {
                    base.SetText(text);
                    OnTextChanged?.Invoke();
                }
            }
            else
            {
                base.SetText(text);
                OnTextChanged?.Invoke();
            }

            if (text != HintText) TextColor = new Color(252, 180, 151);
        }

        bool JustPressed(Keys key) => Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);

        int BlinkerCount;
        bool BlinkerState;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Update();

            if (Focused)
            {
                Terraria.GameInput.PlayerInput.WritingText = true;
                Main.instance.HandleIME();

                string newText = Main.GetInputText(Text);
                if (newText != Text)
                {
                    SetText(newText);
                }

                if (JustPressed(Keys.Tab))
                {
                    if (UnfocusOnTab) Unfocus();
                    OnTabPressed?.Invoke();
                }

                if (JustPressed(Keys.Escape))
                {
                    if (UnfocusOnEnter) Unfocus();
                    OnEnterPressed?.Invoke();
                }

                if (++BlinkerCount >= 20)
                {
                    BlinkerState = !BlinkerState;
                    BlinkerCount = 0;
                }

                Main.instance.DrawWindowsIMEPanel(new Vector2(98f, Main.screenHeight - 36), 0f);
            }

            var display = Text;
            if (BlinkerState && Focused)
                display += "|";
            DrawWithText(spriteBatch, display);
        }
    }
}