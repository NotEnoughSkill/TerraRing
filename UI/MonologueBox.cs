using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace TerraRing.UI
{
    
    public class MonologueBox : UIState
    {
        protected UIText dialogueText;
        protected UIPanel panel;
        protected UIText closeText;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(300f, 0f);
            panel.Top.Set(300f, 0f);
            panel.Width.Set(400f, 0f);
            panel.Height.Set(100f, 0f);
            panel.BackgroundColor = new Color(73, 94, 171);

            // Dialogue message
            dialogueText = new UIText("This is a message.");
            dialogueText.Top.Set(10f, 0f);
            dialogueText.Left.Set(10f, 0f);
            panel.Append(dialogueText);

            // Close "X" text (acts like a button)
            closeText = new UIText("X");
            closeText.Width.Set(20f, 0f);
            closeText.Height.Set(20f, 0f);
            closeText.Left.Set(panel.Width.Pixels - 30f, 0f);
            closeText.Top.Set(5f, 0f);
            closeText.TextColor = Color.Red;
            closeText.OnLeftClick += ClosePanel;

            panel.Append(closeText);
            Append(panel);
        }

        protected void ClosePanel(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<MonologueUISystem>().DialogueInterface.SetState(null);
        }

        public void SetMessage(string message)
        {
            dialogueText.SetText(message);
        }
    }
}