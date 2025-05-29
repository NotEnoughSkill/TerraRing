using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TerraRing.World;

namespace TerraRing.Systems
{
    internal class RoundtableSystem : ModSystem
    {
        public static void TryEnterRoundtable()
        {
            if (!SubworldSystem.IsActive<RoundtableHold>())
            {
                SubworldSystem.Enter<RoundtableHold>();
            }
        }

        public static void ExitRoundtable()
        {
            if (SubworldSystem.IsActive<RoundtableHold>())
            {
                SubworldSystem.Exit();
            }
        }
    }
}
