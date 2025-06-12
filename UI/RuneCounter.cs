using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerraRing.UI
{
    internal class RuneCounter : UIState
    {
        private Vector2 position;
        private Asset<Texture2D> runeIcon;
        private bool visible = true;

        public override void OnInitialize()
        {
            runeIcon = ModContent.Request<Texture2D>("TerraRing/UI/RuneIcon");
            position = new Vector2(Main.screenWidth - 200, Main.screenHeight - 50);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;

            var player = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
            string runeCount = player.Stats.Runes.ToString("N0");

            if (runeIcon?.Value == null) return;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(runeCount);
            float desiredIconHeight = textSize.Y * 0.9f;

            float iconScale = desiredIconHeight / runeIcon.Value.Height;
            int scaledIconWidth = (int)(runeIcon.Value.Width * iconScale);
            int scaledIconHeight = (int)(runeIcon.Value.Height * iconScale);

            int verticalPadding = 8;
            int horizontalPadding = 12;
            int bottomMargin = 25;
            int rightMargin = 20; 

            int totalWidth = (int)textSize.X + scaledIconWidth + (horizontalPadding * 2 + 10);
            int totalHeight = Math.Max(scaledIconHeight, (int)textSize.Y) + (verticalPadding * 2);

            position = new Vector2(
                Main.screenWidth - totalWidth - rightMargin,
                Main.screenHeight - totalHeight - bottomMargin
            );

            Rectangle backgroundRect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                totalWidth,
                totalHeight
            );

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, backgroundRect, null, new Color(0, 0, 0, 180));

            int borderWidth = 2;
            Color borderColor = new Color(255, 207, 107);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(backgroundRect.X, backgroundRect.Y, backgroundRect.Width, borderWidth),
                borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(backgroundRect.X, backgroundRect.Y + backgroundRect.Height - borderWidth, backgroundRect.Width, borderWidth),
                borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(backgroundRect.X, backgroundRect.Y, borderWidth, backgroundRect.Height),
                borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(backgroundRect.X + backgroundRect.Width - borderWidth, backgroundRect.Y, borderWidth, backgroundRect.Height),
                borderColor);

            Vector2 iconPosition = new Vector2(
                position.X + horizontalPadding,
                position.Y + (totalHeight - scaledIconHeight) / 2
            );

            spriteBatch.Draw(
                runeIcon.Value,
                iconPosition,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                iconScale,
                SpriteEffects.None,
                0f
            );

            Vector2 textPosition = new Vector2(
                iconPosition.X + scaledIconWidth + 10,
                position.Y + (totalHeight - textSize.Y) / 2
            );

            spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                runeCount,
                textPosition + new Vector2(2, 2),
                Color.Black * 0.5f
            );

            spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                runeCount,
                textPosition,
                new Color(255, 236, 179)
            );
        }
    }
}