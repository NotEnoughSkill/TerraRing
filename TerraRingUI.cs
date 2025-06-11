using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraRing.UI;

namespace TerraRing
{
    internal class TerraRingUI : ModSystem
    {
        public static TerraRingUI Instance { get; private set; }

        private Vector2 barPosition = new Vector2(570, 20);
        private int barWidth = 200;
        private int barHeight = 20;
        private int padding = 5;

        internal RuneCounter RuneCounter;
        private UserInterface _runeCounterInterface;

        internal UserInterface SiteOfGraceInterface;
        internal SiteOfGraceUI SiteOfGraceUI;

        internal UserInterface LevelUpInterface;
        internal LevelUpUI LevelUpUI;

        public override void Load()
        {
            Instance = this;
            if (!Main.dedServ)
            {
                RuneCounter = new RuneCounter();
                RuneCounter.Activate();
                _runeCounterInterface = new UserInterface();
                _runeCounterInterface.SetState(RuneCounter);

                SiteOfGraceInterface = new UserInterface();
                SiteOfGraceUI = new SiteOfGraceUI();

                LevelUpInterface = new UserInterface();
                LevelUpUI = new LevelUpUI();
                LevelUpUI.Activate();
            }
        }

        public override void Unload()
        {
            Instance = null;
            RuneCounter = null;
            SiteOfGraceInterface = null;
            SiteOfGraceUI = null;
            LevelUpInterface = null;
            LevelUpUI = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!Main.gameMenu && !Main.dedServ)
            {
                _runeCounterInterface?.Update(gameTime);
                SiteOfGraceInterface?.Update(gameTime);
                LevelUpInterface?.Update(gameTime);
            }
        }

        public void HideSiteOfGraceUI()
        {
            SiteOfGraceInterface?.SetState(null);
            Main.blockInput = false;
        }

        public void ShowSiteOfGraceUI()
        {
            if (SiteOfGraceInterface?.CurrentState != SiteOfGraceUI)
            {
                SiteOfGraceInterface?.SetState(SiteOfGraceUI);
            }
        }

        public void ShowLevelUpUI()
        {
            if (LevelUpInterface?.CurrentState != LevelUpUI)
            {
                LevelUpInterface?.SetState(LevelUpUI);
            }
        }

        public void HideLevelUpUI()
        {
            LevelUpInterface?.SetState(null);
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

                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Level Up UI",
                    delegate
                    {
                        if (!Main.gameMenu)
                        {
                            LevelUpInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }

            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "TerraRing: Site of Grace UI",
                    delegate
                    {
                        SiteOfGraceInterface?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }

            int mapIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map"));
            if (mapIndex != -1)
            {
                layers.Insert(mapIndex + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Map Icons",
                    delegate
                    {
                        if (Main.mapFullscreen)
                        {
                            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                            if (modPlayer.MapTravelMode)
                            {
                                DrawMapIcons();
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.Game)
                );
            }
        }

        private void DrawMapIcons()
        {
            if (!Main.mapFullscreen) return;

            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();

            foreach (Point site in modPlayer.DiscoveredSitesOfGrace)
            {
                Vector2 mapPosition = new Vector2(site.X, site.Y) * 16;
                Vector2 screenPosition = (mapPosition - Main.mapFullscreenPos) * Main.mapFullscreenScale + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

                Rectangle mouseRect = new Rectangle(Main.mouseX - 5, Main.mouseY - 5, 10, 10);
                Rectangle iconRect = new Rectangle((int)screenPosition.X - 8, (int)screenPosition.Y - 8, 16, 16);
                bool isHovered = mouseRect.Intersects(iconRect);

                float scale = isHovered ? 1.2f : 1f;
                Color color = isHovered ? Color.Gold : Color.Yellow;

                if (isHovered)
                {
                    Utils.DrawBorderString(
                        Main.spriteBatch,
                        "Site of Grace",
                        screenPosition + new Vector2(0, -20),
                        Color.White,
                        1f,
                        0.5f,
                        0.5f
                    );

                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        modPlayer.TravelToSite(site);
                    }
                }
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