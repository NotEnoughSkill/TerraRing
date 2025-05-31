using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerraRing.UI
{
    internal class RuneCounter : UIState
    {
        private UIText runeText;
        private const float PADDING = 50f;
        private const float SCALE = 1.2f;

        public override void OnInitialize()
        {
            runeText = new UIText("0", SCALE);
            runeText.Width.Set(200, 0f);
            runeText.Height.Set(30, 0f);

            UpdatePosition();

            Append(runeText);
        }

        private void UpdatePosition()
        {
            if (runeText != null)
            {
                runeText.Left.Set(Main.screenWidth - 200 - PADDING, 0f);
                runeText.Top.Set(Main.screenHeight - 20 - PADDING, 0f);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Main.LocalPlayer != null && !Main.gameMenu)
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                string runeDisplay = modPlayer.CurrentRunes.ToString("N0");
                runeText.SetText(runeDisplay);

                UpdatePosition();

                if (runeText.IsMouseHovering)
                {
                    Main.instance.MouseText("Runes");
                }
            }
        }
    }
}