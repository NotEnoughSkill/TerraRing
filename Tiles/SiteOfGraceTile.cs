using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using TerraRing.UI;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.UI;

namespace TerraRing.Tiles
{
    internal class SiteOfGraceTile : ModPylon
    {
        public Asset<Texture2D> mapIcon;

        public override void Load()
        {
            mapIcon = ModContent.Request<Texture2D>("TerraRing/Tiles/SiteOfGraceTile_MapIcon", AssetRequestMode.ImmediateLoad);
        }

        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;

            Main.tileMergeDirt[Type] = true;
            TileID.Sets.IsValidSpawnPoint[Type] = true;

            TileObjectData.addTile(Type);

            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;
            TileID.Sets.AvoidedByMeteorLanding[Type] = true;

            AnimationFrameHeight = 54;
            DustType = -1;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<Items.Placeables.SiteOfGrace>();
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            var modPlayer = player.GetModPlayer<TerraRingPlayer>();

            int left = i - (tile.TileFrameX / 18);
            int top = j - (tile.TileFrameY / 18);

            if (tile.TileFrameY == 0)
            {
                player.FindSpawn();
                if (player.SpawnX != left || player.SpawnY != top)
                {
                    player.ChangeSpawn(left, top);
                    Main.NewText("Site of Grace discovered", 255, 240, 20);
                }
            }

            modPlayer.DiscoverSiteOfGrace(new Point(left, top));

            if (Main.playerInventory)
            {
                Main.playerInventory = false;
            }

            TerraRingUI.Instance.ShowSiteOfGraceUI();
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Point16 origin = new Point16(i - frameX / 18, j - frameY / 18);
            ModContent.GetInstance<SiteOfGraceEntity>().Kill(origin.X, origin.Y);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && !effectOnly)
            {
                var player = Main.LocalPlayer;
                var modPlayer = player.GetModPlayer<TerraRingPlayer>();
                modPlayer.RemoveSiteOfGrace(new Point(i, j));
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.mapEnabled)
            {
                Vector2 position = new Vector2(i * 16, j * 16);
                spriteBatch.Draw(mapIcon.Value, position, Color.White);
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameY < 36)
            {
                float intensity = Main.rand.Next(28, 42) * 0.005f + (270 - Main.mouseTextColor) / 700f;
                r = 1f + intensity;
                g = 0.8f + intensity;
                b = 0.3f + intensity;
            }
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            ModContent.GetInstance<SiteOfGraceEntity>().Place(i, j);
        }

        public override void DrawMapIcon(
        ref MapOverlayDrawContext context,
        ref string mouseOverText,
        TeleportPylonInfo pylonInfo,
        bool isNearPylon,
        Color drawColor,
        float deselectedScale,
        float selectedScale)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();

            Point pylonPos = new Point(pylonInfo.PositionInTiles.X, pylonInfo.PositionInTiles.Y);

            foreach (var site in modPlayer.DiscoveredSitesOfGrace)
            {
                bool withinX = pylonPos.X >= site.X && pylonPos.X <= site.X + 2;
                bool withinY = pylonPos.Y >= site.Y && pylonPos.Y <= site.Y + 2;

                if (withinX && withinY)
                {
                    Vector2 position = new Vector2(site.X + 1.5f, site.Y + 1.5f);

                    bool isHovered = context.Draw(
                        mapIcon.Value,
                        position,
                        drawColor,
                        new SpriteFrame(1, 1),
                        deselectedScale,
                        selectedScale,
                        Terraria.UI.Alignment.Center
                    ).IsMouseOver;

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

                    if (isHovered)
                    {
                        mouseOverText = "Site of Grace";
                        DefaultMapClickHandle(isHovered, pylonInfo, "Site of Grace", ref mouseOverText);
                    }

                    return;
                }
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.rand.NextBool(2))
            {
                CreateAmbientParticle(i, j);
            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer && Main.rand.NextBool(15))
            {
                CreateAmbientParticle(i, j);
            }
        }

        private void CreateAmbientParticle(int i, int j)
        {
            int left = i - (Main.tile[i, j].TileFrameX / 18);
            int top = j - (Main.tile[i, j].TileFrameY / 18);

            Vector2 center = new Vector2((left + 1.5f) * 16, (top + 1.5f) * 16);

            Vector2 dustPos = center + Main.rand.NextVector2Circular(24f, 24f);

            Vector2 velocity = new Vector2(
                Main.rand.NextFloat(-0.3f, 0.3f),
                Main.rand.NextFloat(-0.5f, -0.1f)
            );

            int dustType = Main.rand.NextFromList(
                DustID.GoldFlame,
                DustID.GoldCoin,
                DustID.GoldCoin
            );

            Dust dust = Dust.NewDustPerfect(
                dustPos,
                dustType,
                velocity,
                0,
                new Color(255, 207, 107) * 0.6f,
                Main.rand.NextFloat(0.3f, 0.5f)
            );

            dust.noGravity = true;
            dust.noLight = false;
            dust.fadeIn = 1.2f;
        }

        public override bool CanPlacePylon()
        {
            return true;
        }

        public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
        {
            return true;
        }

        public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
        {
            return true;
        }

        public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo)
        {
            return true;
        }

        public override void ModifyTeleportationPosition(TeleportPylonInfo destinationPylonInfo, ref Vector2 teleportationPosition)
        {
            teleportationPosition.X += 24f;
            teleportationPosition.Y -= 32f;

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(teleportationPosition, 16, 16, DustID.GoldFlame,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 0f),
                    Scale: Main.rand.NextFloat(1f, 1.5f));
            }
        }

        public override void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey)
        {
            destinationPylonValid = true;
        }

        public override void ValidTeleportCheck_NearbyPostCheck(TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool anyNearbyValidPylon, ref string errorKey)
        {
            anyNearbyValidPylon = true;
            destinationPylonValid = true;
        }

        public TeleportPylonType PylonType => TeleportPylonType.SurfacePurity;
    }
}