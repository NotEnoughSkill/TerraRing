using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace TerraRing.Projectiles
{
    internal class LostRunes : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 87;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = int.MaxValue;
            Projectile.hide = false;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 1f * pulse, 0.8f * pulse, 0.4f * pulse);

            Projectile.ai[1] += 0.1f;
            Projectile.position.Y += (float)Math.Sin(Projectile.ai[1]) * 0.3f;

            Player owner = Main.player[Projectile.owner];
            if (owner.active && !owner.dead &&
                Vector2.Distance(owner.Center, Projectile.Center) < 50f)
            {
                var modPlayer = owner.GetModPlayer<TerraRingPlayer>();
                if (modPlayer.HasLostRunes)
                {
                    modPlayer.AddRunes(modPlayer.LostRunes);
                    modPlayer.HasLostRunes = false;
                    modPlayer.LostRunes = 0;
                    Projectile.Kill();

                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                            DustID.GoldCoin, Scale: 1.5f);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color glowColor = new Color(255, 207, 107) * pulse;

            Texture2D texture = ModContent.Request<Texture2D>("TerraRing/Projectiles/LostRunes").Value;
            Main.spriteBatch.Draw(
                texture,
                drawPos,
                null,
                glowColor,
                0f,
                texture.Size() * 0.5f,
                1f + pulse * 0.1f,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
    }
}