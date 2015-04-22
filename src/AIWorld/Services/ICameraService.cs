using Microsoft.Xna.Framework;

namespace AIWorld.Services
{
    public interface ICameraService
    {
        Matrix World { get; }
        Matrix View { get; }
        Matrix Projection { get; }

        void Update(Vector3 cameraPosition, Vector3 cameraTargetPosition, float aspectRatio);
    }
}