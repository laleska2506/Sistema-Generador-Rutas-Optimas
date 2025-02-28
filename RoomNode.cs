using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode : Node<Vector2Int>
{
    MazeGenerator maze;

    public RoomNode(Vector2Int value, MazeGenerator maze) : base(value)
    {
        this.maze = maze;
    }

    public override List<Node<Vector2Int>> GetNeighbours()
    {
        return maze.GetNeighbours(Value.x, Value.y);
    }
}
