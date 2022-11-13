using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace justbake.astar
{
	public class AStar : MonoBehaviour
	{
		public Grid grid;
		public Tilemap walkable;

		public bool useDiagonals = true;

		public Vector3Int startPos;
		public Vector3Int endPos;

		Node[,,] nodeGrid;
		List<Node> path;

		private void Start()
		{
			CreateGrid();
			FindPath(startPos, endPos);
		}

		void CreateGrid()
		{
			nodeGrid = new Node[walkable.size.x, walkable.size.y, walkable.size.z];

			for(int x = walkable.cellBounds.xMin; x < walkable.cellBounds.xMax; x++)
			{
				for (int y = walkable.cellBounds.yMin; y < walkable.cellBounds.yMax; y++)
				{
					for (int z = walkable.cellBounds.zMin; z < walkable.cellBounds.zMax; z++)
					{
						Vector3Int pos = new Vector3Int(x, y, z);
						bool isWalkable = walkable.GetTile(pos) == null;
						nodeGrid[x + Mathf.Abs(walkable.cellBounds.xMin), y + Mathf.Abs(walkable.cellBounds.yMin), z + Mathf.Abs(walkable.cellBounds.zMin)] = new Node(isWalkable, pos);
					}
				}
			}
		}

		private void OnValidate()
		{
			CreateGrid();
			FindPath(startPos, endPos);
		}

		private void OnDrawGizmos()
		{
			if(nodeGrid != null)
			{
				foreach (Node n in nodeGrid)
				{
					Vector3 tileWorldPos = walkable.CellToWorld(n.position) + new Vector3(0.5f, 0.5f, 0);
					//Gizmos.DrawCube(Vector3.zero, Vector3.one);
					if ((path != null && path.Contains(n)) || n.position == endPos || n.position == startPos)
					{
						Gizmos.color = Color.cyan;
						if (n.position == endPos) Gizmos.color = Color.red;
						if (n.position == startPos) Gizmos.color = Color.green;
						Gizmos.DrawCube(tileWorldPos, grid.cellSize);
					}
				}
			}
		}

		void FindPath(Vector3Int startPos, Vector3Int targetPos)
		{
			Node startNode = GetNodeFromArray(startPos);
			Node targetNode = GetNodeFromArray(targetPos);

			List<Node> openSet = new List<Node>();
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node node = openSet[0];
				for (int i = 1; i < openSet.Count; i++)
				{
					if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost)
					{
						if (openSet[i].hCost < node.hCost)
							node = openSet[i];
					}
				}

				openSet.Remove(node);
				closedSet.Add(node);

				if (node == targetNode)
				{
					RetracePath(startNode, targetNode);
					return;
				}

				foreach (Node neighbour in GetNeighbours(node))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

					int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
					if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = node;

						if (!openSet.Contains(neighbour))
						{
							openSet.Add(neighbour);
						}
					}
				}
			}
		}

		private List<Node> GetNeighbours(Node node)
		{
			List<Node> neighbours = new List<Node>();

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0 || ((Mathf.Abs(x)==1&& Mathf.Abs(y) == 1) && !useDiagonals))
						continue;

					int checkX = node.position.x + Mathf.Abs(walkable.cellBounds.xMin) + x;
					int checkY = node.position.y + Mathf.Abs(walkable.cellBounds.yMin) + y;

					if (checkX >= 0 && checkX < walkable.size.x && checkY >= 0 && checkY < walkable.size.y)
					{
						neighbours.Add(nodeGrid[checkX, checkY, 0]);
					}
				}
			}
			return neighbours;
		}

		void RetracePath(Node startNode, Node endNode)
		{
			List<Node> path = new List<Node>();
			Node currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}
			path.Reverse();

			this.path = path;

		}

		private int GetDistance(Node a, Node b)
		{
			Vector3Int arrayPosA = GetArrayIndex(a.position);
			Vector3Int arrayPosB = GetArrayIndex(b.position);
			int xDist = Mathf.Abs(arrayPosA.x - arrayPosB.x);
			int yDist = Mathf.Abs(arrayPosA.y - arrayPosB.y);
			return (xDist > yDist) ? yDist * 14 + (xDist-yDist) * 10 : xDist * 14 + (yDist - xDist) * 10;
		}

		private bool IsInArrayBounds(Vector3Int position)
		{
			return position.x <= walkable.size.x && position.y <= walkable.size.y && position.z <= walkable.size.z &&
					position.x >= 0 && position.y >= 0 && position.z >= 0;
		}

		private Vector3Int GetTileIndex(Vector3Int pos)
		{
			return new Vector3Int(pos.x - Mathf.Abs(walkable.cellBounds.xMin), pos.y - Mathf.Abs(walkable.cellBounds.yMin), pos.z - Mathf.Abs(walkable.cellBounds.zMin));
		}

		private Vector3Int GetArrayIndex(Vector3Int pos)
		{
			return new Vector3Int(pos.x + Mathf.Abs(walkable.cellBounds.xMin), pos.y + Mathf.Abs(walkable.cellBounds.yMin), pos.z + Mathf.Abs(walkable.cellBounds.zMin));
		}

		private Node GetNodeFromArray(Vector3Int pos)
		{
			Vector3Int ArrayIndex = GetArrayIndex(pos);
			return nodeGrid[ArrayIndex.x, ArrayIndex.y, ArrayIndex.z];
		}
	}
}
