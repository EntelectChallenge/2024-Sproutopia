using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Visualiser.Utils.Camera
{
    public class CameraWindow
    {
        private Rectangle MinimisedBounds;
        private Rectangle MaximisedBounds;

        private bool IsMaximised;

        public Rectangle Bounds => IsMaximised ? MaximisedBounds : MinimisedBounds;
        public RenderTarget2D RenderTarget { get; private set; }
        public Guid BotIndex { get; private set; }

        public CameraWindow(Guid botIndex, Rectangle bounds, GraphicsDeviceManager graphicsDeviceManager)
        {
            MinimisedBounds = bounds;
            MaximisedBounds = new(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);
            RenderTarget = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, MaximisedBounds.Width, MaximisedBounds.Height);
            IsMaximised = false;
            BotIndex = botIndex;
        }

        public void Maximise()
        {
            IsMaximised = true;
        }

        public void Minimise()
        {
            IsMaximised = false;
        }
    }
}
