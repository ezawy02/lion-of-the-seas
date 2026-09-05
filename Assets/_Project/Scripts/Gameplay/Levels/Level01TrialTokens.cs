using SeaLion.Gameplay.Deployment;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    internal sealed class DeploymentToken : ILandingCraft
    {
        public void Activate(Vector3 position, int contribution, int sequence) { }
        public void Deactivate() { }
    }

    internal sealed class LandingToken : ILandingCraft
    {
        public void Activate(Vector3 position, int contribution, int sequence) { }
        public void Deactivate() { }
    }
}
