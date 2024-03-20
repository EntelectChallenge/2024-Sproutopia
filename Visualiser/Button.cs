using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Visualiser
{
    public class Button
    {
        private Texture2D _staticTeture;
        private Texture2D _clickedTexture;
        private int AnimationTime { get; set; }
        private string Name { get; }
        public Point Dimensions { get; }
        private int ID { get; }
        private float LayerDepth { get; set; }
        public bool Visible { get; set; }
        public Vector2 Position { get; }
        public Texture2D Texture { get; private set; }
        private int CellWidth { get; set; }
        private int CellHeight { get; set; }

        public Button(Texture2D staticImage,
            Texture2D clickedImage,
            Point dimensions, Vector2 position,
            string name,
            int id,
            bool visible,
            float layerDepth)
        {
            _staticTeture = staticImage;
            _clickedTexture = clickedImage;
            Dimensions = dimensions;
            Name = name;
            ID = id;
            Visible = visible;
            Position = position;
            Texture = staticImage;
            LayerDepth = layerDepth;
        }

        public void Clicked()
        {
            AnimationTime = 30;
            Texture = _clickedTexture;
        }

        public void UpdateButton()
        {
            if (AnimationTime > 0)
            {
                AnimationTime--;
            }
            if (AnimationTime == 0)
            {
                Texture = _staticTeture;
            }
        }
    }
}
