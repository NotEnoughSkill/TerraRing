using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraRing
{
    internal class TerraRingPlayer : ModPlayer
    {
        #region Stats
        public int Vigor;
        public int Mind;
        public int Endurance;
        public int Strength;
        public int Dexterity;
        public int Intelligence;
        public int Faith;
        public int Arcane;

        public float MaxHP => 300 + (40 * Vigor);
        public float MaxFP => 50 + (12 * Mind);
        public float MaxStamina => 80 + (15 * Endurance);
        public float MaxEquipLoad => 30f + (0.5f * Endurance);

        public float CurrentFP;
        public float CurrentStamina;
        public float CurrentEquipLoad;

        public long RunesHeld;
        public long RunesLastDeath;
        public Vector2 RunesLocation;
        public bool HasLostRunes;
        #endregion

        #region Rolling States
        public bool IsRolling;
        public int RollTimer;
        private const int LIGHT_ROLL_DURATION = 30;
        private const int MEDIUM_ROLL_DURATION = 45;
        private const int HEAVY_ROLL_DURATION = 55;
        public int RollDirection = 1;
        public double Rotation = 0f;
        public int IFrames = 0;
        public bool IsOverweight;
        public float LoopVelocity = 0;
        public float RampVelocity = 0;
        public Vector2 RotationOrigin;
        private int staminaRegenDelay = 0;
        private const int STAMINA_REGEN_COOLDOWN = 60;

        #region Input Handling
        private bool rollKeyPressed;
        #endregion

        public enum EquipLoadState
        {
            Light,
            Medium,
            Heavy,
            Overload
        }

        public EquipLoadState CurrentLoadState => GetEquipLoadState();

        public bool CanUseAshOfWar = true;
        public bool InBossFight;
        public bool IsAtSiteOfGrace;
        #endregion

        #region Initialization
        public override void Initialize()
        {
            Vigor = 10;
            Mind = 10;
            Endurance = 10;
            Strength = 10;
            Dexterity = 10;
            Intelligence = 10;
            Faith = 10;
            Arcane = 10;

            CurrentFP = MaxFP;
            CurrentStamina = MaxStamina;
            CurrentEquipLoad = 0f;

            RunesHeld = 0;
            RunesLastDeath = 0;
            HasLostRunes = false;

            ResetRollState();
        }

        private void ResetRollState()
        {
            IsRolling = false;
            Rotation = 0f;
            IsOverweight = false;
            Player.fullRotation = 0f;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            rollKeyPressed = TerraRingKeybinds.RollKey.JustPressed;
        }
        #endregion

        #region Save/Load
        public override void SaveData(TagCompound tag)
        {
            tag["Vigor"] = Vigor;
            tag["Mind"] = Mind;
            tag["Endurance"] = Endurance;
            tag["Strength"] = Strength;
            tag["Dexterity"] = Dexterity;
            tag["Intelligence"] = Intelligence;
            tag["Faith"] = Faith;
            tag["Arcane"] = Arcane;
            tag["RunesHeld"] = RunesHeld;
            tag["HasLostRunes"] = HasLostRunes;
            if (HasLostRunes)
            {
                tag["RunesLastDeath"] = RunesLastDeath;
                tag["RunesLocationX"] = RunesLocation.X;
                tag["RunesLocationY"] = RunesLocation.Y;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            Vigor = tag.GetInt("Vigor");
            Mind = tag.GetInt("Mind");
            Endurance = tag.GetInt("Endurance");
            Strength = tag.GetInt("Strength");
            Dexterity = tag.GetInt("Dexterity");
            Intelligence = tag.GetInt("Intelligence");
            Faith = tag.GetInt("Faith");
            Arcane = tag.GetInt("Arcane");
            RunesHeld = tag.GetLong("RunesHeld");
            HasLostRunes = tag.GetBool("HasLostRunes");
            if (HasLostRunes)
            {
                RunesLastDeath = tag.GetLong("RunesLastDeath");
                RunesLocation = new Vector2(tag.GetFloat("RunesLocationX"), tag.GetFloat("RunesLocationY"));
            }
        }
        #endregion

        #region Update Logic
        public override void PreUpdate()
        {
            base.PreUpdate();

            if (IsRolling)
            {
                Player.velocity.Y = Math.Min(Player.velocity.Y, 0f);
            }

            UpdateResources();
            UpdateRollState();
            UpdateEquipLoad();
            HandleRollInput();
        }

        public override void PostUpdate()
        {
            if (staminaRegenDelay > 0)
            {
                staminaRegenDelay--;
            }

            if (staminaRegenDelay <= 0)
            {
                if (CurrentStamina < MaxStamina)
                {
                    CurrentStamina += 0.5f;
                    if (CurrentStamina > MaxStamina)
                    {
                        CurrentStamina = MaxStamina;
                    }
                }

                if (IsRolling)
                {
                    Player.height = Player.defaultHeight;
                    Player.width = Player.defaultWidth;
                }
            }
        }

        private void UpdateResources()
        {
            if (IsAtSiteOfGrace)
            {
                CurrentFP = MaxFP;
                CurrentStamina = MaxStamina;
            }
            else
            {
                if (!InBossFight && CurrentFP < MaxFP)
                {
                    CurrentFP += 0.1f;
                }
            }

            CurrentFP = Math.Clamp(CurrentFP, 0, MaxFP);
            CurrentStamina = Math.Clamp(CurrentStamina, 0, MaxStamina);
        }

        private void UpdateRollState()
        {
            if (IsRolling)
            {
                if (RollTimer > 0)
                {
                    RollTimer--;
                    UpdateRollRotation();
                    HandleRollMovement();

                    if (CurrentLoadState == EquipLoadState.Heavy && RollTimer < 10)
                    {
                        Player.velocity.X *= 0.9f;
                    }
                }
                else
                {
                    EndRoll();
                }

                if (IFrames > 0)
                {
                    Player.immune = true;
                    if (Player.immuneTime < IFrames)
                    {
                        Player.immuneTime = IFrames;
                    }
                }
            }
            else
            {
                RollTimer = GetMaxRollDuration();
            }
        }

        private void UpdateRollRotation()
        {
            float rotationSpeedMultiplier = CurrentLoadState switch
            {
                EquipLoadState.Light => 1.1f,
                EquipLoadState.Medium => 1.0f,
                EquipLoadState.Heavy => 0.9f,
                _ => 1.0f
            };

            if (IsOverweight)
            {
                if (Player.velocity.X == 0 && Player.velocity.Y == 0)
                {
                    Rotation = RollTimer * 8 * (float)Math.PI / 180 * RollDirection * Player.gravDir * rotationSpeedMultiplier;
                }
                else
                {
                    Rotation = (float)Math.PI / 180 * (Player.position.X % 360) * 8 * 0.255f * Player.gravDir * rotationSpeedMultiplier;
                }
            }
            else
            {
                float rollProgress = 1f - (float)RollTimer / GetMaxRollDuration();
                float totalRotation = MathHelper.TwoPi;

                Rotation = totalRotation * rollProgress * RollDirection * Player.gravDir * rotationSpeedMultiplier;
            }

            RotationOrigin = Player.Center - new Vector2(0, Player.height / 4) - Player.position;
            if (Player.gravDir < 0)
            {
                RotationOrigin = Player.Center - new Vector2(0, Player.height * 0.8f) - Player.position;
            }
        }

        private int GetMaxRollDuration()
        {
            return CurrentLoadState switch
            {
                EquipLoadState.Light => LIGHT_ROLL_DURATION,
                EquipLoadState.Medium => MEDIUM_ROLL_DURATION,
                EquipLoadState.Heavy => HEAVY_ROLL_DURATION,
                _ => MEDIUM_ROLL_DURATION
            };
        }

        private void HandleRollMovement()
        {
            int originalHeight = Player.height;

            Player.height = 20;

            Player.position.Y += originalHeight - Player.height;

            Player.fullRotation = (float)Rotation;
            Player.fullRotationOrigin = RotationOrigin;
        }

        private void EndRoll()
        {
            int originalHeight = Player.height;

            IsRolling = false;
            IsOverweight = false;

            if (CurrentLoadState == EquipLoadState.Heavy)
            {
                Player.velocity.X *= 0.7f;
            }

            Player.height = Player.defaultHeight;
            Player.position.Y -= Player.defaultHeight - originalHeight;
            Player.fullRotation = 0f;
            Rotation = 0f;
        }

        private void UpdateEquipLoad()
        {
            CurrentEquipLoad = CalculateEquipLoad();
        }

        private float CalculateEquipLoad()
        {
            float totalLoad = 0f;

            for (int i = 0; i < 8 + Player.extraAccessorySlots; i++)
            {
                Item item = Player.armor[i];
                if (!item.IsAir)
                {
                    totalLoad += GetItemWeight(item);
                }
            }
            return totalLoad;
        }
        #endregion

        #region Combat Methods
        public void StartRoll()
        {
            IsRolling = true;

            switch (CurrentLoadState)
            {
                case EquipLoadState.Light:
                    RollTimer = LIGHT_ROLL_DURATION;
                    break;
                case EquipLoadState.Medium:
                    RollTimer = MEDIUM_ROLL_DURATION;
                    break;
                case EquipLoadState.Heavy:
                    RollTimer = HEAVY_ROLL_DURATION;
                    break;
                case EquipLoadState.Overload:
                    IsRolling = false;
                    return;
            }

            RollDirection = Player.controlRight ? 1 : -1;
            Player.direction = RollDirection;

            float rollSpeed = GetRollSpeed();
            Player.velocity.X = RollDirection * rollSpeed;
            Player.velocity.Y = 0f;

            ApplyIFrames();
            ConsumeRollStamina();

            SoundEngine.PlaySound(SoundID.Item7 with { Volume = 0.5f, Pitch = GetRollSoundPitch() }, Player.Center);
        }

        public void ConsumeRollStamina()
        {
            CurrentStamina -= 20f;
            staminaRegenDelay = STAMINA_REGEN_COOLDOWN;
        }

        private float GetRollSoundPitch()
        {
            return CurrentLoadState switch
            {
                EquipLoadState.Light => 0.2f,
                EquipLoadState.Medium => 0f,
                EquipLoadState.Heavy => -0.2f,
                _ => 0f
            };
        }

        private float GetRollSpeed()
        {
            float baseSpeed = 6.5f + Player.jumpSpeedBoost;

            float loadRatio = CurrentEquipLoad / MaxEquipLoad;
            if (loadRatio < 0.3f)
                return baseSpeed * 1.2f;
            else if (loadRatio < 0.7f)
                return baseSpeed;
            else
                return baseSpeed * 0.7f;
        }

        private void ApplyIFrames()
        {
            int iFrameCount = CurrentLoadState switch
            {
                EquipLoadState.Light => 9,
                EquipLoadState.Medium => 6,
                EquipLoadState.Heavy => 3,
                _ => 6
            };

            Player.immune = true;
            Player.immuneTime = iFrameCount;
            IFrames = iFrameCount;
        }

        public bool TryUseAshOfWar(float fpCost)
        {
            if (CurrentFP >= fpCost && CanUseAshOfWar)
            {
                CurrentFP -= fpCost;
                return true;
            }
            return false;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (Player.statLife - hurtInfo.Damage <= 0 && !HasLostRunes)
            {
                DropRunes();
            }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (Player.statLife - hurtInfo.Damage <= 0 && !HasLostRunes)
            {
                DropRunes();
            }
        }
        #endregion

        #region Utility Methods
        private EquipLoadState GetEquipLoadState()
        {
            float loadPercentage = (CurrentEquipLoad / MaxEquipLoad) * 100f;

            if (loadPercentage > 100f)
            {
                return EquipLoadState.Overload;
            }
            if (loadPercentage > 70f)
            {
                return EquipLoadState.Heavy;
            }
            if (loadPercentage > 30f)
            {
                return EquipLoadState.Medium;
            }
            return EquipLoadState.Light;
        }

        public float GetItemWeight(Item item)
        {
            return item.rare + 1f;
        }

        private void DropRunes()
        {
            if (RunesHeld > 0)
            {
                RunesLastDeath = RunesHeld;
                RunesLocation = new Microsoft.Xna.Framework.Vector2(Player.Center.X, Player.Center.Y);
                RunesHeld = 0;
                HasLostRunes = true;
            }
        }

        public void CollectRunes()
        {
            if (HasLostRunes)
            {
                RunesHeld += RunesLastDeath;
                RunesLastDeath = 0;
                HasLostRunes = false;
            }
        }

        public bool TryLevelUp(string stat, int runesCost)
        {
            if (RunesHeld >= runesCost)
            {
                switch (stat.ToLower())
                {
                    case "vigor":
                        Vigor++;
                        break;
                    case "mind":
                        Mind++;
                        break;
                    case "endurance":
                        Endurance++;
                        break;
                    case "strength":
                        Strength++;
                        break;
                    case "dexterity":
                        Dexterity++;
                        break;
                    case "intelligence":
                        Intelligence++;
                        break;
                    case "faith":
                        Faith++;
                        break;
                    case "arcane":
                        Arcane++;
                        break;
                    default:
                        return false;
                }
                RunesHeld -= runesCost;
                return true;
            }
            return false;
        }
        #endregion

        #region Input Methods
        private void HandleRollInput()
        {
            if (!IsRolling &&
                rollKeyPressed &&
                CurrentStamina >= 20f &&
                (Player.controlLeft || Player.controlRight) &&
                Player.velocity.Y == 0 &&
                CurrentLoadState != EquipLoadState.Overload)
            {
                StartRoll();
            }
            else if (rollKeyPressed && CurrentLoadState == EquipLoadState.Overload)
            {
                CombatText.NewText(Player.getRect(), Color.Red, "Too heavy to roll!", true);
            }
        }
        #endregion

        #region Network Sync
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncTerraRingPlayer);
            packet.Write((byte)Player.whoAmI);
            packet.Write(IsRolling);
            packet.Write(RollTimer);
            packet.Write(RollDirection);
            packet.Write(CurrentStamina);
            packet.Send(toWho, fromWho);
        }

        public enum MessageType : byte
        {
            SyncTerraRingPlayer
        }
        #endregion
    }
}