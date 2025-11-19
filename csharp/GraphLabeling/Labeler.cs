using System.Collections;

namespace GraphLabeling;

public class GraphLabeler
{
    readonly int[][] adjList;
    readonly int[] labels;

    static int seed = 0;
    Random random = new (seed++);

    public HashSet<int> edgeSet { get; private set; } = [];
    
    public GraphLabeler(int[][] adjList)
    {
        this.adjList = adjList;
        labels = new int[adjList.Length];
    }

    public int[] SolveLabels()
    {
        // TODO: Potential optimization:
        //       Prioritize first by lowest label *then* by largest number of labeled adjacent nodes. 
        //       In C# use OrderedDictionary of labeled frontier nodes keyed by label, then SortedSet?
        var frontier = new PriorityQueue<int, int> ();
        labels[0] = 1; // Set first vertex label to be 1
        for (int i = 0; i < adjList[0].Length; i++)
            frontier.Enqueue(adjList[0][i], 1); // Add starting vertexes

        var skippedWeights = new List<int>();
        int nextLargestWeight = 2;


        while (frontier.Count > 0)
        {
            // Get vertex to be labeled
            var currentVertexId = frontier.Dequeue();
            /*_ = frontier.TryPeek(out _, out int minPriority);
            List<int> nextVertexIds = [];
            while (frontier.TryPeek(out int _, out int priority) && priority == minPriority)
                nextVertexIds.Add(frontier.Dequeue());
            int nextVertexIdIndex = random.Next(0, nextVertexIds.Count);
            var currentVertexId = nextVertexIds[nextVertexIdIndex];
            nextVertexIds.RemoveAt(nextVertexIdIndex);
            foreach (var idPriority in nextVertexIds)
                frontier.Enqueue(idPriority, minPriority);*/
            
            
            // Don't label already labeled nodes
            if (labels[currentVertexId] != 0)
                continue;
            
            // Get adjacent vertexes
            var adjVertexes = adjList[currentVertexId];
            
            // Labels will be stored in accenting order
            var adjLabels = new List<int>(adjVertexes.Length);

            var unlabeledNeighbors = new List<int>();
            
            // Get the labels of adjacent vertexes if set AKA non-zero
            for (int i = 0; i < adjVertexes.Length; i++)
            {
                var label = labels[adjVertexes[i]];
                if (label > 0)
                    adjLabels.Add(label);
                else
                    unlabeledNeighbors.Add(adjVertexes[i]);
                    
            }
            adjLabels.Sort(); // Sort smallest to largest for efficiency
            
            // Assert there is at least one labeled adjacent vertex
            if (adjLabels.Count == 0)
                throw new ApplicationException("Labeling vertex without labeled neighbors!");

            // Find a label for the vertex //

            int? newLabel = null;

            // Try skipped weights
            for (var weightIndex = 0; weightIndex < skippedWeights.Count; weightIndex++)
            {
                int nextEdge = skippedWeights[weightIndex];
                
                int potentialLabel = nextEdge - adjLabels[0]; 
                
                if (CheckLabel(potentialLabel, currentVertexId, adjLabels))
                { // Label is valid
                    newLabel = potentialLabel;
                    skippedWeights.RemoveAt(weightIndex);
                    break;
                }
            }
            
            // Try max weight
            while (newLabel is null)
            {
                // Note: labelsOfAdjNodes[0] *should* be the smallest for efficiency
                int potentialLabel = nextLargestWeight - adjLabels[0];

                if (CheckLabel(potentialLabel, currentVertexId, adjLabels))
                {
                    newLabel = potentialLabel;
                }
                else
                { // Put skipped values in skippedWeights in ascending order
                    skippedWeights.Add(nextLargestWeight);
                }
                
                nextLargestWeight++;
            }

            
            // Assert new label must not be null
            if (newLabel is null || newLabel == 0)
                throw new ApplicationException("No valid label found!");

            // Set new label
            labels[currentVertexId] = (int)newLabel;

            // Add adjNodes that have not been labeled
            foreach (int unlabeledNeighbor in unlabeledNeighbors)
            {
                frontier.Enqueue(unlabeledNeighbor, (int)newLabel);
            }
        }
        
        // Assert All nodes should be labeled when the queue is empty.
        if (labels.Contains(0))
            // throw new ApplicationException("Some vertexes were not labeled!");
            Console.Error.WriteLine("Some vertexes were not labeled!");
        
        return labels;
    }

    bool CheckLabel(int label, int currentVertexIndex, in List<int> adjLabels)
    {
        List<int> newEdges = new(adjLabels.Count);
        
        foreach (var adjLabel in adjLabels)
        {
            int newEdge = label + adjLabel;
            if (edgeSet.Contains(newEdge) || newEdges.Contains(newEdge))
                return false;
            newEdges.Add(newEdge);
        }
        
        // Block if this label is used two degrees away
        if(CheckTwoDegreeDuplicate(label, currentVertexIndex))
            return false;

        foreach (var newEdge in newEdges)
        {
            if(!edgeSet.Add(newEdge))
                throw new ApplicationException($"Edge {newEdge} already exists!");
        }
        
        return true;
    }

    // returns True if a duplicate exists; False if no duplicate was found.
    bool CheckTwoDegreeDuplicate(int potentialLabel, int vertexIndex)
    {
        foreach (int firstNeighborIndex in adjList[vertexIndex])
            foreach (int secondNeighborIndex in adjList[firstNeighborIndex])
                if (potentialLabel == labels[secondNeighborIndex])
                    return true;
        
        return false;
    }
    
}

public static class GraphData
{
    public static readonly int[][] P2Graph2AdjList = [
        [1, 4, 8], // 0 <- This vertex is shared with the previous graph in the chain
        [0, 2, 9], // 1
        [1, 3, 5], // 2 <- This vertex is shared with the next graph in the chain
        [2, 4, 6], // 3
        [0, 3, 7], // 4
        [2, 7, 8], // 5
        [3, 8, 9], // 6
        [4, 5, 9], // 7
        [0, 5, 6], // 8
        [1, 6, 7], // 9
    ];


}

public static class GraphBuilder
{
    
    public static int[][] ExtendGraph(int[][] graphAdjList, int oldConnectionPoint, int newConnectionPoint, int chainLength)
    {
        List<List<int>> adjList = new(graphAdjList.Length);
        adjList.AddRange(graphAdjList.Select(t => new List<int>(t)));

        for (int i = 0; i < chainLength - 1; i++)
        {
            int oldSharedPoint = oldConnectionPoint + 9*i;
            int newSharedPoint = newConnectionPoint + 9*i;

            for (int baseVertexIndex = 0; baseVertexIndex < graphAdjList.Length; baseVertexIndex++)
            {
                var newVertexIndex = baseVertexIndex+ 9 * (i + 1);

                foreach (int neighborIndex in graphAdjList[baseVertexIndex])
                {
                    if (baseVertexIndex == newConnectionPoint)
                    {
                        var newNeighborIndex = neighborIndex + 9 * (i + 1);
                        adjList[oldSharedPoint].Add(newNeighborIndex);
                    }
                    else
                    {
                        // Instantiate if it does not exist.
                        if(adjList.Count <= newVertexIndex)
                            adjList.Add(new List<int>(3));
                        
                        if (neighborIndex == newConnectionPoint)
                        {
                            adjList[newVertexIndex].Add(oldSharedPoint);
                        }
                        else
                        {
                            var newNeighborIndex = neighborIndex + 9 * (i + 1);
                            adjList[newVertexIndex].Add(newNeighborIndex);
                        }
                    }
                }
            }
        }

        return adjList.Select(innerArray => innerArray.ToArray()).ToArray();
    }
    
    /// <summary>
    /// Builds a chain of graphs that are clones of the supplied graph.
    /// The clone that is attached to the previous one merges its vertex
    /// at index idxNew with the vertex at index idxOld of the previous
    /// clone.
    /// </summary>
    /// <param name="originalAdj">Adjacency list of the original graph.</param>
    /// <param name="idxOld">Vertex index in the previous clone that will be shared.</param>
    /// <param name="idxNew">Vertex index in the new clone that will be shared.</param>
    /// <param name="repeat">Number of additional clones to add (0 = no extra clones).</param>
    /// <returns>Adjacency list of the resulting chain.</returns>
    public static int[][] BuildChain(int[][] originalAdj, int idxOld, int idxNew, int repeat)
    {
        if (originalAdj == null)
            throw new ArgumentNullException(nameof(originalAdj));

        if (idxOld < 0 || idxOld >= originalAdj.Length)
            throw new ArgumentOutOfRangeException(nameof(idxOld));

        if (idxNew < 0 || idxNew >= originalAdj.Length)
            throw new ArgumentOutOfRangeException(nameof(idxNew));

        if (repeat < 0)
            throw new ArgumentOutOfRangeException(nameof(repeat));

        int n = originalAdj.Length;                 // vertices in one copy
        int totalClones = repeat + 1;               // how many copies we will end up with
        int totalVertices = n * totalClones - repeat; // we merge one vertex for every link

        // Work with a mutable list of vertices
        var vertices = new List<int[]>();

        // Helper: clone the original adjacency list (deep copy)
        int[][] CloneGraph(int[][] graph)
        {
            var copy = new int[graph.Length][];
            for (int i = 0; i < graph.Length; i++)
                copy[i] = (int[])graph[i].Clone();
            return copy;
        }

        // 1. Add the first clone (the original graph)
        vertices.AddRange(CloneGraph(originalAdj));

        // 2. For each subsequent clone, merge the two chosen vertices
        int offset = n; // first free index in the growing graph

        for (int step = 0; step < repeat; step++)
        {
            // Clone the original graph again
            var clone = CloneGraph(originalAdj);

            // The indices of the vertices that will be merged
            int mergeOld = idxOld;            // in the current growing graph
            int mergeNew = idxNew + offset;   // in the fresh clone (after shifting)

            // ---- Build merged adjacency for the shared vertex ----
            var mergedAdj = new HashSet<int>();

            // old adjacency
            foreach (int v in vertices[mergeOld])
                if (v != mergeNew) mergedAdj.Add(v);

            // new adjacency (shifted)
            foreach (int v in clone[idxNew])
            {
                int shifted = v + offset;
                if (shifted != mergeOld) mergedAdj.Add(shifted);
            }

            vertices[mergeOld] = mergedAdj.OrderBy(x => x).ToArray();

            // ---- Replace references to the new vertex with the old one ----
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices[i].Length; j++)
                    if (vertices[i][j] == mergeNew)
                        vertices[i][j] = mergeOld;
            }

            // ---- Add the remaining vertices of the new clone ----
            for (int i = 0; i < n; i++)
            {
                if (i == idxNew) continue;   // already merged

                // shift adjacency indices
                var shiftedAdj = clone[i]
                    .Select(v => v + offset)
                    .ToArray();

                vertices.Add(shiftedAdj);
            }

            // Update the offset for the next clone
            offset += n;
        }

        // Convert back to an array for the caller
        return vertices.ToArray();
    }
}