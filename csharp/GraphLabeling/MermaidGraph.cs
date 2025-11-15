namespace GraphLabeling;

using System;
using System.Collections.Generic;
using System.Text;

public static class MermaidGraph
{
    /// <summary>
    /// Builds a Mermaid diagram (graph TD) from an adjacency‑list.
    /// Nodes are labelled with the integer array <paramref name="vertexLabels"/>.
    /// Each undirected edge is annotated with the sum of the two vertex indices it connects.
    /// </summary>
    /// <param name="adjacencyList">
    /// adjacencyList[i] contains an array of vertex indices adjacent to vertex i.
    /// The list may contain each edge twice (because the graph is undirected).
    /// </param>
    /// <param name="vertexLabels">
    /// Integer labels for each vertex. vertexLabels[i] will be shown for node i.
    /// </param>
    /// <returns>A string that can be pasted into a Mermaid live editor or Markdown.</returns>
    public static string GenerateMermaidGraph(int[][] adjacencyList, int[] vertexLabels)
    {
        if (adjacencyList == null) throw new ArgumentNullException(nameof(adjacencyList));
        if (vertexLabels   == null) throw new ArgumentNullException(nameof(vertexLabels));
        if (adjacencyList.Length != vertexLabels.Length)
            throw new ArgumentException("Adjacency list and label array must contain the same number of vertices.");

        var sb = new StringBuilder();

        // 1. Header – top‑down layout (feel free to change to graph LR, etc.)
        sb.AppendLine("---");
        sb.AppendLine("config:");
        sb.AppendLine("  layout: fixed");
        sb.AppendLine("---");
        sb.AppendLine("flowchart LR");

        // 2. Define the nodes with their integer labels.
        for (int i = 0; i < adjacencyList.Length; i++)
        {
            // Node id: V0, V1, …   (any unique string works)
            sb.AppendLine($"    V{i}[[{vertexLabels[i]}]]");   // double brackets for a “rounded rectangle” style
        }

        // 3. Add the undirected edges – only once per edge.
        var seenEdges = new HashSet<(int, int)>();
        var seenEdgeWeights = new HashSet<int>();

        for (int u = 0; u < adjacencyList.Length; u++)
        {
            if (adjacencyList[u] == null) continue;

            foreach (int v in adjacencyList[u])
            {
                // sanity checks
                if (v < 0 || v >= adjacencyList.Length || u == v) continue;

                // canonical order ensures we never output the same edge twice
                var key = u < v ? (u, v) : (v, u);
                if (!seenEdges.Add(key)) continue;   // already processed

                int edgeSum = vertexLabels[u] + vertexLabels[v];
                if (!seenEdgeWeights.Add(edgeSum))
                    throw new ArgumentException();
                sb.AppendLine($"    V{u}<-->|{edgeSum}|V{v}");
            }
        }

        return sb.ToString();
    }
    
    public static string GenerateMermaidGraph(int[][] adjacencyList)
    {
        if (adjacencyList == null) throw new ArgumentNullException(nameof(adjacencyList));

        var sb = new StringBuilder();

        // 1. Header – top‑down layout (feel free to change to graph LR, etc.)
        sb.AppendLine("---");
        sb.AppendLine("config:");
        sb.AppendLine("  layout: fixed");
        sb.AppendLine("---");
        sb.AppendLine("flowchart LR");

        // 2. Define the nodes with their integer labels.
        for (int i = 0; i < adjacencyList.Length; i++)
        {
            // Node id: V0, V1, …   (any unique string works)
            sb.AppendLine($"    V{i}[[{i}]]");   // double brackets for a “rounded rectangle” style
        }

        // 3. Add the undirected edges – only once per edge.
        var seenEdges = new HashSet<(int, int)>();

        for (int u = 0; u < adjacencyList.Length; u++)
        {
            if (adjacencyList[u] == null) continue;

            foreach (int v in adjacencyList[u])
            {
                // sanity checks
                if (v < 0 || v >= adjacencyList.Length || u == v) continue;

                // canonical order ensures we never output the same edge twice
                var key = u < v ? (u, v) : (v, u);
                if (!seenEdges.Add(key)) continue;   // already processed

                sb.AppendLine($"    V{u}<-->V{v}");
            }
        }

        return sb.ToString();
    }
}