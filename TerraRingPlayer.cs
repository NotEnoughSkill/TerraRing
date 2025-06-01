using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using TerraRing.Projectiles;
using TerraRing.Systems;

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

        #endregion

        #region Scaling
        public float PhysicalDefence => 100f + (2f * Vigor) + (1.5f * Strength);
        public float MagicDefense => 100f + (2f * Mind) + (1.8f * Intelligence);
        public float FireDefense => 100f + (1.5f * Vigor) + (1.2f * Faith);
        public float LightningDefense => 100f + (1.2f * Endurance) + (1.5f * Faith);

        public float StrengthScaling => 1f + (Strength * 0.01f);
        public float DexterityScaling => 1f + (Dexterity * 0.01f);
        public float IntelligenceScaling => 1f + (Intelligence * 0.01f);
        public float FaithScaling => 1f + (Faith * 0.01f);
        public float ArcaneScaling => 1f + (Arcane * 0.01f);
        #endregion

        #region Rolling States
        public bool IsRolling;
        public int RollTimer;
        private const int ROLL_DURATION = 30;
        public int RollDirection = 1;
        public double Rotation = 0f;
        public int IFrames = 0;
        public bool IsOverweight;
        public Vector2 StartVelocity;
        public Vector2 RotationOrigin;
        private int staminaRegenDelay = 0;
        private const int STAMINA_REGEN_COOLDOWN = 60;
        private const float MAX_ROLL_ROTATION = MathHelper.TwoPi;

        private const float LIGHT_ROLL_DISTANCE = 1.4f;
        private const float MEDIUM_ROLL_DISTANCE = 1.0f;
        private const float HEAVY_ROLL_DISTANCE = 0.7f;

        private const int LIGHT_ROLL_IFRAMES = 15;
        private const int MEDIUM_ROLL_IFRAMES = 10;
        private const int HEAVY_ROLL_IFRAMES = 6;
        #endregion

        #region AccumuVal
        private float accumulatedValue = 0f;
        private uint lastHitTime = 0;
        private const float DECAY_RATE = 0.95f;
        private const float MAX_ACCUMU_VAL = 100f;
        private const int DECAY_DELAY = 90;
        #endregion

        #region Runes
        public long CurrentRunes { get; set; }
        public long LostRunes { get; set; }
        public Vector2 LostRunesPosition { get; set; }
        public bool HasLostRunes { get; set; }

        private int lastDeathWorld;
        #endregion

        #region Site of Grace
        public bool IsAtSiteOfGrace { get; set; }
        public Point LastSiteOfGracePosition { get; set; }
        public string LastSiteOfGraceName { get; set; }
        public List<Point> DiscoveredSitesOfGrace { get; private set; } = new List<Point>();

        public bool MapTravelMode { get; set; }
        public bool CanTravel { get; set; }
        private Point? selectedSiteOfGrace;
        #endregion

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

            CurrentRunes = 0;
            LostRunes = 0;
            HasLostRunes = false;
            LostRunesPosition = Vector2.Zero;

            DiscoveredSitesOfGrace = new List<Point>();
            MapTravelMode = false;
            IsAtSiteOfGrace = false;

            ResetRollState();
        }

        private void ResetRollState()
        {
            IsRolling = false;
            Rotation = 0f;
            IsOverweight = false;
            Player.fullRotation = 0f;
        }

        public void ClearSitesOfGrace()
        {
            int count = DiscoveredSitesOfGrace.Count;
            DiscoveredSitesOfGrace.Clear();

            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = 0.2f });

            for (int i = 0; i < 50; i++)
            {
                Vector2 position = Player.Center + Main.rand.NextVector2Circular(100f, 100f);
                Dust.NewDust(position, 32, 32, DustID.GoldFlame,
                    Scale: Main.rand.NextFloat(1f, 1.5f));
            }

            CombatText.NewText(Player.getRect(), new Color(255, 207, 107),
                $"Cleared {count} Sites of Grace", true);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MessageType.ClearSitesOfGrace);
                packet.Write(Player.whoAmI);
                packet.Send();
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            rollKeyPressed = TerraRingKeybinds.RollKey.JustPressed;

            if (TerraRingKeybinds.ClearSitesKey.JustPressed)
            {
                ClearSitesOfGrace();
            }
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

            tag["AccumuVal"] = accumulatedValue;
            tag["LastHitTime"] = lastHitTime;

            tag["CurrentRunes"] = (long)CurrentRunes;
            tag["LostRunes"] = (long)LostRunes;
            tag["HasLostRunes"] = HasLostRunes;
            tag["LostRunesX"] = LostRunesPosition.X;
            tag["LostRunesY"] = LostRunesPosition.Y;
            tag["LastDeathWorld"] = lastDeathWorld;

            tag["SitesOfGrace"] = DiscoveredSitesOfGrace.Select(p => $"{p.X},{p.Y}").ToList();
            tag["MapTravelMode"] = MapTravelMode;
            tag["IsAtSiteOfGrace"] = IsAtSiteOfGrace;
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
            HasLostRunes = tag.GetBool("HasLostRunes");

            accumulatedValue = tag.GetFloat("AccumuVal");
            lastHitTime = tag.Get<uint>("LastHitTime");

            CurrentRunes = tag.Get<long>("CurrentRunes");
            LostRunes = tag.Get<long>("LostRunes");
            HasLostRunes = tag.GetBool("HasLostRunes");
            LostRunesPosition = new Vector2(
                tag.GetFloat("LostRunesX"),
                tag.GetFloat("LostRunesY")
            );
            lastDeathWorld = tag.GetInt("LastDeathWorld");

            DiscoveredSitesOfGrace.Clear();
            var sites = tag.Get<List<string>>("SitesOfGrace") ?? new List<string>();
            foreach (var site in sites)
            {
                var coords = site.Split(',');
                if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                {
                    DiscoveredSitesOfGrace.Add(new Point(x, y));
                }
            }
            MapTravelMode = tag.GetBool("MapTravelMode");
            IsAtSiteOfGrace = tag.GetBool("IsAtSiteOfGrace");
        }

        public float GetAccumuVal()
        {
            return accumulatedValue;
        }
        #endregion

        public void DebugPrintSites()
        {
            Main.NewText($"Total Sites of Grace discovered: {DiscoveredSitesOfGrace.Count}", Color.Yellow);
            for (int i = 0; i < DiscoveredSitesOfGrace.Count; i++)
            {
                Point site = DiscoveredSitesOfGrace[i];
                Main.NewText($"Site {i + 1}: ({site.X}, {site.Y})", Color.White);
            }
        }

        #region Update Logic
        public override void PreUpdate()
        {
            if (Main.keyState.IsKeyDown(Keys.P) && !Main.oldKeyState.IsKeyDown(Keys.P))
            {
                DebugPrintSites();
            }


            bool wasAtSiteOfGrace = IsAtSiteOfGrace;
            IsAtSiteOfGrace = false;

            Point playerTile = Player.Center.ToTileCoordinates();
            foreach (Point site in DiscoveredSitesOfGrace)
            {
                if (Vector2.Distance(playerTile.ToVector2(), site.ToVector2()) < 3f)
                {
                    IsAtSiteOfGrace = true;
                    break;
                }
            }

            if (wasAtSiteOfGrace && !IsAtSiteOfGrace && Main.netMode != NetmodeID.Server)
            {
                TerraRingUI.Instance.HideSiteOfGraceUI();
            }

            if (MapTravelMode)
            {
                if (!Main.mapFullscreen)
                {
                    MapTravelMode = false;
                }
                else if (Main.keyState.IsKeyDown(Keys.Escape))
                {
                    MapTravelMode = false;
                    Main.mapFullscreen = false;
                    Main.mapEnabled = false;
                }
            }

            if (!MapTravelMode && Main.mapFullscreen)
            {
                Main.mapFullscreen = false;
                Main.mapEnabled = true;
            }

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

            if (Main.GameUpdateCount - lastHitTime > DECAY_DELAY)
            {
                accumulatedValue *= DECAY_RATE;
                if (accumulatedValue < 0.1f)
                {
                    accumulatedValue = 0f;
                }
            }

            if (MapTravelMode && Main.mapFullscreen)
            {
                Vector2 mouseWorld = Main.mapFullscreenPos + new Vector2(Main.mouseX - Main.screenWidth / 2, Main.mouseY - Main.screenHeight / 2) / Main.mapFullscreenScale;
                Point mouseTile = mouseWorld.ToTileCoordinates();

                selectedSiteOfGrace = null;

                foreach (Point site in DiscoveredSitesOfGrace)
                {
                    if (Vector2.Distance(site.ToVector2(), mouseTile.ToVector2()) < 3f)
                    {
                        selectedSiteOfGrace = site;
                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            TravelToSite(site);
                            break;
                        }
                    }
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
                        Player.velocity.X *= 0.92f;
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
                RollTimer = ROLL_DURATION;
            }
        }

        private void UpdateRollRotation()
        {
            float rollProgress = 1f - (float)RollTimer / ROLL_DURATION;

            float targetRotation = MAX_ROLL_ROTATION * RollDirection;
            float smoothedProgress = (float)Math.Pow(Math.Sin(rollProgress * MathHelper.PiOver2), 0.75);
            Rotation = targetRotation * smoothedProgress * Player.gravDir;

            RotationOrigin = Player.Center - new Vector2(0, Player.height / 4) - Player.position;
            if (Player.gravDir < 0)
            {
                RotationOrigin = Player.Center - new Vector2(0, Player.height * 0.8f) - Player.position;
            }
        }

        private void HandleRollMovement()
        {
            int originalHeight = Player.height;

            Player.height = (int)(Player.height * 0.6f);

            Player.position.Y += originalHeight - Player.height;

            Player.fullRotation = (float)Rotation;
            Player.fullRotationOrigin = RotationOrigin;

            float baseSpeed = Math.Max(6f, Player.maxRunSpeed);

            float distanceMultiplier = CurrentLoadState switch
            {
                EquipLoadState.Light => LIGHT_ROLL_DISTANCE,
                EquipLoadState.Medium => MEDIUM_ROLL_DISTANCE,
                EquipLoadState.Heavy => HEAVY_ROLL_DISTANCE,
                _ => MEDIUM_ROLL_DISTANCE
            };

            float rollProgress = 1f - (float)RollTimer / ROLL_DURATION;
            float velocityMultiplier = (float)Math.Sin(rollProgress * MathHelper.Pi);

            if (rollProgress < 0.5f)
            {
                Player.velocity.X = baseSpeed * RollDirection * velocityMultiplier * distanceMultiplier;
            }
            else
            {
                Player.velocity.X *= 0.95f;
            }

            Player.velocity.Y = Math.Min(Player.velocity.Y, 0f);
        }

        private void EndRoll()
        {
            int originalHeight = Player.height;

            IsRolling = false;
            IsOverweight = false;

            float endSpeedMultiplier = CurrentLoadState switch
            {
                EquipLoadState.Light => 0.9f,
                EquipLoadState.Medium => 0.85f,
                EquipLoadState.Heavy => 0.7f,
                _ => 0.85f
            };
            Player.velocity.X *= endSpeedMultiplier;

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
            if (!IsRolling)
            {
                IsRolling = true;
                StartVelocity = Player.velocity;
                RollDirection = Player.direction;
                RollTimer = ROLL_DURATION;

                switch (CurrentLoadState)
                {
                    case EquipLoadState.Light:
                        IFrames = LIGHT_ROLL_IFRAMES;
                        break;
                    case EquipLoadState.Medium:
                        IFrames = MEDIUM_ROLL_IFRAMES;
                        break;
                    case EquipLoadState.Heavy:
                        IFrames = HEAVY_ROLL_IFRAMES;
                        break;
                    case EquipLoadState.Overload:
                        IsRolling = false;
                        return;
                }

                ConsumeRollStamina();

                float distanceMultiplier = CurrentLoadState switch
                {
                    EquipLoadState.Light => LIGHT_ROLL_DISTANCE,
                    EquipLoadState.Medium => MEDIUM_ROLL_DISTANCE,
                    EquipLoadState.Heavy => HEAVY_ROLL_DISTANCE,
                    _ => MEDIUM_ROLL_DISTANCE
                };

                float initialSpeed = Math.Max(6f, Player.maxRunSpeed) * 1.5f * distanceMultiplier;
                Player.velocity.X = RollDirection * initialSpeed;
                Player.velocity.Y = Math.Min(Player.velocity.Y, 0f);
            }
        }

        public void ConsumeRollStamina()
        {
            CurrentStamina -= 20f;
            staminaRegenDelay = STAMINA_REGEN_COOLDOWN;
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
            HandleHit();
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            HandleHit();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            HandleHit();
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
            HandleHit();
        }

        private void HandleHit()
        {
            accumulatedValue = System.Math.Min(accumulatedValue + 10f, MAX_ACCUMU_VAL);
            lastHitTime = Main.GameUpdateCount;
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

        public bool TryLevelUp(string stat, int runesCost)
        {
            if (CurrentRunes >= runesCost)
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
                CurrentRunes -= runesCost;
                return true;
            }
            return false;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (Main.mapFullscreen)
            {
                foreach (Point site in DiscoveredSitesOfGrace)
                {
                    Vector2 mapPos = site.ToVector2() * 16;
                    Vector2 screenPos = Main.mapFullscreenPos + (mapPos - Main.mapFullscreenPos) * Main.mapFullscreenScale;

                    bool isSelected = selectedSiteOfGrace.HasValue && selectedSiteOfGrace.Value == site;
                    Color iconColor = isSelected ? new Color(255, 207, 107) : new Color(200, 170, 80);

                    Main.spriteBatch.Draw(ModContent.Request<Texture2D>("TerraRing/Items/Placeables/SiteOfGrace").Value,
                        screenPos, null, iconColor, 0f, Vector2.Zero, Main.mapFullscreenScale, SpriteEffects.None, 0f);
                }
            }
        }
        #endregion

        #region Runes Methods
        public void AddRunes(long amount)
        {
            if (amount > 0)
            {
                CurrentRunes += amount;
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.SyncRunes);
                    packet.Write(Player.whoAmI);
                    packet.Write(CurrentRunes);
                    packet.Send();
                }
            }
        }

        public bool SpendRunes(long amount)
        {
            if (amount <= CurrentRunes)
            {
                CurrentRunes -= amount;
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.SyncRunes);
                    packet.Write(Player.whoAmI);
                    packet.Write(CurrentRunes);
                    packet.Send();
                }
                return true;
            }
            return false;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (CurrentRunes > 0)
            {
                LostRunes = CurrentRunes;
                CurrentRunes = 0;
                LostRunesPosition = Player.position;
                HasLostRunes = true;
                lastDeathWorld = Main.worldID;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projIndex = Projectile.NewProjectile(
                        Player.GetSource_Death(),
                        LostRunesPosition,
                        Vector2.Zero,
                        ModContent.ProjectileType<LostRunes>(),
                        0,
                        0f,
                        Player.whoAmI
                    );

                    if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    {
                        Main.projectile[projIndex].ai[0] = LostRunes;
                    }
                }

                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.SyncLostRunes);
                    packet.Write(Player.whoAmI);
                    packet.Write(LostRunes);
                    packet.Write(LostRunesPosition.X);
                    packet.Write(LostRunesPosition.Y);
                    packet.Send();
                }
            }
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            base.Kill(damage, hitDirection, pvp, damageSource);

            if (Main.netMode != NetmodeID.Server)
            {
                TerraRingUI.Instance.HideSiteOfGraceUI();
            }
        }

        public override void OnEnterWorld()
        {
            if (HasLostRunes && lastDeathWorld != Main.worldID)
            {
                HasLostRunes = false;
                LostRunes = 0;
                LostRunesPosition = Vector2.Zero;
            }
        }
        #endregion

        #region Site of Grace Methods
        public void ActivateSiteOfGrace(Point position)
        {
            bool isNewDiscovery = !DiscoveredSitesOfGrace.Any(p => p.X == position.X && p.Y == position.Y);

            if (isNewDiscovery)
            {
                DiscoveredSitesOfGrace.Add(position);
                CombatText.NewText(Player.getRect(), new Color(255, 207, 107), "Site of Grace discovered", true);

                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = 0.2f });

                for (int i = 0; i < 50; i++)
                {
                    Vector2 position2 = position.ToVector2() * 16;
                    Vector2 speed = Main.rand.NextVector2Circular(8f, 8f);
                    Dust.NewDust(position2, 32, 32, DustID.GoldFlame, speed.X, speed.Y);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.SyncSiteOfGrace);
                    packet.Write(position.X);
                    packet.Write(position.Y);
                    packet.Send();
                }
            }

            IsAtSiteOfGrace = true;
            LastSiteOfGracePosition = position;
            LastSiteOfGraceName = "Site of Grace";

            Player.statLife = Player.statLifeMax2;
            Player.statMana = Player.statManaMax2;
            CurrentFP = MaxFP;
            CurrentStamina = MaxStamina;

            for (int i = 0; i < Player.buffType.Length; i++)
            {
                if (Main.debuff[Player.buffType[i]])
                {
                    Player.DelBuff(i);
                    i--;
                }
            }

            if (Main.netMode != NetmodeID.Server)
            {
                TerraRingUI.Instance.ShowSiteOfGraceUI();
            }
        }

        public void TeleportToLastSiteOfGrace()
        {
            if (LastSiteOfGracePosition != Point.Zero)
            {
                Player.Teleport(new Vector2(LastSiteOfGracePosition.X * 16 + 16, LastSiteOfGracePosition.Y * 16));

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height,
                            DustID.GoldFlame, Scale: 1.5f);
                    }
                }
            }
        }

        public void TravelToSite(Point site)
        {
            if (!DiscoveredSitesOfGrace.Any(p => p.X == site.X && p.Y == site.Y))
                return;

            MapTravelMode = false;
            Main.mapFullscreen = false;
            Main.mapEnabled = false;

            SoundEngine.PlaySound(SoundID.Item6);

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height,
                    DustID.GoldFlame,
                    Scale: Main.rand.NextFloat(1f, 1.5f));
            }

            Player.Teleport(new Vector2(site.X * 16 + 8, site.Y * 16));

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height,
                    DustID.GoldFlame,
                    Scale: Main.rand.NextFloat(1f, 1.5f));
            }
        }

        public void DiscoverSiteOfGrace(Point position)
        {
            bool siteExists = DiscoveredSitesOfGrace.Any(p =>
                Math.Abs(p.X - position.X) <= 2 &&
                Math.Abs(p.Y - position.Y) <= 2);

            if (!siteExists)
            {
                DiscoveredSitesOfGrace.Add(position);
            }
        }

        public void RemoveSiteOfGrace(Point position)
        {
            if (DiscoveredSitesOfGrace.RemoveAll(p => p.X == position.X && p.Y == position.Y) > 0)
            {
                CombatText.NewText(Player.getRect(), new Color(255, 207, 107), "Site of Grace removed", true);

                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = position.ToVector2() * 16;
                    Vector2 speed = Main.rand.NextVector2Circular(8f, 8f);
                    Dust.NewDust(dustPos, 32, 32, DustID.GoldFlame, speed.X, speed.Y);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.RemoveSiteOfGrace);
                    packet.Write(position.X);
                    packet.Write(position.Y);
                    packet.Send();
                }
            }
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
            SyncTerraRingPlayer,
            SyncRunes,
            SyncLostRunes,
            SyncSiteOfGrace,
            RemoveSiteOfGrace,
            ClearSitesOfGrace
        }
        #endregion
    }
}