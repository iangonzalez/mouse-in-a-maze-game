using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GraphClasses;
using System;

public class Maze : MonoBehaviour {
    public IntVector2 size;
    public MazeCell cellPrefab;
    public MazePassage passagePrefab;
    public MazeWall wallPrefab;
    private MazeCell[,] cells;

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
         
    public IEnumerator Generate() {
        GridGraph mazeGrid = new GridGraph(size.x, size.z);
        cells = new MazeCell[mazeGrid.x, mazeGrid.y];
        MazeDirection[] dirs = new MazeDirection[] { MazeDirection.North, MazeDirection.South, MazeDirection.East, MazeDirection.West };

        mazeGrid.RandomizedKruskals();
        //mazeGrid.RandomConnectedPartition(3, 7);

        for (int i = 0; i < mazeGrid.x; i++) {
            for (int j = 0; j < mazeGrid.y; j++) {
                CreateCell(new IntVector2(i, j));
            }
        }

        for (int i = 0; i < mazeGrid.x; i++) {
            for (int j = 0; j < mazeGrid.y; j++) {
                yield return 1;
                
                foreach (var dir in dirs) {
                    IntVector2 move = dir.ToIntVector2();
                    IntVector2 newCoord = move + new IntVector2(i, j);
                    if (ValidCoordinate(newCoord)) {
                        if (mazeGrid.GetEdge(mazeGrid.grid[i,j], mazeGrid.grid[newCoord.x, newCoord.z]) == null) {
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
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
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
        newCell.transform.localPosition = new Vector3(
            coordinates.x - (size.x * 0.5f) + 0.5f,
            0.0f,
            coordinates.z - (size.z * 0.5f) + 0.5f
            );
        newCell.coordinates = coordinates;
        return newCell;
    }
}
