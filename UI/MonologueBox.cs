using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;

namespace TerraRing.UI
{

    public class MonologueBox : UIState
    {
        protected UIText dialogueText;
        protected UIPanel panel;
        protected UIText closeText;
        bool dragging = false;
        Vector2 offset;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(300f, 0f);
            panel.Top.Set(300f, 0f);
            panel.Width.Set(400f, 0f);
            panel.Height.Set(100f, 0f);
            panel.BackgroundColor = new Color(46, 43, 37);
            panel.OnLeftMouseDown += DragPanelStart;
            panel.OnLeftMouseUp += DragPanelEnd;


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
        protected void DragPanelStart(UIMouseEvent evt, UIElement listeningElement)
        {
            dragging = true;
            offset = new Vector2(evt.MousePosition.X - panel.Left.Pixels, evt.MousePosition.Y - panel.Top.Pixels);
        }

        protected void DragPanelEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            dragging = false;
        }
        public override void Update(GameTime gameTime)
        {
            
            MouseState mouse = Mouse.GetState();
            if (dragging)
            {
                panel.Left.Set(Main.mouseX - offset.X, 0f);
                panel.Top.Set(Main.mouseY - offset.Y, 0f);
                panel.Recalculate();
            }
        }
        public void SetMessage(string message)
        {
            dialogueText.SetText(message);
        }
    }
}