using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerraRing.UI;

namespace TerraRing.Signs
{
    public class MessageSign : ModTile
    {
        protected List<string> List_Of_Texts;
        protected int current_text_idx = -1;

        protected void IncrementText()
        {
            //If no elements do nothing!
            if (List_Of_Texts.Count == 0)
            {
                return;
            }
            current_text_idx++;
            if (current_text_idx == List_Of_Texts.Count)
            {
                current_text_idx = 0;
            }
        }

        public virtual List<string> SetText()
        {
            return new List<string>();
        }

        public override void SetStaticDefaults()
        {
            Main.tileSign[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileNoAttach[Type] = true;

            Main.editSign = false;
            Main.npcChatText = "";
            Main.npcChatCornerItem = 0;

            List_Of_Texts = SetText();
        }

        public override void MouseOver(int i, int j)
        {
            //If no elements do nothing!
            if (List_Of_Texts.Count == 0)
            {
                return;
            }

        }

        public override bool RightClick(int i, int j)
        {
            Main.playerInventory = false;
            IncrementText();
            MonologueUISystem system = ModContent.GetInstance<MonologueUISystem>();
            if (List_Of_Texts.Count == 0)
            {
                system.DialogueUIState.SetMessage("Default Text");
            }
            else
            {
                system.DialogueUIState.SetMessage(List_Of_Texts[current_text_idx]);
            }
            system.DialogueInterface.SetState(system.DialogueUIState);
            return true;
        }
    }
    
    public class MessageSignStyle : ModItem
    {
        public override string Texture => $"TerraRing/Signs/MessageSign";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<MessageSign>());
        }
        
    }
}