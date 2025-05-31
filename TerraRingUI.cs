using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using TerraRing.UI;

namespace TerraRing
{
    internal class TerraRingUI : ModSystem
    {
        private Vector2 barPosition = new Vector2(570, 20);
        private int barWidth = 200;
        private int barHeight = 20;
        private int padding = 5;

        internal RuneCounter RuneCounter;
        private UserInterface _runeCounterInterface;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                RuneCounter = new RuneCounter();
                RuneCounter.Activate();
                _runeCounterInterface = new UserInterface();
                _runeCounterInterface.SetState(RuneCounter);
            }
        }

        public override void Unload()
        {
            RuneCounter = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!Main.gameMenu && !Main.dedServ) 
            {
                _runeCounterInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Resource Bar",
                    delegate
                    {
                        DrawResourceBar();
                        return true;
                    },
                    InterfaceScaleType.UI));

                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Rune Counter",
                    delegate
                    {
                        if (!Main.gameMenu)
                        {
                            _runeCounterInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        private void DrawResourceBar()
        {
            if (Main.gameMenu) {
                return;
            }

            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<TerraRingPlayer>();

            Vector2 fpPosition = barPosition;
            DrawResourceBar(fpPosition, modPlayer.CurrentFP, modPlayer.MaxFP, Color.Blue, "FP");

            Vector2 staminaPosition = barPosition + new Vector2(0, barHeight + padding);
            DrawResourceBar(staminaPosition, modPlayer.CurrentStamina, modPlayer.MaxStamina, Color.Green, "Stamina");
        }

        private void DrawResourceBar(Vector2 position, float current, float max, Color color, string label)
        {
            Rectangle backgroundRect = new Rectangle((int)position.X, (int)position.Y, barWidth, barHeight);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, backgroundRect, Color.Black * 0.5f);

            float fillAmount = MathHelper.Clamp(current / max, 0f, 1f);
            Rectangle fillRect = new Rectangle((int)position.X, (int)position.Y, (int)(barWidth * fillAmount), barHeight);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, color * 0.7f);

            Rectangle borderRect = new Rectangle((int)position.X - 1, (int)position.Y - 1, barWidth + 2, barHeight + 2);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(borderRect.X, borderRect.Y, borderRect.Width, 1), Color.White);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(borderRect.X, borderRect.Bottom - 1, borderRect.Width, 1), Color.White);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(borderRect.X, borderRect.Y, 1, borderRect.Height), Color.White);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(borderRect.Right - 1, borderRect.Y, 1, borderRect.Height), Color.White);

            string text = $"{label}: {(int)current}/{(int)max}";
            Vector2 textPosition = position + new Vector2(5, barHeight / 2 - 10);
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text,
                textPosition.X,
                textPosition.Y,
                Color.White,
                Color.Black,
                Vector2.Zero,
                1f);
        }
    }
}
