using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraRing.Items.Placeables
{
    internal class SiteOfGrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 52;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Orange;
            Item.createTile = ModContent.TileType<Tiles.SiteOfGraceTile>();
        }
    }
}
