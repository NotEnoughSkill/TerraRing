using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using static TerraRing.TerraRingPlayer;
using Microsoft.Xna.Framework;

namespace TerraRing.Systems
{
    internal class WeaponRequirementSystem : GlobalItem
    {
        private struct WeaponScaling
        {
            public ScalingGrade Strength;
            public ScalingGrade Dexterity;

            public WeaponScaling(ScalingGrade str, ScalingGrade dex)
            {
                Strength = str;
                Dexterity = dex;
            }
        }

        private static readonly Dictionary<string, WeaponScaling> WeaponTypeScaling = new()
        {
            {"Greatsword", new WeaponScaling(ScalingGrade.B, ScalingGrade.E)},
            {"Hammer", new WeaponScaling(ScalingGrade.A, ScalingGrade.E)},
            {"Axe", new WeaponScaling(ScalingGrade.B, ScalingGrade.D)},
        
            {"Broadsword", new WeaponScaling(ScalingGrade.D, ScalingGrade.D)},
            {"Spear", new WeaponScaling(ScalingGrade.D, ScalingGrade.C)},
            {"Mace", new WeaponScaling(ScalingGrade.C, ScalingGrade.E)},
        
            {"Shortsword", new WeaponScaling(ScalingGrade.E, ScalingGrade.C)},
            {"Katana", new WeaponScaling(ScalingGrade.E, ScalingGrade.B)},
            {"Dagger", new WeaponScaling(ScalingGrade.E, ScalingGrade.B)},
        
            {"Bow", new WeaponScaling(ScalingGrade.E, ScalingGrade.B)},
            {"Crossbow", new WeaponScaling(ScalingGrade.C, ScalingGrade.D)},
            {"Gun", new WeaponScaling(ScalingGrade.None, ScalingGrade.C)},
        
            {"Default", new WeaponScaling(ScalingGrade.D, ScalingGrade.D)}
        };

        private static readonly Dictionary<int, WeaponScaling> WeaponScalingData = new();

        public bool IsWeapon(Item item)
        {
            if (item.damage <= 0) return false;
            if (item.pick > 0 || item.axe > 0 || item.hammer > 0) return false;

            bool isValidDamageType = item.DamageType == DamageClass.Melee ||
                                item.DamageType == DamageClass.Ranged ||
                                item.DamageType == DamageClass.Magic ||
                                item.DamageType == DamageClass.Summon ||
                                item.DamageType.CountsAsClass(DamageClass.Melee) ||
                                item.DamageType.CountsAsClass(DamageClass.Ranged) ||
                                item.DamageType.CountsAsClass(DamageClass.Magic) ||
                                item.DamageType.CountsAsClass(DamageClass.Summon);

            if (item.type < ItemID.Count)
            {
                bool isValidUseStyle = item.useStyle == ItemUseStyleID.Swing ||
                                     item.useStyle == ItemUseStyleID.Thrust ||
                                     item.useStyle == ItemUseStyleID.Shoot;
                return isValidDamageType && isValidUseStyle;
            }

            return isValidDamageType && item.damage > 0;
        }

        public override void SetDefaults(Item item)
        {
            if (!IsWeapon(item)) return;
            AssignWeaponScaling(item);
        }

        private void AssignWeaponScaling(Item item)
        {
            string itemName = item.Name.ToLower();
            WeaponScaling scaling;

            if (item.damage <= 0) return;

            if (item.useTime >= 30 || itemName.Contains("great") || itemName.Contains("heavy"))
            {
                scaling = WeaponTypeScaling["Greatsword"];
            }
            else if (item.useTime <= 15 || itemName.Contains("quick") || itemName.Contains("fast"))
            {
                scaling = WeaponTypeScaling["Dagger"];
            }
            else if (itemName.Contains("katana"))
            {
                scaling = WeaponTypeScaling["Katana"];
            }
            else if (itemName.Contains("bow"))
            {
                scaling = WeaponTypeScaling["Bow"];
            }
            else if (itemName.Contains("gun"))
            {
                scaling = WeaponTypeScaling["Gun"];
            }
            else
            {
                scaling = WeaponTypeScaling["Default"];
            }

            WeaponScalingData[item.type] = scaling;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!IsWeapon(item))
                return;

            var player = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
            var (strReq, dexReq, intReq, faithReq, arcReq) = GetWeaponRequirements(item);
            var scaling = GetWeaponScaling(item);

            int insertIndex = tooltips.FindLastIndex(t =>
                t.Mod == "Terraria" && !t.Name.Contains("Prefix") && !t.Name.Contains("SetBonus"));
            if (insertIndex == -1) insertIndex = tooltips.Count;
            else insertIndex++;

            if (strReq > 0 || dexReq > 0 || intReq > 0 || faithReq > 0 || arcReq > 0)
            {
                tooltips.Insert(insertIndex++, new TooltipLine(Mod, "ReqHeader", "Requirements:")
                {
                    OverrideColor = new Color(255, 207, 107)
                });

                AddStatRequirement(tooltips, ref insertIndex, "Strength", strReq, player.Strength);
                AddStatRequirement(tooltips, ref insertIndex, "Dexterity", dexReq, player.Dexterity);
                AddStatRequirement(tooltips, ref insertIndex, "Intelligence", intReq, player.Intelligence);
                AddStatRequirement(tooltips, ref insertIndex, "Faith", faithReq, player.Faith);
                AddStatRequirement(tooltips, ref insertIndex, "Arcane", arcReq, player.Arcane);
            }

            if (scaling.Any(s => s.Value != ScalingGrade.None))
            {
                tooltips.Insert(insertIndex++, new TooltipLine(Mod, "ScalingHeader", "Attribute Scaling:")
                {
                    OverrideColor = new Color(255, 207, 107)
                });

                foreach (var (stat, grade) in scaling)
                {
                    if (grade != ScalingGrade.None)
                    {
                        tooltips.Insert(insertIndex++, new TooltipLine(Mod, $"{stat}Scaling", $"{stat}: {grade}")
                        {
                            OverrideColor = Color.White
                        });
                    }
                }
            }
        }

        private void AddStatRequirement(List<TooltipLine> tooltips, ref int index, string stat, int req, int playerStat)
        {
            if (req <= 0) return;

            bool meetReq = playerStat >= req;
            string text = $"{stat}: {req}";

            tooltips.Insert(index++, new TooltipLine(Mod, $"{stat}Req", text)
            {
                OverrideColor = meetReq ? Color.White : Color.Red
            });
        }

        public (int Str, int Dex, int Int, int Fth, int Arc) GetWeaponRequirements(Item item)
        {
            var baseScaling = WeaponScalingData.TryGetValue(item.type, out var scaling)
                ? scaling
                : new WeaponScaling(ScalingGrade.E, ScalingGrade.E);

            int baseDamage = item.damage;
            float damageMultiplier = baseDamage / 100f;

            int strReq = baseScaling.Strength switch
            {
                ScalingGrade.S => 40,
                ScalingGrade.A => 30,
                ScalingGrade.B => 20,
                ScalingGrade.C => 16,
                ScalingGrade.D => 12,
                ScalingGrade.E => 8,
                _ => 0
            };

            int dexReq = baseScaling.Dexterity switch
            {
                ScalingGrade.S => 40,
                ScalingGrade.A => 30,
                ScalingGrade.B => 20,
                ScalingGrade.C => 16,
                ScalingGrade.D => 12,
                ScalingGrade.E => 8,
                _ => 0
            };

            switch (item.type)
            {
                case ItemID.Zenith:
                    return (50,
                           50,
                           50,
                           50,
                           30);

                case ItemID.LastPrism:
                    return (15,
                           0,
                           60,
                           30,
                           25); 

                case ItemID.TerraBlade:
                case ItemID.InfluxWaver:
                    return (40,
                           25,
                           30,
                           0,
                           15);

                case ItemID.DayBreak:
                case ItemID.SolarEruption:
                    return (35,
                           20,
                           25,
                           0,
                           15);

                case ItemID.RainbowRod:
                case ItemID.NimbusRod:
                    return (0,
                           0,
                           35,
                           25,
                           20); 

                case ItemID.EmpressBlade:
                    return (30,
                           25,
                           0,
                           35,
                           20);

                case ItemID.VampireKnives:
                    return (0,
                           30,
                           0,
                           35,
                           20);

                case ItemID.MagicDagger:
                    return (20,
                           0,
                           35,
                           0,
                           15);

                default:
                    int str = 0, dex = 0, intel = 0, faith = 0, arc = 0;

                    if (item.DamageType.CountsAsClass(DamageClass.Melee))
                    {
                        str = Math.Min((int)(20 * damageMultiplier), 40);
                        dex = Math.Min((int)(15 * damageMultiplier), 25);
                    }
                    else if (item.DamageType.CountsAsClass(DamageClass.Ranged))
                    {
                        dex = Math.Min((int)(20 * damageMultiplier), 40);
                        str = Math.Min((int)(10 * damageMultiplier), 20);
                    }
                    else if (item.DamageType.CountsAsClass(DamageClass.Magic))
                    {
                        intel = Math.Min((int)(20 * damageMultiplier), 40);
                        arc = Math.Min((int)(10 * damageMultiplier), 20);
                    }
                    else if (item.DamageType.CountsAsClass(DamageClass.Summon))
                    {
                        faith = Math.Min((int)(20 * damageMultiplier), 40);
                        arc = Math.Min((int)(15 * damageMultiplier), 25);
                    }

                    return (str, dex, intel, faith, arc);
            }
        }

        public Dictionary<string, ScalingGrade> GetWeaponScaling(Item item)
        {
            var scaling = new Dictionary<string, ScalingGrade>()
            {
                {"Strength", ScalingGrade.None},
                {"Dexterity", ScalingGrade.None},
                {"Intelligence", ScalingGrade.None},
                {"Faith", ScalingGrade.None},
                {"Arcane", ScalingGrade.None}
            };

            var baseScaling = WeaponScalingData.TryGetValue(item.type, out var weaponScaling) ? weaponScaling : new WeaponScaling(ScalingGrade.E, ScalingGrade.E);

            switch (item.type)
            {
                case ItemID.TerraBlade:
                case ItemID.InfluxWaver:
                case ItemID.DayBreak:
                case ItemID.SolarEruption:
                    scaling["Strength"] = ScalingGrade.B;
                    scaling["Intelligence"] = ScalingGrade.C;
                    break;

                case ItemID.NorthPole:
                case ItemID.Frostbrand:
                    scaling["Strength"] = ScalingGrade.C;
                    scaling["Intelligence"] = ScalingGrade.C;
                    scaling["Arcane"] = ScalingGrade.D;
                    break;

                case ItemID.RainbowRod:
                case ItemID.NimbusRod:
                    scaling["Intelligence"] = ScalingGrade.B;
                    scaling["Faith"] = ScalingGrade.D;
                    break;

                case ItemID.EmpressBlade:
                case ItemID.StardustDragonStaff:
                    scaling["Strength"] = ScalingGrade.D;
                    scaling["Faith"] = ScalingGrade.A;
                    break;

                case ItemID.LaserMachinegun:
                case ItemID.ChargedBlasterCannon:
                    scaling["Intelligence"] = ScalingGrade.B;
                    scaling["Dexterity"] = ScalingGrade.C;
                    break;

                case ItemID.LastPrism:
                case ItemID.Zenith:
                    scaling["Strength"] = ScalingGrade.B;
                    scaling["Intelligence"] = ScalingGrade.B;
                    scaling["Faith"] = ScalingGrade.B;
                    scaling["Arcane"] = ScalingGrade.C;
                    break;

                default:
                    if (item.DamageType.CountsAsClass(DamageClass.Melee))
                    {
                        scaling["Strength"] = item.useTime >= 30 ? ScalingGrade.B : ScalingGrade.D;
                        scaling["Dexterity"] = item.useTime <= 20 ? ScalingGrade.C : ScalingGrade.E;
                    }
                    else if (item.DamageType.CountsAsClass(DamageClass.Ranged))
                    {
                        scaling["Dexterity"] = ScalingGrade.C;
                        scaling["Strength"] = ScalingGrade.E;
                    }
                    else if (item.DamageType.CountsAsClass(DamageClass.Magic))
                    {
                        scaling["Intelligence"] = ScalingGrade.B;
                        scaling["Arcane"] = ScalingGrade.D;
                    }
                    else if (item.DamageType.CountsAsClass(DamageClass.Summon))
                    {
                        scaling["Faith"] = ScalingGrade.B;
                        scaling["Arcane"] = ScalingGrade.C;
                    }
                    break;
            }

            return scaling;
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (!IsWeapon(item))
                return true;

            var modPlayer = player.GetModPlayer<TerraRingPlayer>();
            var (strReq, dexReq, intReq, faithReq, arcReq) = GetWeaponRequirements(item);

            bool meetsReq = (strReq == 0 || modPlayer.Strength >= strReq) &&
                           (dexReq == 0 || modPlayer.Dexterity >= dexReq) &&
                           (intReq == 0 || modPlayer.Intelligence >= intReq) &&
                           (faithReq == 0 || modPlayer.Faith >= faithReq) &&
                           (arcReq == 0 || modPlayer.Arcane >= arcReq);

            if (!meetsReq)
            {
                CombatText.NewText(player.Hitbox, Color.Red, "Requirements not met!", true);
                return false;
            }

            return true;
        }

        private int ClampStatRequirement(int requirement)
        {
            return Math.Min(requirement, 99);
        }
    }
}