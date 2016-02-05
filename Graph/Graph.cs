using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphClasses {

    //represents a node in a graph. contains neighbor, edge information and a weight value
    public class GraphNode {
        public int weight;
        public List<GraphNode> neighbors = new List<GraphNode>();
        public List<GraphEdge> edges = new List<GraphEdge>();

        public GraphNode(int weight) {
            this.weight = weight;
        }

        public GraphNode() {
            weight = 0;
        }
    }

    //represents an edge in a graph. Creating the edge updates each node.
    public class GraphEdge {
        public int weight;
        public GraphNode node1, node2;

        public GraphEdge(GraphNode node1, GraphNode node2) {
            if (node1 == null || node2 == null) {
                throw new InvalidOperationException("Cannot add an edge if one or more of the nodes doesnt exist.");
            }

            node1.edges.Add(this);
            node2.edges.Add(this);
            node1.neighbors.Add(node2);
            node2.neighbors.Add(node1);
            this.node1 = node1;
            this.node2 = node2;
            weight = 0;
        }

        public GraphEdge(GraphNode node1, GraphNode node2, int weight) : this(node1, node2) {
            this.weight = weight;
        }
    }

    //class to represent a graph. Includes a list of edges, a list of nodes,
    //a dict to check if nodes are in the graph, and a dict to map two nodes to an edge
    public class Graph {
        protected List<GraphEdge> edges;
        protected List<GraphNode> nodes;
        protected Dictionary<GraphNode, bool> nodeDict;
        protected Dictionary<long, GraphEdge> nodeToEdgeDict;

        public List<GraphEdge> edgeList {
            get {
                return edges;
            }
        }

        public List<GraphNode> nodeList {
            get {
                return nodes;
            }
        }

        public Graph() {
            edges = new List<GraphEdge>();
            nodes = new List<GraphNode>();
            nodeDict = new Dictionary<GraphNode, bool>();
            nodeToEdgeDict = new Dictionary<long, GraphEdge>();
        }

        public Graph(List<GraphNode> nodes, List<GraphEdge> edges) : this() {
            this.edges = edges;
            this.nodes = nodes;
            foreach (GraphNode n in nodes) {
                nodeDict.Add(n, true);
            }
            foreach (GraphEdge e in edges) {
                nodeToEdgeDict.Add(CombineHashes(e.node1, e.node2), e);
            }
        }


        //Basic graph functions:

        //combines hashes of two nodes for purposes of retrieving edge from nodeToEdge dict
        protected long CombineHashes(GraphNode obj1, GraphNode obj2) {
            long code1 = (long)obj2.GetHashCode(), code2 = (long)obj1.GetHashCode(), swap;
            if (code1 > code2) {
                swap = code1;
                code1 = code2;
                code2 = swap;
            }
            long hash = 17;
            hash = hash * 31 + code1;
            hash = hash * 31 + code2;
            return hash;
        }

        //Creates an edge in the graph between the two given nodes, if they exist in the graph
        public virtual void CreateEdge(GraphNode node1, GraphNode node2) {
            if (!nodes.Contains(node1) || !nodes.Contains(node2)) {
                throw new InvalidOperationException("Cannot add an edge if one or more of the nodes arent in the graph.");
            }

            GraphEdge newEdge = new GraphEdge(node1, node2);
            edges.Add(newEdge);

            try {
                nodeToEdgeDict.Add(CombineHashes(node1, node2), newEdge);
            }
            catch(ArgumentException)  {
                Debug.Log("hash collision: " + node1.GetHashCode().ToString() + " " + node2.GetHashCode().ToString() 
                    + " " + CombineHashes(node1, node2).ToString());
                Debug.Log(nodeToEdgeDict[CombineHashes(node1, node2)].ToString());
                return;
            }

        }

        //add a node to the graph
        public virtual void AddNode(GraphNode node) {
            if (node == null) {
                throw new InvalidOperationException("Trying to add null node to the graph.");
            }

            nodes.Add(node);
            nodeDict.Add(node, true);
        }

        //remove an edge from the graph. requires updating the nodes involved with the edge.
        public virtual void RemoveEdge(GraphEdge e) {
            e.node1.edges.Remove(e);
            e.node2.edges.Remove(e);
            e.node1.neighbors.Remove(e.node2);
            e.node2.neighbors.Remove(e.node1);
            edges.Remove(e);
            nodeToEdgeDict.Remove(CombineHashes(e.node1, e.node2));
        }

        //get an edge based on the two nodes in the edge
        public virtual GraphEdge GetEdge(GraphNode n1, GraphNode n2) { 
            try {
                return nodeToEdgeDict[CombineHashes(n1, n2)];
            }
            catch {
                return null;
            }
        }

        //remove an edge based on the two nodes in the edge (overload of above)
        public virtual void RemoveEdge(GraphNode n1, GraphNode n2) {
            GraphEdge edge = GetEdge(n1, n2);

            if (edge == null) {
                return;
            }

            n1.edges.Remove(edge);
            n2.edges.Remove(edge);
            n1.neighbors.Remove(n2);
            n2.neighbors.Remove(n1);
            edges.Remove(edge);
            nodeToEdgeDict.Remove(CombineHashes(n1, n2));
        }

        public void RemoveEdgesByCondition(Func<GraphEdge, bool> edgeRemoveCondition) {
            foreach (var e in edgeList) {
                if (edgeRemoveCondition(e)) {
                    RemoveEdge(e);
                }
            }
        }

        public virtual void RemoveNode(GraphNode n) {
            nodes.Remove(n);
            nodeDict.Remove(n);
        }



        // More complex graph functions below:

        //debugging function for printing a HashSet
        private void printSet(HashSet<GraphNode> nodeset) {
            Debug.Log(string.Join(" ", nodeset.Select(n => n.GetHashCode().ToString()).ToArray()));
        }

        //return a randomized edge list
        private IEnumerable<GraphEdge> RandomizedEdgeList {
            get {
                var rng = new System.Random();
                return edgeList.OrderBy(e => rng.Next());
            }
        }

        //return a copy of the graph edge list
        private IEnumerable<GraphEdge> EdgeListCopy {
            get {
                GraphEdge[] edgeCpy = new GraphEdge[edgeList.Count];
                edgeList.CopyTo(edgeCpy);
                return edgeCpy;
            }
        }

        //run randomized kruskal's algorithm on the graph by keeping track of connected sets for
        //each node at each step. Remove all edges that arent in the final tree.
        public void RandomizedKruskals() {
            Dictionary<GraphEdge, bool> edgesToSave = new Dictionary<GraphEdge, bool>();
            Dictionary<GraphNode, HashSet<GraphNode>> nodeSets = new Dictionary<GraphNode, HashSet<GraphNode>>();
            foreach (var n in nodeList) {
                HashSet<GraphNode> nodeSet = new HashSet<GraphNode>(new GraphNode[] { n });
                nodeSets.Add(n, nodeSet);
            }

            foreach (var e in RandomizedEdgeList) {
                if (!nodeSets[e.node1].Contains(e.node2) && !nodeSets[e.node2].Contains(e.node1)) {
                    edgesToSave.Add(e, true);
                    nodeSets[e.node1] = new HashSet<GraphNode>(nodeSets[e.node1].Union(nodeSets[e.node2]));
                    foreach (var n in nodeSets[e.node1]) {
                        nodeSets[n] = nodeSets[e.node1];
                    }
                }
            }

            Func<GraphEdge, bool> edgeRemoveCondition = (e => !edgesToSave.ContainsKey(e));
            RemoveEdgesByCondition(edgeRemoveCondition);
        }

        private void AddRandConnectedPartition(Dictionary<GraphNode, int> partitionAssignment, int partitionNum,
                                               int partitionSize, List<GraphNode> activeNodes) {
            List<GraphNode> partition = new List<GraphNode>();
            var rng = new System.Random();
            var curNode = activeNodes[rng.Next(activeNodes.Count)];
            partitionAssignment[curNode] = partitionNum;
            partition.Add(curNode);

            for (int j = 0; j < partitionSize; j++) {
                GraphNode nextNode = null;

                nextNode = curNode.neighbors[rng.Next(curNode.neighbors.Count)];
                if (partitionAssignment[nextNode] != -1) {
                    nextNode = null;
                }

                activeNodes.Remove(curNode);

                if (nextNode == null) {
                    curNode = partition[rng.Next(partition.Count)];
                }
                else {
                    partitionAssignment[nextNode] = partitionNum;
                    partition.Add(nextNode);
                    curNode = nextNode;
                }
            }
        }

        public void RandomConnectedPartition(int partitionCount, int partitionSize) {
            Dictionary<GraphNode, int> partitionAssignment = new Dictionary<GraphNode, int>();
            List<GraphNode> activeNodes = new List<GraphNode>();
            var rng = new System.Random();

            foreach (var n in nodeList) {
                activeNodes.Add(n);
                partitionAssignment.Add(n, -1);
            }

            for (int partitionNum = 0; partitionNum < partitionCount; partitionNum++) {
                AddRandConnectedPartition(partitionAssignment, partitionNum, partitionSize, activeNodes);
            }

            Func<GraphEdge, bool> edgeRemoveCondition = (e => (partitionAssignment[e.node1] != partitionAssignment[e.node2]));
            RemoveEdgesByCondition(edgeRemoveCondition);
        }
    }

    //class to represent a square grid graph (https://en.wikipedia.org/wiki/Lattice_graph#Square_grid_graph)
    public class GridGraph : Graph {
        int sizeX, sizeY;
        public int x {
            get { return sizeX; }
        }
        public int y {
            get { return sizeY; }
        }

        //the nodes are stored in a public 2D grid
        public GraphNode[,] grid;

        public GridGraph(int x, int y) : base() {
            sizeX = x;
            sizeY = y;
            InitGridGraph();
        }

        private void InitNodes() {
            for (int i = 0; i < sizeX; i++) {
                for (int j = 0; j < sizeY; j++) {
                    grid[i, j] = new GraphNode();
                    AddNode(grid[i, j]);
                }
            }
        }

        private void InitEdges() {
            for (int i = 0; i < sizeX; i++) {
                for (int j = 0; j < sizeY; j++) {
                    if (i + 1 < sizeX) {
                        CreateEdge(grid[i, j], grid[i + 1, j]);
                    }
                    if (j + 1 < sizeY) {
                        CreateEdge(grid[i, j], grid[i, j + 1]);
                    }
                }
            }
        }

        //initialize the nodes and edges of the grid graph
        private void InitGridGraph() {
            grid = new GraphNode[sizeX, sizeY];
            InitNodes();
            InitEdges();            
        }

        //print the grid
        public void PrintGrid() {
            for (int i = 0; i < sizeX; i++) {
                for (int j = 0; j < sizeY; j++) {
                    Console.Write(grid[i, j].weight);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }

    public class TreeGraph : Graph {
        public override void CreateEdge(GraphNode node1, GraphNode node2) {
            if (!nodes.Contains(node1) || !nodes.Contains(node2)) {
                throw new InvalidOperationException("Cannot add an edge if one or more of the nodes arent in the graph.");
            }

            if (node1.edges.Count > 0 && node2.edges.Count > 0) {
                throw new InvalidOperationException("Can't create an edge between nodes that are already connected to tree.");
            }

            GraphEdge newEdge = new GraphEdge(node1, node2);
            edges.Add(newEdge);
            nodeToEdgeDict[CombineHashes(node1, node2)]  = newEdge;
        }


    }

    public static class Test {
        public static void Main() {
            GridGraph testGraph = new GridGraph(5, 10);
            testGraph.PrintGrid();
            testGraph.GetEdge(testGraph.grid[0,0], testGraph.grid[0,1]);
        }
    }
}
