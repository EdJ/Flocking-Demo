namespace Flocking.Lib
{
    public struct BrainWeightings
    {
        public float randomInitialVelocity;
        public float nearestPeer;
        public float flockCentre;
        public float nearestObject;
        public float peerVelocity;
        public float goal;

        public float Total()
        {
            return this.randomInitialVelocity +
                this.nearestPeer +
                this.flockCentre +
                this.nearestObject +
                this.peerVelocity +
                this.goal;
        }
    }
}
