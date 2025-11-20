using System.Diagnostics;

namespace GraphLabeling;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, GraphLabeling!");

        _ = RunOnce(300, false);
        // RunRange(1, 3);

    }

    static int[] RunOnce(int chainLength, bool printGraph = false)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var graph = GraphBuilder.ExtendGraph(GraphData.P2Graph2AdjList, 2, 0, chainLength);

        var labeler = new GraphLabeler(graph);

        var labels = labeler.SolveLabels();
        
        Console.WriteLine($"Min-k: {Math.Ceiling(((double) chainLength * 15 + 1)/2)}, Max-Label: {labels.Max()}");

        sw.Stop();

        if (printGraph)
        {
            Console.Write("Labels: ");
            foreach (var label in labels)
                Console.Write(label + ",");
            Console.WriteLine();

            Console.Write("Edges: ");
            foreach (var edge in labeler.edgeSet)
                Console.Write(edge + ",");
            Console.WriteLine();


            var mermaid = MermaidGraph.GenerateMermaidGraph(graph, labels);
            // var mermaid = MermaidGraph.GenerateMermaidGraph(graph);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(mermaid);

            Console.WriteLine();
            Console.WriteLine(sw.Elapsed);
        }

        HashSet<int> edgeSet = new HashSet<int>();
        foreach (int edge in labeler.edgeSet)
            if(!edgeSet.Add(edge))
                Console.Write("Duplicate edge found #" + edge);

        return labels;
    }

    static void RunRange(int minChain, int maxChain)
    {
        for (int i = minChain; i <= maxChain; i++)
        {
            var graph = GraphBuilder.ExtendGraph(GraphData.P2Graph2AdjList, 2, 0, i);

            var labeler = new GraphLabeler(graph);

            var labels = labeler.SolveLabels();
            
            Console.WriteLine($"Min-k: {Math.Ceiling(((double) i * 15 + 1)/2)}, Max-Label: {labels.Max()}");
        }
    }

    static void RunFindOptimal()
    {
        Random rnd = new Random();

        int[] labels = [1, 1, 2, 4, 5, 7, 2, 8, 8, 7];

        for (int i = 0; i < 999999; i++)
        {
            try
            {
                var mermaid = MermaidGraph.GenerateMermaidGraph(GraphData.P2Graph2AdjList, labels);
            
                Console.WriteLine(mermaid);
                break;
            }
            catch
            {
                // ignored
            }

            rnd.Shuffle(labels);
            // _ = RunOnce(1);
        }
        
        // Result:
        // TODO: Make labeling order fixed based on this?
        /*
---
config:
  layout: fixed
---
flowchart LR
    V0[[1]]
    V1[[4]]
    V2[[7]]
    V3[[5]]
    V4[[8]]
    V5[[7]]
    V6[[2]]
    V7[[8]]
    V8[[1]]
    V9[[2]]
    V0<-->|5|V1
    V0<-->|9|V4
    V0<-->|2|V8
    V1<-->|11|V2
    V1<-->|6|V9
    V2<-->|12|V3
    V2<-->|14|V5
    V3<-->|13|V4
    V3<-->|7|V6
    V4<-->|16|V7
    V5<-->|15|V7
    V5<-->|8|V8
    V6<-->|3|V8
    V6<-->|4|V9
    V7<-->|10|V9
         */
    }
}