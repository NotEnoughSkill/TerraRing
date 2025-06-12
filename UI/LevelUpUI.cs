using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraRing.Systems;


namespace TerraRing.UI
{
    internal class LevelUpUI : UIState
    {
        private UIPanel mainPanel;
        private UIPanel statsPanel;
        private UIPanel levelInfoPanel;
        private Dictionary<LevelSystem.StatType, StatRow> statRows;
        private UIText confirmButton;
        private UIText cancelButton;
        public Color defaultColor = new Color(255, 255, 255);
        public Color hoverColor = new Color(255, 207, 107);
        public Color increasedStatColor = new Color(146, 227, 169);
        public Color decreasedStatColor = new Color(227, 146, 146);

        public int pendingLevels = 0;

        public int pointsToAllocate;
        public Dictionary<LevelSystem.StatType, int> tempStats;

        public override void OnInitialize()
        {
            tempStats = new Dictionary<LevelSystem.StatType, int>();

            mainPanel = new UIPanel();
            mainPanel.Width.Set(800f, 0f);
            mainPanel.Height.Set(500f, 0f);
            mainPanel.HAlign = 0.5f;
            mainPanel.VAlign = 0.5f;
            mainPanel.BackgroundColor = new Color(0, 0, 0) * 0.95f;
            mainPanel.BorderColor = new Color(255, 207, 107) * 0.7f;

            levelInfoPanel = new UIPanel();
            levelInfoPanel.Width.Set(250f, 0f);
            levelInfoPanel.Height.Set(480f, 0f);
            levelInfoPanel.Left.Set(10f, 0f);
            levelInfoPanel.Top.Set(10f, 0f);
            levelInfoPanel.BackgroundColor = new Color(0, 0, 0, 0);
            levelInfoPanel.BorderColor = Color.Transparent;
            mainPanel.Append(levelInfoPanel);

            statsPanel = new UIPanel();
            statsPanel.Width.Set(500f, 0f);
            statsPanel.Height.Set(480f, 0f);
            statsPanel.Left.Set(270f, 0f);
            statsPanel.Top.Set(10f, 0f);
            statsPanel.BackgroundColor = new Color(0, 0, 0, 0);
            statsPanel.BorderColor = Color.Transparent;
            mainPanel.Append(statsPanel);

            InitializeStatRows();
            InitializeButtons();

            Append(mainPanel);
        }

        private void InitializeStatRows()
        {
            statRows = new Dictionary<LevelSystem.StatType, StatRow>();
            float currentY = 50f;

            foreach (LevelSystem.StatType statType in System.Enum.GetValues(typeof(LevelSystem.StatType)))
            {
                var row = new StatRow(statType, this);
                row.Top.Set(currentY, 0f);
                row.Left.Set(10f, 0f);
                row.Width.Set(480f, 0f);
                statsPanel.Append(row);
                statRows[statType] = row;
                currentY += 45f;
            }
        }

        private void InitializeButtons()
        {
            confirmButton = new UIText("Accept", 1.2f);
            confirmButton.Left.Set(300f, 0f);
            confirmButton.Top.Set(430f, 0f);
            confirmButton.TextColor = defaultColor;
            confirmButton.OnMouseOver += (evt, el) => confirmButton.TextColor = hoverColor;
            confirmButton.OnMouseOut += (evt, el) => confirmButton.TextColor = defaultColor;
            confirmButton.OnLeftClick += ConfirmLevelUp;
            statsPanel.Append(confirmButton);

            cancelButton = new UIText("Cancel", 1.2f);
            cancelButton.Left.Set(420f, 0f);
            cancelButton.Top.Set(430f, 0f);
            cancelButton.TextColor = defaultColor;
            cancelButton.OnMouseOver += (evt, el) => cancelButton.TextColor = hoverColor;
            cancelButton.OnMouseOut += (evt, el) => cancelButton.TextColor = defaultColor;
            cancelButton.OnLeftClick += CancelLevelUp;
            statsPanel.Append(cancelButton);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Main.LocalPlayer != null && !Main.gameMenu)
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                UpdateLevelInfo(modPlayer);
                UpdateStatRows(modPlayer);
            }

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) &&
                !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                TerraRingUI.Instance.HideLevelUpUI();
                return;
            }
        }

        private void UpdateLevelInfo(TerraRingPlayer player)
        {
            levelInfoPanel.RemoveAllChildren();

            float currentY = 20f;

            int previewLevel = player.Stats.Level + pendingLevels;
            long currentRunes = player.Stats.Runes;
            long totalCost = CalculateTotalCost(player.Stats.Level, pendingLevels);
            long remainingRunes = currentRunes - totalCost;

            AddInfoText("Level", previewLevel.ToString(), ref currentY,
                pendingLevels > 0 ? increasedStatColor : defaultColor);

            AddInfoText("Runes", Math.Max(0, remainingRunes).ToString("N0"), ref currentY,
                pendingLevels > 0 ? defaultColor : defaultColor);

            long nextLevelCost = CalculateNextLevelCost(previewLevel);
            AddInfoText("Next Level", nextLevelCost.ToString("N0"), ref currentY);

            currentY += 20f;

            foreach (LevelSystem.StatType statType in Enum.GetValues(typeof(LevelSystem.StatType)))
            {
                int currentValue = player.Stats.Stats[statType];
                int previewValue = tempStats.TryGetValue(statType, out int temp) ? temp : currentValue;

                AddInfoText(
                    statType.ToString(),
                    previewValue.ToString(),
                    ref currentY,
                    previewValue > currentValue ? increasedStatColor : defaultColor
                );
            }
        }

        private long CalculateTotalCost(int baseLevel, int levels)
        {
            long totalCost = 0;
            for (int i = 0; i < levels; i++)
            {
                totalCost += CalculateNextLevelCost(baseLevel + i);
            }
            return totalCost;
        }

        public long CalculateNextLevelCost(int level)
        {
            return (long)(Math.Pow(level, 1.2) * 100);
        }

        private void AddInfoText(string label, string value, ref float yPosition)
        {
            var text = new UIText($"{label}: {value}", 0.9f);
            text.Left.Set(10f, 0f);
            text.Top.Set(yPosition, 0f);
            levelInfoPanel.Append(text);
            yPosition += 25f;
        }

        private void UpdateStatRows(TerraRingPlayer player)
        {
            foreach (var statType in System.Enum.GetValues(typeof(LevelSystem.StatType)))
            {
                if (statRows.TryGetValue((LevelSystem.StatType)statType, out var row))
                {
                    row.UpdateValues(player);
                }
            }
        }

        private void ConfirmLevelUp(UIMouseEvent evt, UIElement listeningElement)
        {
            var player = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
            long totalCost = CalculateTotalCost(player.Stats.Level, pendingLevels);

            if (player.Stats.Runes >= totalCost)
            {
                foreach (var stat in tempStats)
                {
                    player.Stats.Stats[stat.Key] = stat.Value;
                }

                player.Stats.Level += pendingLevels;
                player.Stats.Runes -= totalCost;

                pendingLevels = 0;
                tempStats.Clear();

                SoundEngine.PlaySound(SoundID.Item37);

                UpdateAllUI(player);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }

        private void CancelLevelUp(UIMouseEvent evt, UIElement listeningElement)
        {
            TerraRingUI.Instance.HideLevelUpUI();
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        public void IncreaseStat(LevelSystem.StatType statType)
        {
            var player = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();

            long totalCost = CalculateTotalCost(player.Stats.Level, pendingLevels + 1);

            if (player.Stats.Runes >= totalCost)
            {
                if (!tempStats.ContainsKey(statType))
                {
                    tempStats[statType] = player.Stats.Stats[statType];
                }
                tempStats[statType]++;
                pendingLevels++;

                SoundEngine.PlaySound(SoundID.MenuTick);
                UpdateAllUI(player);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }

        public void DecreaseStat(LevelSystem.StatType statType)
        {
            var player = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
            if (tempStats.TryGetValue(statType, out int value))
            {
                if (value > player.Stats.Stats[statType])
                {
                    tempStats[statType]--;
                    pendingLevels--;

                    if (tempStats[statType] == player.Stats.Stats[statType])
                    {
                        tempStats.Remove(statType);
                    }

                    SoundEngine.PlaySound(SoundID.MenuTick);
                    UpdateAllUI(player);
                }
            }
        }
        private void UpdateAllUI(TerraRingPlayer player)
        {
            UpdateLevelInfo(player);

            foreach (var row in statRows.Values)
            {
                row.UpdateValues(player);
            }
        }

        private void AddInfoText(string label, string value, ref float yPosition, Color? color = null)
        {
            var text = new UIText($"{label}: {value}", 0.9f);
            text.Left.Set(10f, 0f);
            text.Top.Set(yPosition, 0f);
            if (color.HasValue)
            {
                text.TextColor = color.Value;
            }
            levelInfoPanel.Append(text);
            yPosition += 25f;
        }
    }

    class StatRow : UIElement
    {
        private readonly LevelSystem.StatType statType;
        private readonly LevelUpUI parent;
        private UIText statName;
        private UIText statValue;
        private UIText decreaseButton;
        private UIText increaseButton;
        private bool canIncrease;

        public StatRow(LevelSystem.StatType type, LevelUpUI levelUpUI)
        {
            statType = type;
            parent = levelUpUI;
            Width.Set(480f, 0f);
            Height.Set(40f, 0f);

            statName = new UIText(GetStatName(type), 1f);
            statName.Left.Set(10f, 0f);
            statName.Top.Set(10f, 0f);
            Append(statName);

            decreaseButton = new UIText("<", 1f);
            decreaseButton.Left.Set(200f, 0f);
            decreaseButton.Top.Set(10f, 0f);
            decreaseButton.OnLeftClick += (evt, el) =>
            {
                var player = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                if (parent.tempStats.TryGetValue(statType, out int value) && value > player.Stats.Stats[statType])
                {
                    parent.DecreaseStat(statType);
                }
            };
            Append(decreaseButton);

            statValue = new UIText("0", 1f);
            statValue.Left.Set(230f, 0f);
            statValue.Top.Set(10f, 0f);
            Append(statValue);

            increaseButton = new UIText(">", 1f);
            increaseButton.Left.Set(260f, 0f);
            increaseButton.Top.Set(10f, 0f);
            increaseButton.OnLeftClick += HandleIncreaseClick;
            Append(increaseButton);
        }

        private void HandleIncreaseClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (canIncrease)
            {
                parent.IncreaseStat(statType);
            }
        }

        public void UpdateValues(TerraRingPlayer player)
        {
            int currentValue = player.Stats.Stats[statType];
            int previewValue = parent.tempStats.TryGetValue(statType, out int temp) ? temp : currentValue;

            statValue.SetText(previewValue.ToString());

            if (previewValue > currentValue)
            {
                statValue.TextColor = parent.increasedStatColor;
            }
            else
            {
                statValue.TextColor = parent.defaultColor;
            }

            long nextLevelCost = parent.CalculateNextLevelCost(player.Stats.Level + parent.pendingLevels);
            canIncrease = player.Stats.Runes >= nextLevelCost;

            increaseButton.TextColor = canIncrease ? parent.defaultColor : Color.Gray * 0.5f;
            decreaseButton.TextColor = previewValue > currentValue ? parent.defaultColor : Color.Gray * 0.5f;
        }

        private string GetStatName(LevelSystem.StatType type)
        {
            return type.ToString();
        }
    }
}