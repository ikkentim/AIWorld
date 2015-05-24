using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    //TODO: move to Agent class.
    public class MeshData
    {
        private Vector3 _translation;
        private Vector3 _scale;
        private Vector3 _rotation;

        public MeshData(Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            _rotation = rotation;
            _scale = scale;
            _translation = translation;
            CalculateMatrix();
        }

        public bool IsVisible { get; set; }
        public Vector3 Translation
        {
            get { return _translation; }
            set
            {
                _translation = value;
                CalculateMatrix();
            }
        }

        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                CalculateMatrix();
            }
        }

        public Vector3 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                CalculateMatrix();
            }
        }

        public Matrix Matrix { get; private set; }

        private void CalculateMatrix()
        {
            Matrix = Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) *
                     Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateScale(Scale) *
                     Matrix.CreateTranslation(Translation);
        }
    }
}