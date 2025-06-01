using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Terraria.ModLoader.Default;
using Terraria.ModLoader;


namespace TerraRing.Tiles
{
    internal class SiteOfGraceEntity : TEModdedPylon
    {
        public override bool IsTileValidForEntity(int i, int j)
        {
            var tile = Main.tile[i, j];
            return tile.HasTile && tile.TileType == ModContent.TileType<SiteOfGraceTile>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3, 3);
                NetMessage.SendData(87, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }
    }
}