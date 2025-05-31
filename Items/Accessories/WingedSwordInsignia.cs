using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace TerraRing.Items.Accessories
{
    internal class WingedSwordInsignia : ModItem
    {
        private const float TIER1_THRESHOLD = 17f;
        private const float TIER2_THRESHOLD = 30f;
        private const float TIER3_THRESHOLD = 45f;

        private const float TIER1_BONUS = 0.03f;
        private const float TIER2_BONUS = 0.05f;
        private const float TIER3_BONUS = 0.10f;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<TerraRingPlayer>();
            float accumuVal = modPlayer.GetAccumuVal();

            float damageBonus = 0f;
            if (accumuVal >= TIER3_THRESHOLD)
            {
                damageBonus = TIER3_BONUS;
            }
            else if (accumuVal >= TIER2_THRESHOLD)
            {
                damageBonus = TIER2_BONUS;
            }
            else if (accumuVal >= TIER1_THRESHOLD)
            {
                damageBonus = TIER1_BONUS;
            }

            player.GetDamage(DamageClass.Generic) += damageBonus;
            player.GetDamage(DamageClass.Magic) += damageBonus;

            if (!hideVisual && accumuVal >= TIER1_THRESHOLD && Main.rand.NextBool(20))
            {
                Color dustColor = accumuVal >= TIER3_THRESHOLD ? Color.Red :
                                accumuVal >= TIER2_THRESHOLD ? Color.Yellow :
                                Color.White;

                Dust.NewDust(player.position, player.width, player.height,
                            DustID.WhiteTorch, 0f, 0f, 150, dustColor, 0.8f);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer != null)
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                float accumuVal = modPlayer.GetAccumuVal();

                string currentTier = accumuVal >= TIER3_THRESHOLD ? "III" :
                                   accumuVal >= TIER2_THRESHOLD ? "II" :
                                   accumuVal >= TIER1_THRESHOLD ? "I" :
                                   "None";

                float currentBonus = accumuVal >= TIER3_THRESHOLD ? TIER3_BONUS :
                                   accumuVal >= TIER2_THRESHOLD ? TIER2_BONUS :
                                   accumuVal >= TIER1_THRESHOLD ? TIER1_BONUS :
                                   0f;

                TooltipLine bonusLine = new TooltipLine(Mod,
                    "WingedSwordBonus",
                    Language.GetTextValue("Mods.TerraRing.Items.WingedSwordInsignia.CurrentBonus",
                        currentTier, (currentBonus * 100).ToString("F0")));
                bonusLine.IsModifier = true;
                tooltips.Add(bonusLine);
            }
        }

        // NEED TO UPDATE FOR ITEM DROP
    }
}