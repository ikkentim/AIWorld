using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIWorld
{
    public class Plane
    {
        private static readonly short[] Indexes = {0, 1, 2, 2, 1, 3};

        private readonly BasicEffect _effect;
        private readonly VertexPositionNormalTexture[] _vertices;

        public Plane(GraphicsDevice graphicsDevice, Vector3 position, float size, PlaneRotation textureRotation,
            Texture2D texture)
        {
            _effect = new BasicEffect(graphicsDevice) {TextureEnabled = true, Texture = texture};
            _effect.EnableDefaultLighting();

            var corners = new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            _vertices = new VertexPositionNormalTexture[4];

            for (int i = 0; i < 4; i++) _vertices[i].Normal = Vector3.Up;

            Vector3 quarter = new Vector3(size, 0, size)/2;
            Vector3 nquarter = new Vector3(size, 0, -size)/2;

            var offset = (int) textureRotation;
            _vertices[0].Position = position - quarter;
            _vertices[0].TextureCoordinate = corners[(3 + offset)%4];
            _vertices[1].Position = position + nquarter;
            _vertices[1].TextureCoordinate = corners[(0 + offset)%4];
            _vertices[2].Position = position - nquarter;
            _vertices[2].TextureCoordinate = corners[(2 + offset)%4];
            _vertices[3].Position = position + quarter;
            _vertices[3].TextureCoordinate = corners[(1 + offset)%4];
        }

        public Plane(GraphicsDevice graphicsDevice, Vector3 position, float width, float height, PlaneRotation textureRotation,
            Texture2D texture)
        {
            _effect = new BasicEffect(graphicsDevice) { TextureEnabled = true, Texture = texture };
            _effect.EnableDefaultLighting();

            var corners = new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            _vertices = new VertexPositionNormalTexture[4];

            for (int i = 0; i < 4; i++) _vertices[i].Normal = Vector3.Up;

            Vector3 quarter = new Vector3(width, 0, height) / 2;
            Vector3 nquarter = new Vector3(width, 0, -height) / 2;

            var offset = (int)textureRotation;
            _vertices[0].Position = position - quarter;
            _vertices[0].TextureCoordinate = corners[(3 + offset) % 4];
            _vertices[1].Position = position + nquarter;
            _vertices[1].TextureCoordinate = corners[(0 + offset) % 4];
            _vertices[2].Position = position - nquarter;
            _vertices[2].TextureCoordinate = corners[(2 + offset) % 4];
            _vertices[3].Position = position + quarter;
            _vertices[3].TextureCoordinate = corners[(1 + offset) % 4];
        }

        public Plane(GraphicsDevice graphicsDevice, Vector3 upperLeft, Vector3 upperRight, Vector3 lowerLeft,
            Vector3 lowerRight, PlaneRotation textureRotation,
            Texture2D texture)
        {
            _effect = new BasicEffect(graphicsDevice) { TextureEnabled = true, Texture = texture };
            _effect.EnableDefaultLighting();

            var corners = new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            _vertices = new VertexPositionNormalTexture[4];

            for (int i = 0; i < 4; i++) _vertices[i].Normal = Vector3.Up;

            var offset = (int)textureRotation;
            _vertices[0].Position = lowerLeft;
            _vertices[0].TextureCoordinate = corners[(3 + offset) % 4];
            _vertices[1].Position = upperLeft;
            _vertices[1].TextureCoordinate = corners[(0 + offset) % 4];
            _vertices[2].Position = lowerRight;
            _vertices[2].TextureCoordinate = corners[(2 + offset) % 4];
            _vertices[3].Position = upperRight;
            _vertices[3].TextureCoordinate = corners[(1 + offset) % 4];
        }

        public void Render(GraphicsDevice graphicsDevice, Matrix world, Matrix view, Matrix projection)
        {
            _effect.World = world;
            _effect.View = view;
            _effect.Projection = projection;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices, 0, 4,
                    Indexes, 0, 2);
            }
        }
    }
}