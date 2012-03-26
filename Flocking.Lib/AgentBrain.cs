namespace Flocking.Lib
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;

    public class AgentBrain
    {
        private static readonly BoundingSphere boundary = new BoundingSphere(new Vector3(0, 0, 0), 80);
        private static readonly BoundingSphere closeBoundary = new BoundingSphere(new Vector3(0, 0, 0), 50);

        public AgentBrain(BrainWeightings weightings, float maxVelocity)
        {
            this.weightings = weightings;
            this.maxVelocity = maxVelocity;

            var total = this.weightings.Total();

            AdjustWeighting(ref this.weightings.randomInitialVelocity, maxVelocity, total);
            AdjustWeighting(ref this.weightings.nearestPeer, maxVelocity, total);
            AdjustWeighting(ref this.weightings.flockCentre, maxVelocity, total);
            AdjustWeighting(ref this.weightings.nearestObject, maxVelocity, total);
            AdjustWeighting(ref this.weightings.peerVelocity, maxVelocity, total);
            AdjustWeighting(ref this.weightings.goal, maxVelocity, total);
        }

        private float maxVelocity;

        private BrainWeightings weightings;

        public Vector3 GetNewVelocity(Random random, ref Vector3 currentPosition, ref Vector3 averagePeerVelocity, ref Vector3 nearestPeerLocation, ref Vector3 nearestObjectLocation, ref Vector3 packCentre, ref Vector3 headTowards)
        {
            float x = (random.Next(3) - 1) / 4f;
            float y = (random.Next(3) - 1) / 4f;
            float z = (random.Next(3) - 1) / 4f;

            Vector3 newVelocity = new Vector3(x, y, z);
            SafeNormalise(ref newVelocity, this.weightings.randomInitialVelocity);

            var objectDifference = currentPosition - nearestObjectLocation;
            objectDifference.Z = 0;
            ContainmentType result;
            closeBoundary.Contains(ref objectDifference, out result);

            if (result == ContainmentType.Contains)
            {
                objectDifference = Vector3.Normalize(objectDifference) * (this.weightings.nearestObject * this.weightings.nearestObject);

                newVelocity += objectDifference;
            }
            else
            {
                boundary.Contains(ref objectDifference, out result);
                if (result == ContainmentType.Contains)
                {
                    objectDifference = Vector3.Normalize(objectDifference) * this.weightings.nearestObject;

                    newVelocity += objectDifference;
                }

                var towardsNearestPeer = currentPosition - nearestPeerLocation;
                EnsureHasValue(ref towardsNearestPeer, this.weightings.randomInitialVelocity, random);

                SafeNormalise(ref towardsNearestPeer, this.weightings.nearestPeer);
                newVelocity += towardsNearestPeer;

                var centreDifference = currentPosition - packCentre;
                SafeNormalise(ref centreDifference, this.weightings.flockCentre);
                newVelocity -= centreDifference;

                if (headTowards != Vector3.Zero)
                {
                    var headingTowards = currentPosition - headTowards;
                    SafeNormalise(ref headingTowards, this.weightings.goal);
                    newVelocity -= headingTowards;
                }

                SafeNormalise(ref averagePeerVelocity, this.weightings.peerVelocity);
                newVelocity += averagePeerVelocity;

            }

            Clamp(ref newVelocity, this.maxVelocity);
            return newVelocity;
        }

        private void EnsureHasValue(ref Vector3 vector, float max, Random random)
        {
            if (vector.X == 0 && vector.Y == 0)
            {
                vector.X = (random.Next((int)(max * 2)) - (max / 2)) / 4f;
                vector.Y = (random.Next((int)(max * 2)) - (max / 2)) / 4f;
                vector.Z = (random.Next((int)(max * 2)) - (max / 2)) / 4f;
            }
        }

        private void SafeNormalise(ref Vector3 vector, float multiplyBy)
        {
            if (vector == Vector3.Zero)
            {
                return;
            }

            vector = Vector3.Normalize(vector) * multiplyBy;
        }

        private void Clamp(ref Vector3 toClamp, float max)
        {
            toClamp.X = MathHelper.Clamp(toClamp.X, -max, max);
            toClamp.Y = MathHelper.Clamp(toClamp.Y, -max, max);
            toClamp.Z = MathHelper.Clamp(toClamp.Z, -max, max);
        }

        private void AdjustWeighting(ref float toAdjust, float max, float total)
        {
            toAdjust = (toAdjust / total) * max;
        }
    }
}
