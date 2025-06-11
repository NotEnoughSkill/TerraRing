using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraRing.Utilities;

namespace TerraRing.UI
{
    internal class SiteOfGraceUI : UIState
    {
        private UIPanel mainPanel;
        private UIText passTimeButton;
        private UIText levelUpButton;
        private UIText leaveButton;

        private Color defaultColor = new Color(255, 255, 255);
        private Color hoverColor = new Color(255, 207, 107);

        public override void OnInitialize()
        {
            mainPanel = new UIPanel();
            mainPanel.Width.Set(320f, 0f);
            mainPanel.Height.Set(180f, 0f);
            mainPanel.Left.Set(60f, 0f);
            mainPanel.Top.Set(140f, 0f);
            mainPanel.BackgroundColor = new Color(0, 0, 0) * 0.7f;
            mainPanel.BorderColor = Color.Transparent;

            var title = new UIText("Site of Grace", 1.2f, false);
            title.Left.Set(18f, 0f);
            title.Top.Set(16f, 0f);
            mainPanel.Append(title);

            passTimeButton = new UIText("Pass time", 1.1f, false);
            passTimeButton.Left.Set(36f, 0f);
            passTimeButton.Top.Set(56f, 0f);
            passTimeButton.TextColor = defaultColor;
            passTimeButton.OnMouseOver += (evt, el) => passTimeButton.TextColor = hoverColor;
            passTimeButton.OnMouseOut += (evt, el) => passTimeButton.TextColor = defaultColor;
            passTimeButton.OnLeftClick += PassTimeClick;
            mainPanel.Append(passTimeButton);

            levelUpButton = new UIText("Level Up", 1.1f, false);
            levelUpButton.Left.Set(36f, 0f);
            levelUpButton.Top.Set(56f + 38f, 0f);
            levelUpButton.TextColor = defaultColor;
            levelUpButton.OnMouseOver += (evt, el) => levelUpButton.TextColor = hoverColor;
            levelUpButton.OnMouseOut += (evt, el) => levelUpButton.TextColor = defaultColor;
            levelUpButton.OnLeftClick += LevelUpClick;
            mainPanel.Append(levelUpButton);

            leaveButton = new UIText("Leave", 1.1f, false);
            leaveButton.Left.Set(36f, 0f);
            leaveButton.Top.Set(56f + 38f * 2, 0f);
            leaveButton.TextColor = defaultColor;
            leaveButton.OnMouseOver += (evt, el) => leaveButton.TextColor = hoverColor;
            leaveButton.OnMouseOut += (evt, el) => leaveButton.TextColor = defaultColor;
            leaveButton.OnLeftClick += LeaveClick;
            mainPanel.Append(leaveButton);

            Append(mainPanel);
        }

        private void PassTimeClick(UIMouseEvent evt, UIElement listeningElement)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<TerraRingPlayer>();

            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = 0.2f });

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(player.position, player.width, player.height,
                    DustID.GoldFlame,
                    Scale: Main.rand.NextFloat(1f, 1.5f));
            }

            player.statLife = player.statLifeMax2;
            player.statMana = player.statManaMax2;
            modPlayer.CurrentFP = modPlayer.MaxFP;
            modPlayer.CurrentStamina = modPlayer.MaxStamina;

            for (int i = 0; i < player.buffType.Length; i++)
            {
                if (Main.debuff[player.buffType[i]])
                {
                    player.DelBuff(i);
                    i--;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!Main.dayTime) Main.dayTime = true;
                else
                {
                    Main.dayTime = false;
                    Main.time = 0;
                }

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.townNPC && !npc.friendly)
                    {
                        npc.active = false;
                    }
                }
            }

            if (player.statLife < player.statLifeMax2)
            {
                CombatText.NewText(player.getRect(),
                    new Color(100, 255, 100),
                    player.statLifeMax2 - player.statLife);
            }
        }

        private void LevelUpClick(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            TerraRingUI.Instance.ShowLevelUpUI();
        }

        private void LeaveClick(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            TerraRingUI.Instance.HideSiteOfGraceUI();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) &&
                !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                TerraRingUI.Instance.HideSiteOfGraceUI();
                return;
            }
        }
    }
}