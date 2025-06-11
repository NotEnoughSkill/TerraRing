using Microsoft.Xna.Framework;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraRing.UI
{
    internal class StatsPanel : ModSystem
    {
        private Vector2 panelPosition;
        private readonly int panelWidth = 300;
        private readonly int panelHeight = 400;
        private readonly int padding = 15;
        private readonly int lineHeight = 22;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!Main.playerInventory)
            {
                return;
            }

            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Stats Panel",
                    delegate
                    {
                        DrawStatsPanel();
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        private void DrawStatsPanel()
        {
            if (!Main.playerInventory || Main.gameMenu) return;

            float scale = 0.9f;

            int scaledWidth = (int)(panelWidth * scale);
            int scaledHeight = (int)(panelHeight * scale);

            panelPosition = new Vector2(
                Main.screenWidth - scaledWidth - 210, 380
            );

            Rectangle panelRect = new Rectangle(
                (int)panelPosition.X,
                (int)panelPosition.Y,
                scaledWidth,
                scaledHeight
            );

            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                panelRect,
                Color.Black * 0.8f
            );

            DrawPanelBorder(panelRect);

            Vector2 headerPos = panelPosition + new Vector2(padding * scale, padding * scale);
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.DeathText.Value,
                "Character Status",
                headerPos.X,
                headerPos.Y,
                Color.Gold,
                Color.Black,
                Vector2.Zero,
                0.6f
            );

            Vector2 currentPos = panelPosition + new Vector2(padding * scale, (padding + 50) * scale);

            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
            DrawStatLine("Level", modPlayer.Stats.Level, ref currentPos, scale);

            DrawSectionHeader("Attributes", ref currentPos, scale);

            DrawStatLine("Vigor", modPlayer.Vigor, ref currentPos, scale);
            DrawStatLine("Mind", modPlayer.Mind, ref currentPos, scale);
            DrawStatLine("Endurance", modPlayer.Endurance, ref currentPos, scale);
            DrawStatLine("Strength", modPlayer.Strength, ref currentPos, scale);
            DrawStatLine("Dexterity", modPlayer.Dexterity, ref currentPos, scale);
            DrawStatLine("Intelligence", modPlayer.Intelligence, ref currentPos, scale);
            DrawStatLine("Faith", modPlayer.Faith, ref currentPos, scale);
            DrawStatLine("Arcane", modPlayer.Arcane, ref currentPos, scale);

            currentPos.Y += lineHeight * scale;

            DrawSectionHeader("Equipment", ref currentPos, scale);

            DrawStatLine("Equipment Load", $"{modPlayer.CurrentEquipLoad:F1}/{modPlayer.MaxEquipLoad:F1}", ref currentPos, scale);
        }

        private void DrawPanelBorder(Rectangle rect)
        {
            int borderWidth = 2;
            Color borderColor = Color.White * 0.7f;

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(rect.X - borderWidth, rect.Y - borderWidth, rect.Width + borderWidth * 2, borderWidth),
                borderColor);

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(rect.X - borderWidth, rect.Y + rect.Height, rect.Width + borderWidth * 2, borderWidth),
                borderColor);

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(rect.X - borderWidth, rect.Y - borderWidth, borderWidth, rect.Height + borderWidth * 2),
                borderColor);

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(rect.X + rect.Width, rect.Y - borderWidth, borderWidth, rect.Height + borderWidth * 2),
                borderColor);
        }

        private void DrawSectionHeader(string text, ref Vector2 position, float scale)
        {
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text,
                position.X,
                position.Y,
                Color.Gold,
                Color.Black,
                Vector2.Zero,
                scale
            );
            position.Y += lineHeight * scale;
        }

        private void DrawStatLine(string label, object value, ref Vector2 position, float scale)
        {
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                label,
                position.X,
                position.Y,
                Color.White,
                Color.Black,
                Vector2.Zero,
                scale);

            string valueText = value.ToString();
            float valueWidth = FontAssets.MouseText.Value.MeasureString(valueText).X * scale;
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                valueText,
                position.X + panelWidth * scale - padding * scale - valueWidth - 40 *
                scale,
                position.Y,
                Color.White,
                Color.Black,
                Vector2.Zero,
                scale);

            position.Y += lineHeight;
        }

        private void DrawSectionHeader(string text, ref Vector2 position)
        {
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text,
                position.X,
                position.Y,
                Color.Gold,
                Color.Black,
                Vector2.Zero,
                0.7f);
            position.Y += lineHeight;
        }
    }
}
