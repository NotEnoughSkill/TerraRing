using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
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
        private UIText levelUpButton;
        private UIText restButton;
        private UIText travelButton;
        private Color defaultColor = new Color(255, 255, 255);
        private Color hoverColor = new Color(255, 207, 107);

        public override void OnInitialize()
        {
            mainPanel = new UIPanel();
            mainPanel.Width.Set(300f, 0f);
            mainPanel.Height.Set(200f, 0f);
            mainPanel.HAlign = 0.5f;
            mainPanel.VAlign = 0.5f;
            mainPanel.BackgroundColor = new Color(73, 94, 171) * 0.85f;

            levelUpButton = new UIText("Level Up", 1.1f);
            levelUpButton.HAlign = 0.5f;
            levelUpButton.Top.Set(50f, 0f);
            levelUpButton.OnMouseOver += (evt, element) => levelUpButton.TextColor = hoverColor;
            levelUpButton.OnMouseOut += (evt, element) => levelUpButton.TextColor = defaultColor;
            levelUpButton.OnLeftClick += LevelUpClick;
            mainPanel.Append(levelUpButton);

            restButton = new UIText("Rest", 1.1f);
            restButton.HAlign = 0.5f;
            restButton.Top.Set(90f, 0f);
            restButton.OnMouseOver += (evt, element) => restButton.TextColor = hoverColor;
            restButton.OnMouseOut += (evt, element) => restButton.TextColor = defaultColor;
            restButton.OnLeftClick += RestClick;
            mainPanel.Append(restButton);

            travelButton = new UIText("Travel", 1.1f);
            travelButton.HAlign = 0.5f;
            travelButton.Top.Set(130f, 0f);
            travelButton.OnMouseOver += (evt, element) => travelButton.TextColor = hoverColor;
            travelButton.OnMouseOut += (evt, element) => travelButton.TextColor = defaultColor;
            travelButton.OnLeftClick += TravelClick;
            mainPanel.Append(travelButton);

            Append(mainPanel);
        }

        private void LevelUpClick(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            // TODO: Implement level up menu
        }

        private void RestClick(UIMouseEvent evt, UIElement listeningElement)
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

            TerraRingUI.Instance.HideSiteOfGraceUI();

            CombatText.NewText(player.getRect(),
                new Color(255, 207, 107),
                "Rested at Site of Grace",
                true);

            Timer.Start(action: () => {
                if (modPlayer.IsAtSiteOfGrace)
                    TerraRingUI.Instance.ShowSiteOfGraceUI();
            }, delayInFrames: 60);
        }

        private void TravelClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (evt.Target != listeningElement) return;

            SoundEngine.PlaySound(SoundID.MenuTick);

            TerraRingUI.Instance.HideSiteOfGraceUI();

            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
            modPlayer.MapTravelMode = true;
            Main.mapEnabled = true;
            Main.mapStyle = 1;
            Main.mapFullscreen = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) &&
                !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                TerraRingUI.Instance.HideSiteOfGraceUI();
                modPlayer.MapTravelMode = false;
                Main.mapFullscreen = false;
                return;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Main.LocalPlayer.mouseInterface = true;

            if (Main.rand.NextBool(10))
            {
                Vector2 position = Main.MouseScreen + Main.rand.NextVector2Circular(100f, 100f);
                Dust.NewDustPerfect(position, DustID.GoldFlame, Vector2.Zero, 200, default, 0.8f);
            }
        }
    }
}