namespace Flocking.Lib
{
    using Microsoft.Xna.Framework;
    using System;

    public class Agent
    {
        public Agent(AgentBrain brain, Vector3 position)
        {
            this.Position = position;
            this.Velocity = Vector3.Zero;
            this.Brain = brain;
        }

        public Vector3 Position;

        public Vector3 Velocity;

        public AgentBrain Brain { get; set; }

        public void UpdateVelocity(Random random, ref Vector3 nearestPeerLocation, ref Vector3 averagePeerVelocity, ref Vector3 nearestObjectLocation, ref Vector3 packCentre, ref Vector3 headTowards)
        {
            Vector3 newVelocity = this.Brain.GetNewVelocity(random, ref this.Position, ref averagePeerVelocity, ref nearestPeerLocation, ref nearestObjectLocation, ref packCentre, ref headTowards);
            this.Velocity += newVelocity;

            this.Velocity.X = MathHelper.Clamp(this.Velocity.X, -6, 6);
            this.Velocity.Y = MathHelper.Clamp(this.Velocity.Y, -6, 6);
        }

        public void UpdatePosition(int divisor)
        {
            this.Position += Velocity / divisor;
            this.Position.Z = MathHelper.Clamp(this.Position.Z, -100, 100);
        }
    }
}
