using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerraRing
{
    internal class TerraRingKeybinds : ModSystem
    {
        public static ModKeybind RollKey { get; private set; }
        public static ModKeybind ClearSitesKey { get; private set; }

        public override void Load()
        {
            RollKey = KeybindLoader.RegisterKeybind(Mod, "Roll", Keys.LeftShift);
            ClearSitesKey = KeybindLoader.RegisterKeybind(Mod, "Clear Sites of Grace", Keys.K);
        }

        public override void Unload()
        {
            RollKey = null;
            ClearSitesKey = null;
        }
    }
}
