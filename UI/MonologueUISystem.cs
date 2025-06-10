using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;


namespace TerraRing.UI
{
    public class MonologueUISystem : ModSystem
    {
        internal UserInterface DialogueInterface;
        internal MonologueBox DialogueUIState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                DialogueUIState = new MonologueBox();
                DialogueUIState.Activate();
                DialogueInterface = new UserInterface();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (DialogueInterface?.CurrentState != null)
                DialogueInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryLayerIndex != -1)
            {
                layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
                    "TestingStuff: Dialogue",
                    delegate
                    {
                        DialogueInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}