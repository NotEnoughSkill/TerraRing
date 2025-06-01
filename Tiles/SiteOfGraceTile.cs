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

            Main.tileNoAttach[Type] = true;
            Main.tileNoSunLight[Type] = true;
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
                g = 0.3f + intensity;
                b = 0.0f + intensity;
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
                Main.NewText($"Checking site at: {site.X}, {site.Y}", Color.Yellow);

                bool withinX = pylonPos.X >= site.X && pylonPos.X <= site.X + 2;
                bool withinY = pylonPos.Y >= site.Y && pylonPos.Y <= site.Y + 2;

                Main.NewText($"X bounds check: {site.X} <= {pylonPos.X} <= {site.X + 2}: {withinX}");
                Main.NewText($"Y bounds check: {site.Y} <= {pylonPos.Y} <= {site.Y + 2}: {withinY}");

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

                    if (isHovered)
                    {
                        mouseOverText = "Site of Grace";
                    }

                    return;
                }
            }

        }


        public override bool CanPlacePylon()
        {
            Main.NewText("CanPlacePylon called", Color.Yellow);
            return true;
        }

        public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
        {
            Main.NewText("ValidTeleportCheck_NPCCount called", Color.Yellow);
            return true;
        }

        public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
        {
            Main.NewText("ValidTeleportCheck_BiomeRequirements called", Color.Yellow);
            return true;
        }

        public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo)
        {
            Main.NewText("ValidTeleportCheck_AnyDanger called", Color.Yellow);
            return true;
        }
    }
}