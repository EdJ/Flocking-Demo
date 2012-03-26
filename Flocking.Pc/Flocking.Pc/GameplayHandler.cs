namespace Flocking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Flocking.Lib;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class GameplayHandler
    {
        private GraphicsDevice GraphicsDevice;

        private Agent[] Agents = new Agent[MaxAiThreads * MaxAgentsPerThread];
        private BoundingBox[] Obstacles = new BoundingBox[MaxObstacles];

        private Texture2D AgentTexture;
        private Texture2D ObstacleTexture;

        private int TickDivisor = 4;
        private int CurrentTickDivisor = 0;

        private const int MaxAiThreads = 2;
        private const int MaxAgentsPerThread = 100;
        private const int MaxObstacles = 30;

        private Task[] tasks = new Task[MaxAiThreads];
        private Vector3[][] AllocatedPoints = new Vector3[MaxAiThreads][];
        private Random[] randoms = new Random[MaxAiThreads];

        private struct UpdateInformation
        {
            public Vector3 packCentre;
            public Vector3 packVelocity;
            public Vector3 headTowards;
        }

        private UpdateInformation updateInfo;

        private Vector3 Centre;

        #region Updating

        public void Update(GameTime gameTime)
        {
            foreach (var agent in this.Agents)
            {
                agent.UpdatePosition(this.TickDivisor);
            }

            var current = this.CurrentTickDivisor % this.TickDivisor;
            this.CurrentTickDivisor++;
            if (current != 0)
            {
                return;
            }

            var packCentre = new Vector3();
            var packVelocity = new Vector3();
            foreach (var agent in this.Agents)
            {
                packCentre += agent.Position;
                packVelocity += agent.Velocity;
            }

            this.updateInfo.packCentre = packCentre / this.Agents.Length;
            this.updateInfo.packVelocity = packVelocity / this.Agents.Length;

            var mousePosition = Mouse.GetState();
            if (mousePosition.LeftButton == ButtonState.Pressed)
            {
                this.updateInfo.headTowards = new Vector3(mousePosition.X, mousePosition.Y, 0);
            }
            else if (this.updateInfo.headTowards != Vector3.Zero)
            {
                this.updateInfo.headTowards = Vector3.Zero;
            }

            var perTask = this.Agents.Length / MaxAiThreads;

            for (int i = 0; i < MaxAiThreads; i++)
            {
                var iPrime = i;
                this.tasks[i] = Task.Factory.StartNew(() => UpdateAgentGroup(iPrime, perTask));
            }

            Task.WaitAll(this.tasks);
        }

        private void UpdateAgentGroup(int threadNumber, int numberToProcess)
        {
            var newStart = threadNumber * numberToProcess;
            var random = this.randoms[threadNumber];

            var points = AllocatedPoints[threadNumber];

            for (int i = newStart; i < newStart + numberToProcess && i < this.Agents.Length; i++)
            {
                var agent = this.Agents[i];
                Vector3 nearestPeerLocation = this.GetNearestPoint(ref agent.Position, this.Agents);
                Vector3 nearestObstacleLocation = this.GetNearestPoint(ref agent.Position, this.Obstacles, ref points);

                agent.UpdateVelocity(random, ref nearestPeerLocation, ref this.updateInfo.packVelocity, ref nearestObstacleLocation, ref this.updateInfo.packCentre, ref this.updateInfo.headTowards);

                ClampToScreenArea(ref agent.Position, ref agent.Velocity);
            }
        }

        private void ClampToScreenArea(ref Vector3 position, ref Vector3 velocity)
        {
            if ((position.X < 20 && velocity.X < 0)
                 ||
                 (position.X > this.GraphicsDevice.Viewport.Width - 20 && velocity.X > 0))
            {
                velocity.X = -velocity.X * 0.5f;
            }

            if ((position.Y < 20 && velocity.Y < 0)
                || (position.Y > this.GraphicsDevice.Viewport.Height - 20 && velocity.Y > 0))
            {
                velocity.Y = -velocity.Y * 0.5f;
            }
        }

        private Vector3 GetNearestPoint(ref Vector3 currentPosition, BoundingBox[] possibleRectangles, ref Vector3[] points)
        {
            Vector3 nearestLocation = Vector3.Zero;
            Vector3 nearestDifference = new Vector3(2000, 2000, 2000);

            bool changed = false;

            float nearestDistance = nearestDifference.Length();

            for (int i = 0; i < possibleRectangles.Length; i++)
            {
                var box = possibleRectangles[i];

                Vector3 nearest;
                this.GetNearestPoint(ref box, ref currentPosition, ref points, out nearest);
                var diff = nearest - currentPosition;
                var dl = diff.Length();
                if (dl < nearestDistance && dl != 0)
                {
                    changed = true;

                    nearestDifference = diff;
                    nearestDistance = diff.Length();
                    nearestLocation = nearest;
                }
            }

            if (!changed)
            {
                nearestLocation.X = 0;
                nearestLocation.Y = 0;
                nearestLocation.Z = 0;
            }

            return nearestLocation;
        }

        private Vector3 GetNearestPoint(ref Vector3 currentPosition, IEnumerable<Agent> possiblePoints)
        {
            Vector3 nearestLocation = Vector3.Zero;
            Vector3 nearestDifference = new Vector3(2000, 2000, 2000);

            bool changed = false;

            float nearestDistance = nearestDifference.Length();

            foreach (var point in possiblePoints)
            {
                var diff = point.Position - currentPosition;
                var dl = diff.Length();
                if (dl < nearestDistance && dl != 0)
                {
                    changed = true;

                    nearestDifference = diff;
                    nearestDistance = diff.Length();
                    nearestLocation = point.Position;
                }
            }

            if (!changed)
            {
                nearestLocation.X = 0;
                nearestLocation.Y = 0;
                nearestLocation.Z = 0;
            }

            return nearestLocation;
        }

        private void GetNearestPoint(ref BoundingBox r, ref Vector3 position, ref Vector3[] points, out Vector3 toSet)
        {
            Vector3[] corners = r.GetCorners();

            GetNearestPoint(ref corners[0], ref corners[1], ref position, ref points[0]);
            GetNearestPoint(ref corners[1], ref corners[2], ref position, ref points[1]);
            GetNearestPoint(ref corners[2], ref corners[3], ref position, ref points[2]);
            GetNearestPoint(ref corners[3], ref corners[0], ref position, ref points[3]);

            float minLength = float.MaxValue;
            int toReturn = 0;

            for (int i = 0; i < points.Length; i++)
            {
                var item = points[i];
                var length = (item - position).Length();
                if (length < minLength)
                {
                    toReturn = i;
                }
            }

            toSet = points[toReturn];
        }

        private void GetNearestPoint(ref Vector3 a, ref Vector3 b, ref Vector3 position, ref Vector3 toSet)
        {
            var ap = new Vector2(position.X - a.X, position.Y - a.Y);
            var ab = new Vector2(b.X - a.X, b.Y - a.Y);

            var abMag = (ab.X * ab.X) + (ab.Y * ab.Y);
            var dotProd = (ab.X * ap.X) + (ab.Y * ap.Y);

            var t = dotProd / abMag;

            t = MathHelper.Clamp(t, 0, 1);

            var outX = a.X + (ab.X * t);
            var outY = a.Y + (ab.Y * t);

            toSet.X = outX;
            toSet.Y = outY;
            toSet.Z = position.Z;
        }

        #endregion

        #region Drawing
        
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var agent in this.Agents)
            {
                if (agent.Position.Z > 0)
                {
                    continue;
                }

                this.DrawAgent(spriteBatch, agent);
            }

            this.DrawObstacles(spriteBatch);

            foreach (var agent in this.Agents)
            {
                if (agent.Position.Z <= 0)
                {
                    continue;
                }

                this.DrawAgent(spriteBatch, agent);
            }
        }

        private void DrawAgent(SpriteBatch spriteBatch, Agent agent)
        {
            Rectangle toDraw = new Rectangle((int)agent.Position.X, (int)agent.Position.Y, this.AgentTexture.Width, this.AgentTexture.Height);

            var colour = (150 + agent.Position.Z / 2) / 250;

            spriteBatch.Draw(this.AgentTexture, toDraw, new Color(colour, colour, colour, colour));
        }

        private void DrawObstacles(SpriteBatch spriteBatch)
        {
            foreach (var obstacle in this.Obstacles)
            {
                Vector3[] corners = obstacle.GetCorners();

                var width = (int)(corners[1].X - corners[3].X);
                var height = (int)(corners[1].Y - corners[3].Y);
                Rectangle toDraw = new Rectangle((int)corners[3].X - (width / 2), (int)corners[3].Y - (height / 2), width, height);

                var colour = (150 + obstacle.Max.Z) / 250;

                spriteBatch.Draw(this.ObstacleTexture, toDraw, new Color(colour, 0, 0, colour));
            }
        }
        
        #endregion

        #region Setup

        private int NewObstacle(int number)
        {
            var topLeft = GetNextRandomPosition();

            Vector3 topLeftFront = new Vector3((int)topLeft.X, (int)topLeft.Y, (int)topLeft.Z);
            Vector3 bottomRightBack = new Vector3(topLeftFront.X + this.ObstacleTexture.Width * (this.randoms[0].Next(4) + 1), topLeftFront.Y + this.ObstacleTexture.Height * (this.randoms[0].Next(4) + 1), topLeft.Z);

            this.Obstacles[number] = new BoundingBox(topLeftFront, bottomRightBack);

            return number;
        }

        public void Initialise(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;

            Centre = new Vector3(this.GraphicsDevice.Viewport.Width / 2, this.GraphicsDevice.Viewport.Height / 2, 0);

            for (int i = 0; i < MaxAiThreads; i++)
            {
                this.randoms[i] = new Random();
            }

            BrainWeightings weightings;
            weightings.flockCentre = 5f;
            weightings.goal = 5f;
            weightings.nearestPeer = 5f;
            weightings.nearestObject = 10f;
            weightings.randomInitialVelocity = 10f;
            weightings.peerVelocity = 3f;

            AgentBrain agentBrain = new AgentBrain(weightings, 4f);

            Enumerable.Range(0, this.Agents.Length).Select(x => this.Agents[x] = new Agent(agentBrain, GetNextRandomPosition())).ToList();

            Enumerable.Range(0, this.AllocatedPoints.Length).Select(x => this.AllocatedPoints[x] = new Vector3[4]).ToList();
        }

        public void LoadContent(ContentManager contentManager)
        {
            this.ObstacleTexture = contentManager.Load<Texture2D>("flocking-obstacle");

            Enumerable.Range(0, this.Obstacles.Length).Select(x => this.NewObstacle(x)).ToList();

            this.AgentTexture = contentManager.Load<Texture2D>("flocking-agent");
        }

        private HashSet<Vector3> usedPositions = new HashSet<Vector3>();

        private Vector3 GetNextRandomPosition()
        {
            var possible = GetRandomPosition();
            while (usedPositions.Contains(possible))
            {
                possible = GetRandomPosition();
            }

            return possible;
        }

        private Vector3 GetRandomPosition()
        {
            var x = this.randoms[0].Next(this.GraphicsDevice.Viewport.Width);
            var y = this.randoms[0].Next(this.GraphicsDevice.Viewport.Height);
            var z = this.randoms[0].Next(40) - 20;

            return new Vector3(x, y, z);
        }

        #endregion
    }
}
