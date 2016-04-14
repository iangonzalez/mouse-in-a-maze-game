using UnityEngine;
using System;
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
    public float RoomSeparationDistance;

    //scales the entire maze up or down
    public float MazeScale;

    //hold hallways
    private Dictionary<GraphEdge, MazeHallway> hallways = new Dictionary<GraphEdge, MazeHallway>();

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
        if (transform.localScale != new Vector3(1f,1f,1f)) {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }


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

    public void DestroyCurrentMaze() {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.z; j++) {
                MazeCell cell = cells[i, j];
                foreach (MazeDirection dir in Enum.GetValues(typeof(MazeDirection))) {
                    DestroyMazeCellEdge(cell, dir);
                }
                DestroyMazeCell(cell);
            }
        }
        foreach (var e in MazeGrid.edgeList) {
            DestroyHallway(e);
        }
    }

    private Vector3 GetHallwayPosition(GraphEdge edge) {
        IntVector2 cellCoord1 = MazeGrid.GetNodeCoords(edge.node1);
        IntVector2 cellCoord2 = MazeGrid.GetNodeCoords(edge.node2);

        Vector3 cellPosition1 = GetCellLocalPosition(cellCoord1.x, cellCoord1.z);
        Vector3 cellPosition2 = GetCellLocalPosition(cellCoord2.x, cellCoord2.z);
        Vector3 hallwayPosition = (cellPosition1 + cellPosition2) / 2.0f;
        hallwayPosition.y += 0.3f;

        return hallwayPosition;
    }

    private Vector3 GetHallwayScale() {
        return new Vector3(RoomSeparationDistance - 0.95f, 0.6f, 0.41f);
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

        Vector3 hallwayPosition = GetHallwayPosition(edge);

        newHallway.transform.parent = transform;
        newHallway.transform.localPosition = hallwayPosition;
        

        //decide which way the hallway should be rotated
        if (cellCoord1.z != cellCoord2.z) {
            newHallway.RotateHallway();
        }

        newHallway.StretchHallway(GetHallwayScale());

        //add the hallway to the list
        hallways[edge] = newHallway;
    }

    private void DestroyHallway(GraphEdge edge) {
        Destroy(hallways[edge].gameObject);
    }

    private void DestroyMazeCellEdge(MazeCell cell, MazeDirection dir) {
        MazeCellEdge edge = cell.GetEdge(dir);
        Destroy(edge.gameObject);
    }

    private void DestroyMazeCell(MazeCell cell) {
        Destroy(cell.gameObject);
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
        //newCell.transform.localScale = new Vector3(MazeScale, MazeScale, MazeScale);
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
        var randStartNode = leafNodes.ElementAt(UnityEngine.Random.Range(0, leafNodes.Count()));
        IntVector2 coords = MazeGrid.GetNodeCoords(randStartNode);

        player.InitializePlayerCoords(coords);

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
        cells[exitCoords.x, exitCoords.z].MakeThisExitCell();
    }

    public void CloseDoorsInCell(IntVector2 cellCoords, bool doItInstantly = false) {
        MazeCell cellToClose = cells[cellCoords.x, cellCoords.z];
        MazeDoor[] doorsToClose = cellToClose.GetComponentsInChildren<MazeDoor>();

        foreach (var door in doorsToClose) {
            if (doItInstantly) {
                door.CloseDoorInstantly();
            }
            else {
                door.CloseDoor();
            }
        }
    }

    public void OpenDoorsInCell(IntVector2 cellCoords, bool doItInstantly = false) {
        MazeCell cellToClose = cells[cellCoords.x, cellCoords.z];
        MazeDoor[] doorsToClose = cellToClose.GetComponentsInChildren<MazeDoor>();

        foreach (var door in doorsToClose) {
            if (doItInstantly) {
                door.OpenDoorInstantly();
            }
            else {
                door.OpenDoor();
            }
        }
    }

    public void TurnAllLightsRed() {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.z; j++) {
                cells[i, j].TurnLightRed();
            }
        }
        foreach (MazeHallway hallway in hallways.Values) {
            hallway.TurnLightsRed();
        }
    }

    public void AddSignpostToCell(IntVector2 coords, MazeDirection dir, Vector3 playerPosition) {
        cells[coords.x, coords.z].AddSignPost(dir, playerPosition);
    }

    public void RemoveAllSignPosts() {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.z; j++) {
                cells[i, j].RemoveSignPost();
            }
        }
    }

    //gets the list of MazeCells on the path from the player to the exit cell
    public List<IntVector2> GetPathToExit(IntVector2 playerCoords) {
        //get shortest paths to all nodes from exit
        GraphNode exitNode = MazeGrid.grid[exitCoords.x, exitCoords.z];
        Dictionary<GraphNode, Path> pathsFromExit = MazeGrid.GetShortestPathsForTree(exitNode);

        //get shortest path to player's coordinates
        GraphNode playerNode = MazeGrid.grid[playerCoords.x, playerCoords.z];
        Path pathToPlayer = pathsFromExit[playerNode];

        //reverse list to get path to exit instead
        pathToPlayer.nodeList.Reverse();

        //get maze cells corresponding to graph nodes and return it
        List<IntVector2> cellPath = new List<IntVector2>(
            pathToPlayer.nodeList.Select(n => MazeGrid.GetNodeCoords(n))
            );
        return cellPath;
    }

    private MazeDirection? UnconnectedNeighborWithShortestOrLongestPathToExit(IntVector2 coords, 
        List<IntVector2> pathToExit, bool findShortest = true) {
        MazeDirection? retval = null;

        GraphNode n1 = MazeGrid.grid[coords.x, coords.z];
        foreach (MazeDirection dir in Enum.GetValues(typeof(MazeDirection))) {
            IntVector2 newCoords = coords + dir.ToIntVector2();
            if (ValidCoordinate(newCoords)) {
                GraphNode n2 = MazeGrid.grid[newCoords.x, newCoords.z];

                if (MazeGrid.GetEdge(n1, n2) == null) {
                    List<IntVector2> neighborPathToExit = GetPathToExit(newCoords);

                    bool pathIsShorter = (neighborPathToExit.Count < pathToExit.Count);
                    bool pathIsntEqual = (neighborPathToExit.Count != pathToExit.Count);

                    if ((pathIsShorter == findShortest) && pathIsntEqual) {
                        retval = dir;
                    }
                }
            }
        }

        return retval;
    }

    private void DestroyHallwayAndWallItsDoors(IntVector2 cell1Coords, IntVector2 cell2Coords) {
        if (!ValidCoordinate(cell1Coords) || !ValidCoordinate(cell2Coords)) {
            Debug.LogError("trying to destroy hallway between nonexistent nodes.");
            return;
        }

        MazeDirection removeDir = (cell2Coords - cell1Coords).ToDirection();
        GraphNode n1 = MazeGrid.grid[cell1Coords.x, cell1Coords.z];
        GraphNode oldNeighbor = MazeGrid.grid[cell2Coords.x, cell2Coords.z];

        //destroy the old hallway.
        DestroyHallway(MazeGrid.GetEdge(n1, oldNeighbor));
        MazeGrid.RemoveEdge(n1, oldNeighbor);

        //destroy that hallway's doors and replace with walls:
        DestroyMazeCellEdge(GetCell(cell1Coords), removeDir);
        DestroyMazeCellEdge(GetCell(cell2Coords), removeDir.GetOpposite());

        CreateWall(GetCell(cell1Coords), GetCell(cell2Coords), removeDir);
        CreateWall(GetCell(cell2Coords), GetCell(cell1Coords), removeDir.GetOpposite());
    }

    private void ReplaceWallsWithNewHallwayAndDoors(IntVector2 cell1Coords, IntVector2 cell2Coords, 
                                                    bool greenHallway = false, bool redHallway = false) {
        if (!ValidCoordinate(cell1Coords) || !ValidCoordinate(cell2Coords)) {
            Debug.LogError("trying to destroy hallway between nonexistent nodes.");
            return;
        }

        MazeDirection bestDir = (cell2Coords - cell1Coords).ToDirection();
        GraphNode n1 = MazeGrid.grid[cell1Coords.x, cell1Coords.z];
        GraphNode newNeighbor = MazeGrid.grid[cell2Coords.x, cell2Coords.z];

        //destroy the old walls where the new edge is being added and add doors:
        DestroyMazeCellEdge(GetCell(cell1Coords), bestDir);
        DestroyMazeCellEdge(GetCell(cell2Coords), bestDir.GetOpposite());

        CreatePassage(GetCell(cell1Coords), GetCell(cell2Coords), bestDir);
        CreatePassage(GetCell(cell2Coords), GetCell(cell1Coords), bestDir.GetOpposite());

        //create the new hallway:
        MazeGrid.CreateEdge(n1, newNeighbor);
        CreateHallway(MazeGrid.GetEdge(n1, newNeighbor));

        var newhallway = hallways[MazeGrid.GetEdge(n1, newNeighbor)];
        var walls = newhallway.transform.FindChild("walls");

        if (redHallway || greenHallway) {
            Color hallColor = redHallway ? Color.red : Color.green;
            foreach (Transform child in walls.transform) {
                Renderer rend = child.gameObject.GetComponent<Renderer>();
                rend.material.color = hallColor;
            }
        }
        
    }

    public MazeDirection? CreateShortOrLongCut(IntVector2 playerCoords, bool isShortcut) {
        List<IntVector2> pathToExit = GetPathToExit(playerCoords);

        if (pathToExit.Count < 2) {
            Debug.LogError("trying to make shortcut from exit.");
            return null; //this should only happen if the player is at the exit
        }

        MazeDirection? bestDirMaybe = 
            UnconnectedNeighborWithShortestOrLongestPathToExit(playerCoords, pathToExit, isShortcut);
        if (bestDirMaybe == null) {
            return null;
        }

        MazeDirection bestDir = bestDirMaybe.GetValueOrDefault();

        IntVector2 bestNeighborChoice = playerCoords + bestDir.ToIntVector2();
        IntVector2 neighborToRemove = pathToExit[1];

        DestroyHallwayAndWallItsDoors(playerCoords, neighborToRemove);
        ReplaceWallsWithNewHallwayAndDoors(playerCoords, bestNeighborChoice, 
                                           greenHallway: isShortcut, redHallway: !isShortcut);

        return bestDirMaybe;
    }

    public MazeDirection? CreateShortcutIfPossible(IntVector2 playerCoords) {
        return CreateShortOrLongCut(playerCoords, isShortcut: true);
    }

    public MazeDirection? LengthenPathToExitIfPossible(IntVector2 playerCoords) {
        return CreateShortOrLongCut(playerCoords, isShortcut: false);
    }


    private void AddCoordsToCell(IntVector2 coords) {
        MazeCell cell = GetCell(coords);
        var walls = cell.GetComponentsInChildren<MazeWall>();
        foreach (var wall in walls) {
            wall.AddCoordText(coords);
        }
    }

    public void AddCoordsToAllCells() {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.z; j++) {
                AddCoordsToCell(new IntVector2(i, j));
            }
        }
    }


    //fields and functions to lengthen the hallways
    private bool changeHallwayLength = false;
    private float newRoomSepDist = 0f;
    private Player player;
    public void ChangeHallwayLength(float newRoomSeparationDist, Player player) {
        changeHallwayLength = true;
        newRoomSepDist = newRoomSeparationDist;
        this.player = player;
    }

    void Update() {
        if (changeHallwayLength && (newRoomSepDist > RoomSeparationDistance)) {
            RoomSeparationDistance += 0.1f;

            for (int i = 0; i < size.x; i++) {
                for (int j = 0; j < size.z; j++) {
                    cells[i, j].transform.localPosition = GetCellLocalPosition(i, j);
                    if (player.MazeCellCoords == new IntVector2(i,j)) {
                        player.transform.localPosition = GetCellLocalPosition(i, j) + new Vector3(0, 0.2f, 0);
                    }
                }
            }


            foreach (var e in hallways.Keys) {
                hallways[e].transform.localPosition = GetHallwayPosition(e);
                hallways[e].StretchHallway(GetHallwayScale());
            }

            if (newRoomSepDist <= RoomSeparationDistance) {
                changeHallwayLength = false;
                player = null;
                newRoomSepDist = 0;
            }
        }
    }
}