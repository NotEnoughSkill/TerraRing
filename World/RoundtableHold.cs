using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;
using System;
using Terraria.GameContent;

namespace TerraRing.World
{
    public class RoundtableHold : Subworld
    {
        public override int Width => 800;
        public override int Height => 600;

        public override bool ShouldSave => false;
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>
        {
            new PassLegacy("Roundtable Hold Generation", (progress, _) =>
            {
                progress.Message = "Generating Roundtable Hold...";
                GenerateRoundtable();
            })
        };

        public override void Load()
        {
            Main.dayTime = true;
            Main.time = 27000;
        }

        // ENTIRE THING WILL BE RECODED ONCE I GET THE SCHEM OF THE ROUNDTABLE

        private void GenerateRoundtable()
        {
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    Main.tile[i, j].ClearTile();
                    Main.tile[i, j].ClearEverything();
                }
            }

            int centerX = Main.maxTilesX / 2;
            int centerY = Main.maxTilesY / 2;
            
            BuildMainHall(centerX, centerY);
            BuildSideRooms(centerX, centerY);
            AddDecorations(centerX, centerY);
            AddLighting(centerX, centerY);
        }

        private void BuildMainHall(int centerX, int centerY)
        {
            int hallWidth = 100;
            int hallHeight = 60;

            // Main
            for (int i = centerX - hallWidth; i < centerX + hallWidth; i++)
            {
                for (int j = centerY; j < centerY + 3; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.StoneSlab;
                }
            }

            // Walls
            for (int i = centerX - hallWidth; i < centerX + hallWidth; i++)
            {
                for (int j = centerY - hallHeight; j < centerY; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.WallType = WallID.GrayBrick;
                }
            }

            // Ceiling
            for (int i = centerX - hallWidth; i < centerX + hallWidth; i++)
            {
                int archHeight = (int)(5 * System.Math.Sin((i - (centerX - hallWidth)) * System.Math.PI / hallWidth / 2));
                for (int j = centerY - hallHeight; j < centerY - hallHeight + 8 + archHeight; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.StoneSlab;
                }
            }
        }

        private void BuildSideRooms(int centerX, int centerY)
        {
            BuildRoom(centerX - 50, centerY,  25, 25);
            BuildRoom(centerX + 25, centerY, 25, 25);

            for (int i = centerX - 20; i < centerX + 20; i++)
            {
                Tile tile = Main.tile[i, centerY - 15];
                tile.HasTile = true;
                tile.TileType = TileID.Platforms;
            }
        }

        private void BuildRoom(int x, int y, int width, int height)
        {

            // Floor
            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + 3; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.StoneSlab;
                }
            }

            // Walls
            for (int i = x; i < x + width; i++)
            {
                for (int j = y - height; j < y; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.WallType = WallID.GrayBrick;
                }
            }

            // Ceiling
            for (int i = x; i < x + width; i++)
            {
                for (int j = y - height; j < y - height + 3; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.WallType = WallID.StoneSlab;
                }
            }
        }

        private void AddDecorations(int centerX, int centerY)
        {
            for (int i = centerX - 5; i < centerX + 5; i++)
            {
                WorldGen.PlaceTile(i, centerY - 1, TileID.Tables, true, true);
            }

            WorldGen.PlaceTile(centerX - 6, centerY - 1, TileID.Chairs, true, true);
            WorldGen.PlaceTile(centerX + 6, centerY - 1, TileID.Chairs, true, true);

            WorldGen.PlaceTile(centerX - 45, centerY - 1, TileID.Anvils, true, true);
            WorldGen.PlaceTile(centerX - 45, centerY - 1, TileID.Furnaces, true, true);

            for (int i = -40; i <= 40; i += 10)
            {
                WorldGen.PlaceTile(centerX + i, centerY - 20, TileID.Banners, true, true);
            }

            for (int i = 0; i < 3; i++)
            {
                WorldGen.PlaceTile(centerX + 30 + (i * 3), centerY - 1, TileID.Bookcases, true, true);
            }
        }

        private void AddLighting(int centerX, int centerY)
        {
            WorldGen.PlaceTile(centerX - 20, centerY - 15, TileID.Chandeliers, true, true);
            WorldGen.PlaceTile(centerX + 20, centerY - 15, TileID.Chandeliers, true, true);

            for (int i = -50; i <= 50; i += 15)
            {
                WorldGen.PlaceTile(centerX + i, centerY - 10, TileID.Torches, true, true);
            }

            WorldGen.PlaceTile(centerX - 45, centerY - 10, TileID.Torches, true, true);
            WorldGen.PlaceTile(centerX + 35, centerY - 10, TileID.Torches, true, true);
        }
    }
}