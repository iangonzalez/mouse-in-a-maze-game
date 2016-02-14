using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GraphClasses;

public class Maze : MonoBehaviour {
    public IntVector2 size;
    public MazeCell cellPrefab;
    public MazePassage passagePrefab;
    public MazeWall wallPrefab;
    public MazeDoor doorPrefab;
    public MazeHallway hallwayPrefab;

    public int RoomSeparationDistance;
    public float MazeScale;
    

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
         
    public void Generate() {
        MazeGrid = new GridGraph(size.x, size.z);
        cells = new MazeCell[MazeGrid.x, MazeGrid.y];
        MazeDirection[] dirs = new MazeDirection[] { MazeDirection.North, MazeDirection.South, MazeDirection.East, MazeDirection.West };

        MazeGrid.RandomizedKruskals();
        //MazeGrid.RandomConnectedPartition(3, 7);

        for (int i = 0; i < MazeGrid.x; i++) {
            for (int j = 0; j < MazeGrid.y; j++) {
                CreateCell(new IntVector2(i, j));
            }
        }


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

        if (cellCoord1.z != cellCoord2.z) {
            newHallway.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeDoor passage = Instantiate(doorPrefab) as MazeDoor;
        passage.Initialize(cell, otherCell, direction);
        //passage = Instantiate(passagePrefab) as MazePassage;
        //passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null) {
            wall = Instantiate(wallPrefab) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
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

    private Vector3 GetCellLocalPosition(int x, int z) {
        return new Vector3 (
            RoomSeparationDistance * (x - (size.x * 0.5f)) + 0.5f,
            0.0f,
            RoomSeparationDistance * (z - (size.z * 0.5f)) + 0.5f
        );
    }
}
