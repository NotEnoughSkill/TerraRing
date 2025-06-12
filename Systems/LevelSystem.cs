using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TerraRing.Systems
{
    internal class LevelSystem : ModSystem
    {
        public class PlayerStats
        {
            public const int MAX_STAT_VALUE = 99;
            public const int MAX_LEVEL = 713;

            public int Level { get; set; } = 1;
            public long Runes { get; set; } = 0;
            public Dictionary<StatType, int> Stats { get; private set; }

            public PlayerStats()
            {
                Stats = new Dictionary<StatType, int>()
                {
                    { StatType.Vigor, 10 },
                    { StatType.Mind, 10 },
                    { StatType.Endurance, 10 },
                    { StatType.Strength, 10 },
                    { StatType.Dexterity, 10 },
                    { StatType.Intelligence, 10 },
                    { StatType.Faith, 10 },
                    { StatType.Arcane, 10 },
                };
            }

            public int CalculateLevelUpCost()
            {
                return (int)(Math.Pow(Level, 1.2) * 100);
            }

            public bool TryLevelUpStat(StatType stat)
            {
                if (Level >= MAX_LEVEL)
                {
                    return false;
                }

                if (Stats[stat] >= MAX_STAT_VALUE)
                {
                    return false;
                }

                int cost = CalculateLevelUpCost();
                if (Runes >= cost)
                {
                    Runes -= cost;
                    Stats[stat] = Math.Min(Stats[stat] + 1, MAX_STAT_VALUE);
                    Level++;
                    return true;
                }
                return false;
            }

            public void AddRunes(long amount)
            {
                Runes += amount;
            }
        }

        public enum StatType
        {
            Vigor,
            Mind,
            Endurance,
            Strength,
            Dexterity,
            Intelligence,
            Faith,
            Arcane
        }
    }
}
