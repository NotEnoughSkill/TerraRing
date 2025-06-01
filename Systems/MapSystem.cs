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
            int mapLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map"));
            if (mapLayer != -1)
            {
                layers.Insert(mapLayer + 1, new LegacyGameInterfaceLayer(
                    "TerraRing: Map Icons",
                    delegate
                    {
                        if (Main.mapFullscreen)
                        {
                            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                            if (modPlayer.MapTravelMode)
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

            if (!Main.mapFullscreen || !modPlayer.MapTravelMode) return;

            var iconTexture = ModContent.Request<Texture2D>("TerraRing/Items/Placeables/SiteOfGrace").Value;

            foreach (Point site in modPlayer.DiscoveredSitesOfGrace)
            {
                Vector2 mapPosition = new Vector2(site.X, site.Y) * 16;

                Vector2 screenPosition = (mapPosition - Main.mapFullscreenPos) * Main.mapFullscreenScale + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

                Rectangle mouseRect = new Rectangle(Main.mouseX - 5, Main.mouseY - 5, 10, 10);
                Rectangle iconRect = new Rectangle((int)screenPosition.X - 8, (int)screenPosition.Y - 8, 16, 16);
                bool isHovered = mouseRect.Intersects(iconRect);

                float scale = isHovered ? 1.2f : 1f;
                Color color = isHovered ? Color.Gold : Color.Yellow;
                float rotation = Main.GameUpdateCount * 0.1f;

                Main.spriteBatch.Draw(
                    iconTexture,
                    screenPosition,
                    null,
                    color,
                    rotation,
                    new Vector2(iconTexture.Width / 2f, iconTexture.Height / 2f),
                    scale * Main.mapFullscreenScale * 0.5f,
                    SpriteEffects.None,
                    0f
                );

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
    }
}