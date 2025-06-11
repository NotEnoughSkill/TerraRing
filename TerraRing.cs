using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraRing.UI;

namespace TerraRing
{
	public class TerraRing : Mod
	{
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            TerraRingPlayer.MessageType msgType = (TerraRingPlayer.MessageType)reader.ReadByte();

            int x, y;
            Point gracePosition;

            switch (msgType)
            {
                case TerraRingPlayer.MessageType.SyncTerraRingPlayer:
                    byte playerID = reader.ReadByte();
                    bool isRolling = reader.ReadBoolean();
                    int rollTimer = reader.ReadInt32();
                    int rollDirection = reader.ReadInt32();
                    float currentStamina = reader.ReadSingle();

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        var player = Main.player[playerID];
                        var modPlayer = player.GetModPlayer<TerraRingPlayer>();
                        modPlayer.IsRolling = isRolling;
                        modPlayer.RollTimer = rollTimer;
                        modPlayer.RollDirection = rollDirection;
                        modPlayer.CurrentStamina = currentStamina;
                    }
                    break;

                case TerraRingPlayer.MessageType.SyncRunes:
                    playerID = reader.ReadByte();
                    long currentRunes = reader.ReadInt64();

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        var player = Main.player[playerID];
                        var modPlayer = player.GetModPlayer<TerraRingPlayer>();
                        modPlayer.Stats.Runes = currentRunes;
                    }
                    break;

                case TerraRingPlayer.MessageType.SyncLostRunes:
                    playerID = reader.ReadByte();
                    long lostRunes = reader.ReadInt64();
                    float posX = reader.ReadSingle();
                    float posY = reader.ReadSingle();

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        var player = Main.player[playerID];
                        var modPlayer = player.GetModPlayer<TerraRingPlayer>();
                        modPlayer.LostRunes = lostRunes;
                        modPlayer.HasLostRunes = true;
                        modPlayer.LostRunesPosition = new Vector2(posX, posY);
                    }
                    break;

                case TerraRingPlayer.MessageType.SyncSiteOfGrace:
                    x = reader.ReadInt32();
                    y = reader.ReadInt32();
                    gracePosition = new Point(x, y);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)TerraRingPlayer.MessageType.SyncSiteOfGrace);
                        packet.Write(x);
                        packet.Write(y);
                        packet.Send(-1, whoAmI);
                    }
                    else
                    {
                        var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                        if (!modPlayer.DiscoveredSitesOfGrace.Any(p => p.X == x && p.Y == y))
                        {
                            modPlayer.DiscoveredSitesOfGrace.Add(gracePosition);
                        }
                    }
                    break;
                case TerraRingPlayer.MessageType.RemoveSiteOfGrace:
                    x = reader.ReadInt32();
                    y = reader.ReadInt32();
                    gracePosition = new Point(x, y);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)TerraRingPlayer.MessageType.RemoveSiteOfGrace);
                        packet.Write(x);
                        packet.Write(y);
                        packet.Send(-1, whoAmI);
                    }
                    else
                    {
                        var modPlayer = Main.LocalPlayer.GetModPlayer<TerraRingPlayer>();
                        modPlayer.DiscoveredSitesOfGrace.RemoveAll(p => p.X == x && p.Y == y);
                    }
                    break;
            }
        }
    }
}