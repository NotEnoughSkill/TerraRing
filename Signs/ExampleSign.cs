using System.Collections.Generic;
using Terraria.ModLoader;

namespace TerraRing.Signs
{
    class ExampleSign : MessageSign
    {
        public override string Texture => $"TerraRing/Signs/MessageSign";
        public override List<string> SetText() => ["First Page", "Second Page"];
    }
    
    public class TestStyle : ModItem
    {
        public override string Texture => $"TerraRing/Signs/MessageSign";

        
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ExampleSign>());
        }
    }
    
}
