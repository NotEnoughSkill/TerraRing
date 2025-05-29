using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraRing.Systems;

namespace TerraRing.Items
{
    internal class RoundtableItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = 0;
            Item.rare = ItemRarityID.Quest;
            Item.UseSound = SoundID.Item6;
        }

        public override bool? UseItem(Player player)
        {
            RoundtableSystem.TryEnterRoundtable();
            return true;
        }
    }
}
