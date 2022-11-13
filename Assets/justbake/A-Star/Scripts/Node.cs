using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace justbake.astar
{
    public class Node
    {
        public bool walkable;
        public Vector3Int position;
        public Node parent;

        public int gCost;
        public int hCost;
        public int fCost
		{
			get => gCost + hCost;
		}

        public Node(bool walkable, Vector3Int position)
		{
            this.walkable = walkable;
            this.position = position;
		}

        public override string ToString()
		{
            return $"{walkable}, {position}, {parent}, {gCost}, {hCost}, {fCost}";
		}
    }
}
