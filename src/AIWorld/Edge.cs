namespace AIWorld
{
    public class Edge
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Edge" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="distance">The distance.</param>
        public Edge(Node target, float distance)
        {
            Distance = distance;
            Target = target;
        }

        public float Distance { get; private set; }
        public Node Target { get; set; }
    }
}