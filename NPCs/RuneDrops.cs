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
            if (npc.type == NPCID.GreenSlime) return 10;
            if (npc.type == NPCID.RedSlime) return 12;
            if (npc.type == NPCID.YellowSlime) return 12;
            if (npc.type == NPCID.PurpleSlime) return 15;
            if (npc.type == NPCID.Pinky) return 100;
            if (npc.type == NPCID.Slimer) return 25;
            if (npc.type == NPCID.Zombie) return 15;
            if (npc.type == NPCID.ZombieRaincoat) return 20;
            if (npc.type == NPCID.BloodZombie) return 25;
            if (npc.type == NPCID.DemonEye) return 20;
            if (npc.type == NPCID.WanderingEye) return 25;
            if (npc.type == NPCID.Skeleton) return 20;
            if (npc.type == NPCID.Vulture) return 25;

            // Pre-Hardmode Mid Enemies
            if (npc.type == NPCID.ArmoredSkeleton) return 45;
            if (npc.type == NPCID.AngryBones) return 40;
            if (npc.type == NPCID.GraniteGolem) return 60;
            if (npc.type == NPCID.Salamander) return 45;
            if (npc.type == NPCID.JungleBat) return 35;
            if (npc.type == NPCID.ManEater) return 50;
            if (npc.type == NPCID.Snatcher) return 55;
            if (npc.type == NPCID.GiantShelly) return 60;
            if (npc.type == NPCID.Crawdad) return 50;
            if (npc.type == NPCID.Moth) return 40;
            if (npc.type == NPCID.UndeadViking) return 65;
            if (npc.type == NPCID.SnowFlinx) return 45;
            if (npc.type == NPCID.Wolf) return 50;

            // Pre-Hardmode Strong Enemies
            if (npc.type == NPCID.CaveBat) return 35;
            if (npc.type == NPCID.Antlion) return 45;
            if (npc.type == NPCID.MotherSlime) return 50;
            if (npc.type == NPCID.IceElemental) return 100;
            if (npc.type == NPCID.RuneWizard) return 150;
            if (npc.type == NPCID.IceGolem) return 125;
            if (npc.type == NPCID.Corruptor) return 85;
            if (npc.type == NPCID.Clinger) return 90;
            if (npc.type == NPCID.IcyMerman) return 100;
            if (npc.type == NPCID.Reaper) return 120;
            if (npc.type == NPCID.Wraith) return 100;
            if (npc.type == NPCID.Drippler) return 85;
            if (npc.type == NPCID.LostGirl) return 500;
            if (npc.type == NPCID.Nymph) return 500;

            // Pre-Hardmode Elite Enemies
            if (npc.type == NPCID.GoblinPeon) return 75;
            if (npc.type == NPCID.DarkCaster) return 100;
            if (npc.type == NPCID.ChaosElemental) return 150;

            // Hardmode Basic Enemies
            if (npc.type == NPCID.Werewolf) return 200;
            if (npc.type == NPCID.AngryNimbus) return 250;
            if (npc.type == NPCID.IchorSticker) return 300;
            if (npc.type == NPCID.Gastropod) return 200;
            if (npc.type == NPCID.PossessedArmor) return 175;
            if (npc.type == NPCID.ArmoredViking) return 200;
            if (npc.type == NPCID.GiantBat) return 150;
            if (npc.type == NPCID.DiggerBody) return 175;
            if (npc.type == NPCID.DiggerHead) return 175;
            if (npc.type == NPCID.DiggerTail) return 175;
            if (npc.type == NPCID.DuneSplicerBody) return 225;
            if (npc.type == NPCID.DuneSplicerHead) return 225;
            if (npc.type == NPCID.DuneSplicerTail) return 225;
            if (npc.type == NPCID.RockGolem) return 250;

            // Hardmode Strong Enemies
            if (npc.type == NPCID.Mimic) return 500;
            if (npc.type == NPCID.RedDevil) return 750;
            if (npc.type == NPCID.Necromancer) return 1000;
            if (npc.type == NPCID.Derpling) return 350;
            if (npc.type == NPCID.MossHornet) return 300;
            if (npc.type == NPCID.GiantTortoise) return 400;
            if (npc.type == NPCID.GiantFungiBulb) return 375;
            if (npc.type == NPCID.AnomuraFungus) return 350;
            if (npc.type == NPCID.CursedHammer) return 450;
            if (npc.type == NPCID.EnchantedSword) return 500;
            if (npc.type == NPCID.CrimsonAxe) return 450;
            if (npc.type == NPCID.Mothron) return 600;

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
            if (npc.type == NPCID.QueenSlimeBoss) return 20000;
            if (npc.type == NPCID.Plantera) return 25000;
            if (npc.type == NPCID.Golem) return 30000;
            if (npc.type == NPCID.DukeFishron) return 40000;
            if (npc.type == NPCID.HallowBoss) return 50000;
            if (npc.type == NPCID.CultistBoss) return 50000;
            if (npc.type == NPCID.MoonLordCore) return 100000;

            // Event Bosses
            if (npc.type == NPCID.Pumpking) return 15000;
            if (npc.type == NPCID.MourningWood) return 10000;
            if (npc.type == NPCID.Everscream) return 10000;
            if (npc.type == NPCID.SantaNK1) return 12000;
            if (npc.type == NPCID.IceQueen) return 15000;

            // Dungeon Defenders
            if (npc.type == NPCID.Paladin) return 1000;
            if (npc.type == NPCID.NecromancerArmored) return 600;
            if (npc.type == NPCID.DiabolistWhite) return 600;
            if (npc.type == NPCID.DiabolistRed) return 600;
            if (npc.type == NPCID.TacticalSkeleton) return 500;
            if (npc.type == NPCID.SkeletonCommando) return 550;
            if (npc.type == NPCID.SkeletonSniper) return 550;
            if (npc.type == NPCID.SkeletonArcher) return 500;

            // Special Enemies
            if (npc.type == NPCID.Mimic) return 750;
            if (npc.type == NPCID.IceMimic) return 800;
            if (npc.type == NPCID.BigMimicCorruption) return 1000;
            if (npc.type == NPCID.BigMimicCrimson) return 1000;
            if (npc.type == NPCID.BigMimicHallow) return 1000;
            if (npc.type == NPCID.BigMimicJungle) return 1200;
            if (npc.type == NPCID.WyvernHead) return 2000;
            if (npc.type == NPCID.WyvernLegs) return 2000;
            if (npc.type == NPCID.WyvernTail) return 2000;

            // Event Minibosses
            if (npc.type == NPCID.PirateShip) return 3000;
            if (npc.type == NPCID.PirateCaptain) return 2000;
            if (npc.type == NPCID.MartianSaucer) return 4000;
            if (npc.type == NPCID.MartianEngineer) return 2000;
            if (npc.type == NPCID.MartianOfficer) return 2500;

            // Lunar Events
            if (npc.type == NPCID.SolarCorite) return 3000;
            if (npc.type == NPCID.NebulaBrain) return 3000;
            if (npc.type == NPCID.VortexHornet) return 3000;
            if (npc.type == NPCID.StardustJellyfishBig) return 3000;
            if (npc.type == NPCID.LunarTowerSolar) return 5000;
            if (npc.type == NPCID.LunarTowerVortex) return 5000;
            if (npc.type == NPCID.LunarTowerNebula) return 5000;
            if (npc.type == NPCID.LunarTowerStardust) return 5000;

            return (int)(npc.value * 0.1f);
        }
    }
}