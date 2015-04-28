using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AIWorld
{
    public class Node : List<Edge>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Node" /> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public Node(Vector3 position)
        {
            Position = position;
        }

        public float Distance { get; set; }
        public Node Previous { get; set; }
        public Vector3 Position { get; private set; }
    }
}