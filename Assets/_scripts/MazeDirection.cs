using UnityEngine;
using System.Collections;

public enum MazeDirection {
    North,
    South,
    East,
    West
}

public static class MazeDirections {
    public const int DirectionCount = 4;

    public static IntVector2[] directionVectors = {
        new IntVector2(0, 1),
        new IntVector2(0, -1),
        new IntVector2(1, 0),
        new IntVector2(-1, 0)
    };

    private static MazeDirection[] opposites = {
        MazeDirection.South,
        MazeDirection.North,
        MazeDirection.West,
        MazeDirection.East
    };

    private static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 270f, 0f)
    };

    public static Quaternion ToRotation(this MazeDirection direction) {
        return rotations[(int)direction];
    }

    public static MazeDirection RandDirection {
        get {
            return (MazeDirection)Random.Range(0, DirectionCount);
        }
    }

    public static IntVector2 ToIntVector2 (this MazeDirection dir) {
        return directionVectors[(int)dir];
    }

    

    public static MazeDirection GetOpposite(this MazeDirection direction) {
        return opposites[(int)direction];
    }

}
