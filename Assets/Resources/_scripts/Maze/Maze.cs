using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GraphClasses;
using System.Linq;

public class Maze : MonoBehaviour {

    //size of maze in x by z
    public IntVector2 size;

    //location of exit
    public IntVector2 exitCoords;

    //prefabs to build physical maze
    public MazeCell cellPrefab;
    public MazeWall wallPrefab;
    public MazeDoor doorPrefab;
    public MazeHallway hallwayPrefab;

    //distance between rooms generated
    public int RoomSeparationDistance;

    //scales the entire maze up or down
    public float MazeScale;
    
    //hold the graph and the cells for the maze
    private MazeCell[,] cells;
    private GridGraph MazeGrid;

    public IntVector2 RandomCoordinates {
        get {
            return new IntVector2(UnityEngine.Random.Range(0, size.x), UnityEngine.Random.Range(0, size.z));
        }
    }

    public bool ValidCoordinate(IntVector2 coordinate) {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
    }

    public MazeCell GetCell(IntVector2 coordinate) {
        return cells[coordinate.x, coordinate.z];
    }
    
    /// <summary>
    /// This is the main function for creating the random maze.
    /// A random tree is created from a grid graph, and then maze rooms are
    /// created on nodes and maze doors / hallways on edges.
    /// </summary>     
    public void Generate() {
        MazeGrid = new GridGraph(size.x, size.z);
        cells = new MazeCell[MazeGrid.x, MazeGrid.y];
        MazeDirection[] dirs = new MazeDirection[] { MazeDirection.North, MazeDirection.South, MazeDirection.East, MazeDirection.West };

        //turn the grid graph into a spanning tree of the same nodes
        MazeGrid.RandomizedKruskals();

        //a cell will exist at each node
        for (int i = 0; i < MazeGrid.x; i++) {
            for (int j = 0; j < MazeGrid.y; j++) {
                CreateCell(new IntVector2(i, j));
            }
        }

        //decide whether to place walls or doors in each of the 4 directions around a cell
        for (int i = 0; i < MazeGrid.x; i++) {
            for (int j = 0; j < MazeGrid.y; j++) {
                foreach (var dir in dirs) {
                    IntVector2 move = dir.ToIntVector2();
                    IntVector2 newCoord = move + new IntVector2(i, j);
                    if (ValidCoordinate(newCoord)) {
                        GraphNode node1 = MazeGrid.grid[i, j];
                        GraphNode node2 = MazeGrid.grid[newCoord.x, newCoord.z];

                        //check if edge exists if the new coordinate was valid:
                        if (MazeGrid.GetEdge(node1, node2) == null) {
                            CreateWall(cells[i, j], GetCell(newCoord), dir);
                        }
                        else {
                            CreatePassage(cells[i, j], GetCell(newCoord), dir);
                        }
                    }
                    else {
                        CreateWall(cells[i, j], null, dir);
                    }                 
                }
            }
        }

        //loop through edges and create hallways
        foreach (var e in MazeGrid.edgeList) {            
            CreateHallway(e);
        }

        //make the maze bigger or smaller as desired
        transform.localScale = new Vector3(MazeScale, MazeScale, MazeScale);
    }

    /// <summary>
    /// Creates a hallway on the given edge. Gets coordinates of the nodes in the edge,
    /// translates into world coordinates, and places the properly stretched hallway
    /// between those coordinates
    /// </summary>
    /// <param name="edge"></param>
    private void CreateHallway(GraphEdge edge) {
        MazeHallway newHallway = Instantiate(hallwayPrefab) as MazeHallway;

        IntVector2 cellCoord1 = MazeGrid.GetNodeCoords(edge.node1);
        IntVector2 cellCoord2 = MazeGrid.GetNodeCoords(edge.node2);
        
        Vector3 cellPosition1 = GetCellLocalPosition(cellCoord1.x, cellCoord1.z);
        Vector3 cellPosition2 = GetCellLocalPosition(cellCoord2.x, cellCoord2.z);
        Vector3 hallwayPosition = (cellPosition1 + cellPosition2) / 2.0f;

        hallwayPosition.y += 0.3f;

        newHallway.transform.parent = transform;
        newHallway.transform.localPosition = hallwayPosition;
        newHallway.transform.localScale = new Vector3(RoomSeparationDistance - 0.95f, 0.6f, 0.41f);

        //decide which way the hallway should be rotated
        if (cellCoord1.z != cellCoord2.z) {
            newHallway.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeDoor passage = Instantiate(doorPrefab) as MazeDoor;
        passage.Initialize(cell, otherCell, direction);
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
    }

    private MazeCell CreateCell(IntVector2 coordinates) {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.name = "Cell " + coordinates.x + "," + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = GetCellLocalPosition(coordinates.x, coordinates.z);
        newCell.coordinates = coordinates;
        return newCell;
    }

    /// <summary>
    /// Translate x,z coordinates within the maze grid to actual cell position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public Vector3 GetCellLocalPosition(int x, int z) {
        return new Vector3 (
            RoomSeparationDistance * (x - (size.x * 0.5f)) + 0.5f,
            0.0f,
            RoomSeparationDistance * (z - (size.z * 0.5f)) + 0.5f
        );
    }

    /// <summary>
    /// Attach the given player's transform to the maze and put him in a random leaf cell.
    /// </summary>
    public IntVector2 PlacePlayerInMaze(Player player) {
        player.transform.parent = transform;
        
        var leafNodes = MazeGrid.nodeList.Where(n => n.neighbors.Count == 1);
        var randStartNode = leafNodes.ElementAt(Random.Range(0, leafNodes.Count()));
        IntVector2 coords = MazeGrid.GetNodeCoords(randStartNode);

        Vector3 playerPos = GetCellLocalPosition(coords.x, coords.z);
        playerPos.y += 0.2f;

        player.MovePlayerToPosition(playerPos);

        return coords;        
    }

    /// <summary>
    /// Determine the location of the exit cell (farthest possible from player) and transform that cell into
    /// the exit by calling TurnCellIntoExit
    /// </summary>
    /// <param name="playerPosition"></param>
    /// <returns>The coordinates of the exit cell</returns>
    public IntVector2 PlaceExitCell(IntVector2 playerPosition) {
        var pathDict = MazeGrid.GetShortestPathsForTree(MazeGrid.grid[playerPosition.x, playerPosition.z]);

        int longestLenSoFar = 0;
        IntVector2 exitCoords = playerPosition;
        foreach (var n in pathDict.Keys) {
            if (pathDict[n].pathLength > longestLenSoFar) {
                longestLenSoFar = pathDict[n].pathLength;
                exitCoords = MazeGrid.GetNodeCoords(n);
            }
        }

        this.exitCoords = exitCoords;

        //TurnCellIntoExit(playerPosition);
        TurnCellIntoExit(exitCoords);

        return exitCoords;
    }

    private void TurnCellIntoExit(IntVector2 exitCoords) {
        //cells[exitCoords.x, exitCoords.z].transform.localPosition += new Vector3(0, 3.0f, 0);
    }
}
