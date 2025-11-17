using System;
using System.Collections.Generic;
using System.Linq;

public class Node
{
    public double x;
    public double y;
    
    public Node(double a, double b)
    {
        x = a;
        y = b;
    }
}

public class Edge
{
    public string To ;    // Node đích
    public double Cost ;    // Chi phí truyền (delay, weight...)
    
    public Edge(string to, double cost)
    {
        To = to;
        Cost = cost;
    }
}

public class Graph
{
    public Dictionary<string, List<Edge>> AdjList = new(); // Adjacency list

    public void AddNode(string name)
    {
        if (!AdjList.ContainsKey(name))
            AdjList[name] = new List<Edge>();
        else
        {
            Console.WriteLine($"chọn một tên node khác");
        }
    }

    public void AddEdge(string from, string to, double cost, bool bidirectional = true)
    {
        AddNode(from);
        AddNode(to);
        AdjList[from].Add(new Edge(to, cost));

        if (bidirectional)
            AdjList[to].Add(new Edge(from, cost));
    }
}

public static class Heuristic
{
    // dùng Heuristic Euclidean
    public static double Euclid(string a, string b, Dictionary<string, Node> pos)
    {
        // Truy cập Dictionary lấy tọa độ node
        var pa = pos[a];
        var pb = pos[b];

        double difx = pa.x - pb.x;
        double dify = pa.y - pb.y;

        return Math.Sqrt(difx * difx + dify * dify);
    }
}

public static class AStarNetwork
{
    public static (List<string>? Path, double TotalCost) FindPath( // Trả về cả đường đi và tổng chi phí
        Graph graph,
        string start,
        string goal,
        Func<string, string, double> heuristic)
    {
        // PriorityQueue (min-heap) cho các node cần thăm dò, ưu tiên node có f nhỏ nhất
        var OpenList = new PriorityQueue<string, double>();
        var cameFrom = new Dictionary<string, string>();
        var gScore = new Dictionary<string, double>();

        foreach (var node in graph.AdjList.Keys)
            gScore[node] = double.PositiveInfinity;

        gScore[start] = 0;
        OpenList.Enqueue(start, heuristic(start, goal));

        var closed = new HashSet<string>();
        double finalCost = -1; // Biến lưu tổng chi phí

        while (OpenList.Count > 0)
        {
            OpenList.TryDequeue(out var current, out _);

            if (current == goal)
            {
                finalCost = gScore[current]; // Lưu chi phí tìm được
                return (Reconstruct(cameFrom, current), finalCost);
            }

            if (closed.Contains(current))
                continue;
                
            closed.Add(current);

            foreach (var edge in graph.AdjList[current])
            {
                var neighbor = edge.To;
                // gScore[current] là chi phí thực tế từ start đến current
                // edge.Cost là chi phí thực tế từ current đến neighbor
                double tentative = gScore[current] + edge.Cost;

                if (tentative < gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative;
                    // f = g + h
                    double f = tentative + heuristic(neighbor, goal);
                    OpenList.Enqueue(neighbor, f);
                }
            }
        }
        return (null, finalCost); // Không tìm thấy đường đi
    }

    private static List<string> Reconstruct(Dictionary<string, string> cameFrom, string current)
    {
        var path = new List<string> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // 1. Tọa độ của các node (dùng cho Heuristic)
        var pos = new Dictionary<string, Node>
        {
            { "A", new Node(0, 0) }, // Start
            { "B", new Node(2, 3) },
            { "C", new Node(4, 1) },
            { "D", new Node(6, 4) },
            { "E", new Node(8, 2) },
            { "F", new Node(10, 0) } // Goal
        };

        // 2. Tạo đồ thị kết nối mạng và các cạnh 
        var g = new Graph();
        // Thêm các cạnh (Node 1, Node 2, trọng số)
        g.AddEdge("A", "B", 4); 
        g.AddEdge("A", "C", 2);
        g.AddEdge("B", "D", 5);
        g.AddEdge("C", "D", 7);
        g.AddEdge("C", "E", 3);
        g.AddEdge("D", "E", 2);
        g.AddEdge("E", "F", 1); 

        // 3. Thực hiện tìm đường A*
        string startNode = "A";
        string goalNode = "F";

        Console.WriteLine($" Bắt đầu tìm kiếm đường truyền tối ưu từ Node {startNode} đến Node {goalNode}");
        Console.WriteLine("--------------------------------------------------");
        
        var (path, totalCost) = AStarNetwork.FindPath(g, startNode, goalNode,
            // Định nghĩa hàm heuristic tại chỗ, truyền tọa độ vào cho Heuristic.Euclid
            (a, b) => Heuristic.Euclid(a, b, pos));

        // 4. Minh họa kết quả
        if (path == null)
        {
            Console.WriteLine("Không tìm được đường truyền giữa hai node này.");
        }
        else
        {
            Console.WriteLine("Đường truyền tối ưu (Optimal Path):");
            
            // In đường đi
            Console.WriteLine(string.Join(" -> ", path));

            // In tổng chi phí
            Console.WriteLine($"\nTổng chi phí truyền (Cost/Delay): {totalCost}");
            
            // Minh họa chi tiết các bước chuyển
            Console.WriteLine("\nChi tiết từng bước chuyển:");
            for (int i = 0; i < path.Count - 1; i++)
            {
                string current = path[i];
                string next = path[i + 1];
                
                // Tìm Cost của cạnh (dùng List<Edge> của Node hiện tại)
                double cost = g.AdjList[current]
                               .First(edge => edge.To == next)
                               .Cost;
                               
                Console.WriteLine($"   - {current} -> {next}: Chi phí = {cost}");
            }
        }
    }

}
