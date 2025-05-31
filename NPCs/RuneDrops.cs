using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using TerraRing.Items;

namespace TerraRing.NPCs
{
    internal class RuneDrops : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            int runeValue = GetBaseRuneValue(npc);

            if (Main.hardMode)
            {
                runeValue = (int)(runeValue * 2.5f);
            }

            if (NPC.downedMoonlord)
            {
                runeValue = (int)(runeValue * 1.5f);
            }

            if (Main.expertMode)
            {
                runeValue = (int)(runeValue * 1.75f);
            }
            if (Main.masterMode)
            {
                runeValue = (int)(runeValue * 2f);
            }

            if (npc.boss)
            {
                runeValue *= 10;
            }

            if (runeValue > 0)
            {
                int runeItem = Item.NewItem(
                    npc.GetSource_Loot(),
                    (int)npc.position.X,
                    (int)npc.position.Y,
                    npc.width,
                    npc.height,
                    ModContent.ItemType<Items.RuneItem>()
                );

                if (Main.item[runeItem].ModItem is Items.RuneItem rune)
                {
                    rune.RuneValue = runeValue;
                }

                CombatText.NewText(npc.getRect(), new Color(255, 207, 107), runeValue.ToString(), true);
            }
        }

        private int GetBaseRuneValue(NPC npc)
        {
            // Pre-Hardmode Basic Enemies
            if (npc.type == NPCID.BlueSlime) return 10;
            if (npc.type == NPCID.Zombie) return 15;
            if (npc.type == NPCID.DemonEye) return 25;

            // Pre-Hardmode Strong Enemies
            if (npc.type == NPCID.CaveBat) return 35;
            if (npc.type == NPCID.Antlion) return 45;
            if (npc.type == NPCID.MotherSlime) return 50;

            // Pre-Hardmode Elite Enemies
            if (npc.type == NPCID.GoblinPeon) return 75;
            if (npc.type == NPCID.DarkCaster) return 100;
            if (npc.type == NPCID.ChaosElemental) return 150;

            // Hardmode Basic Enemies
            if (npc.type == NPCID.Werewolf) return 200;
            if (npc.type == NPCID.AngryNimbus) return 250;
            if (npc.type == NPCID.IchorSticker) return 300;

            // Hardmode Strong Enemies
            if (npc.type == NPCID.Mimic) return 500;
            if (npc.type == NPCID.RedDevil) return 750;
            if (npc.type == NPCID.Necromancer) return 1000;

            // Pre-Hardmode Bosses
            if (npc.type == NPCID.KingSlime) return 2000;
            if (npc.type == NPCID.EyeofCthulhu) return 3000;
            if (npc.type == NPCID.EaterofWorldsHead) return 1500;
            if (npc.type == NPCID.EaterofWorldsBody) return 1000;
            if (npc.type == NPCID.EaterofWorldsTail) return 1500;
            if (npc.type == NPCID.BrainofCthulhu) return 4000;
            if (npc.type == NPCID.QueenBee) return 5000;
            if (npc.type == NPCID.SkeletronHead) return 6000;
            if (npc.type == NPCID.WallofFlesh) return 10000;

            // Hardmode Bosses
            if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism) return 15000;
            if (npc.type == NPCID.TheDestroyer) return 15000;
            if (npc.type == NPCID.SkeletronPrime) return 15000;
            if (npc.type == NPCID.Plantera) return 25000;
            if (npc.type == NPCID.Golem) return 30000;
            if (npc.type == NPCID.DukeFishron) return 40000;
            if (npc.type == NPCID.CultistBoss) return 50000;
            if (npc.type == NPCID.MoonLordCore) return 100000;

            return (int)(npc.value * 0.1f);
        }
    }
}