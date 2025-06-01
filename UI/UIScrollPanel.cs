using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerraRing.UI
{
    internal class UIScrollPanel : UIPanel
    {
        private Vector2 offset;
        private bool dragging;
        private Vector2 lastMousePos;
        private float scrollVelocity;
        private float maxScroll;
        private float currentScroll;
        private bool hasScrollbar;

        public void SetMaxScroll(float max)
        {
            maxScroll = max;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            hasScrollbar = true;
        }

        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);
            if (hasScrollbar)
            {
                scrollVelocity = -evt.ScrollWheelValue / 4f;
            }
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            if (ContainsPoint(evt.MousePosition))
            {
                dragging = true;
                lastMousePos = evt.MousePosition;
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (dragging)
            {
                offset.Y += Main.MouseScreen.Y - lastMousePos.Y;
                lastMousePos = Main.MouseScreen;
            }

            currentScroll = offset.Y;
            scrollVelocity = MathHelper.Lerp(scrollVelocity, 0f, 0.1f);
            offset.Y += scrollVelocity;

            if (maxScroll > 0)
            {
                if (offset.Y > 0)
                {
                    offset.Y = MathHelper.Lerp(offset.Y, 0f, 0.1f);
                }
                else if (offset.Y < -maxScroll)
                {
                    offset.Y = MathHelper.Lerp(offset.Y, -maxScroll, 0.1f);
                }
            }
            else
            {
                offset.Y = MathHelper.Lerp(offset.Y, 0f, 0.1f);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var innerDimensions = GetInnerDimensions();
            var rectangle = new Rectangle((int)innerDimensions.X, (int)innerDimensions.Y, (int)innerDimensions.Width, (int)innerDimensions.Height);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateTranslation(0, offset.Y, 0));

            DrawChildren(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}