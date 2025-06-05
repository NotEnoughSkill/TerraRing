using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
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

namespace TerraRing.Systems
{
    internal class MapSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mapLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map / Minimap"));
            if (mapLayer != -1)
            {
                layers.Insert(mapLayer + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Map Icons",
                    delegate
                    {
                        if (Main.mapEnabled)
                        {
                            Main.spriteBatch.End();
                            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                                BlendState.AlphaBlend,
                                Main.DefaultSamplerState,
                                DepthStencilState.None,
                                RasterizerState.CullNone,
                                null,
                                Main.GameViewMatrix.TransformationMatrix);

                            DrawMapIcons();

                            Main.spriteBatch.End();
                            Main.spriteBatch.Begin();
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private void DrawMapIcons()
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();

            Vector2 mouseWorld = Vector2.Zero;
            if (Main.mapFullscreen)
            {
                mouseWorld = Main.mapFullscreenPos + new Vector2(Main.mouseX - Main.screenWidth / 2, Main.mouseY - Main.screenHeight / 2) / Main.mapFullscreenScale;
            }

            foreach (Point site in modPlayer.DiscoveredSitesOfGrace)
            {
                Vector2 mapPosition = new Vector2(site.X, site.Y) * 16;
                Vector2 screenPosition;

                if (Main.mapFullscreen)
                {
                    screenPosition = (mapPosition - Main.mapFullscreenPos) * Main.mapFullscreenScale + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                }
                else
                {
                    float minimapScale = 5f;
                    screenPosition = (mapPosition - Main.LocalPlayer.position) / minimapScale;
                    screenPosition += new Vector2(Main.miniMapX + Main.miniMapWidth / 2, Main.miniMapY + Main.miniMapHeight / 2);
                }

                Rectangle mouseRect = new Rectangle(Main.mouseX - 5, Main.mouseY - 5, 10, 10);
                Rectangle iconRect = new Rectangle((int)screenPosition.X - 8, (int)screenPosition.Y - 8, 16, 16);
                bool isHovered = mouseRect.Intersects(iconRect);

                float baseScale = isHovered ? 0.8f : 0.6f;
                float scale = baseScale;

                if (Main.mapFullscreen)
                {
                    scale *= MathHelper.Lerp(1f, 0.3f, (Main.mapFullscreenScale - 0.4f) / 2f);
                }
                else
                {
                    scale *= 0.5f;
                }

                Color color = isHovered ? Color.Gold : Color.Yellow;

                if (isHovered && Main.mapFullscreen)
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

                    if (Main.mouseLeft && Main.mouseLeftRelease && modPlayer.MapTravelMode)
                    {
                        modPlayer.TravelToSite(site);
                        Main.mapFullscreen = false;
                        modPlayer.MapTravelMode = false;
                        SoundEngine.PlaySound(SoundID.Item6);

                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 dustPosition = new Vector2(site.X * 16 + 24, site.Y * 16 + 24);
                            Dust.NewDust(dustPosition, 32, 32, DustID.GoldFlame,
                                Scale: Main.rand.NextFloat(1f, 1.5f));
                        }
                    }
                }
            }
        }
    }
}