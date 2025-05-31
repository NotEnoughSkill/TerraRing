using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraRing.Items
{
    internal class RuneItem : ModItem
    {
        public int RuneValue { get; set; }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
            Item.autoReuse = false;
            Item.consumable = true;
        }

        public override bool OnPickup(Player player)
        {
            var modPlayer = player.GetModPlayer<TerraRingPlayer>();
            modPlayer.AddRunes(RuneValue);

            CombatText.NewText(player.getRect(), new Color(255, 207, 107), RuneValue.ToString(), true);

            return true;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.4f, 0.3f, 0.1f);
        }

        public override bool ItemSpace(Player player)
        {
            var modPlayer = player.GetModPlayer<TerraRingPlayer>();
            modPlayer.AddRunes(RuneValue);
            CombatText.NewText(player.getRect(), new Color(255, 207, 107), RuneValue.ToString(), true);
            SoundEngine.PlaySound(SoundID.CoinPickup with { Volume = 0.5f, Pitch = 0.2f });
            Item.TurnToAir();
            return false;
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            gravity *= 0.5f;
            maxFallSpeed *= 0.5f;
        }

    }
}