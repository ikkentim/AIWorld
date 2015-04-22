using Microsoft.Xna.Framework;

namespace AIWorld.Services
{
    public class CameraService : ICameraService
    {
        public CameraService()
        {
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.Identity;
        }

        #region Implementation of ICameraService

        public Matrix World { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public void Update(Vector3 cameraPosition, Vector3 cameraTargetPosition, float aspectRatio)
        {
            View = Matrix.CreateLookAt(cameraPosition, cameraTargetPosition, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.1f,
                10000.0f);
        }

        #endregion
    }
}