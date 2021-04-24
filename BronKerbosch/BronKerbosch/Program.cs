using Graphviz4Net.Dot;
using Graphviz4Net.Dot.AntlrParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BronKerbosch
{
    class Program
    {


        /// <summary>
        /// Implementacja algorytmu Bron-Kerbosch bez pivota. Przy pierwszym uruchomieniu R i X muszą być puste, a P zawierać wszystkie
        /// wierzchołki znajdujące się w grafie
        /// </summary>
        /// <param name="R">Zbiór wierchołków będących częsciowym wynikiem znajdowania kliki</param>
        /// <param name="P">Zbiór wierzchołków, które są kandydatami do rozważenia</param>
        /// <param name="X">Zbiór wierzchołków pominietych</param>
        /// <param name="adjacencyMatrix">Macierz sąsiedztwa grafu</param>
        /// <param name="n">Ilość wierzchołków</param>
        static void BronKerboschWithoutPivoting(List<int> R, List<int> P, List<int> X, int[,] adjacencyMatrix, int n)
        {
            if (P.Count == 0 && X.Count == 0)
            {
                Console.Write($"Klika: ");
                foreach (var item in R)
                {
                    Console.Write($"{item} ");
                }

                Console.WriteLine();
            }

            foreach (var v in P.ToList())
            {
                // Przygotowanie R do wysłania rekurencyjnego
                List<int> SendR = new List<int>();
                SendR.AddRange(R);
                SendR.Add(v);

                // Przygotowanie P do wysłanie rekurencyjnego
                List<int> SendP = new List<int>();
                List<int> neighborsV = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    if (adjacencyMatrix[v, i] == 1)
                    {
                        neighborsV.Add(i);
                    }
                }
                SendP = P.Intersect(neighborsV).ToList();

                // Przygotowanie X do wysłania
                List<int> SendX = new List<int>();
                SendX = X.Intersect(neighborsV).ToList();

                // Wywołanie rekurencyjne
                BronKerboschWithoutPivoting(SendR, SendP, SendX, adjacencyMatrix, n);

                P.Remove(v);
                X.Add(v);

            }
        }

        /// <summary>
        /// Implementacja algorytmu Bron-Kerbosch z pivotem. Przy pierwszym uruchomieniu R i X muszą być puste, a P zawierać wszystkie
        /// wierzchołki znajdujące się w grafie. Pivot to wirrzchołek z największą iloscią sąsiadow.
        /// </summary>
        /// <param name="R">Zbiór wierchołków będących częsciowym wynikiem znajdowania kliki</param>
        /// <param name="P">Zbiór wierzchołków, które są kandydatami do rozważenia</param>
        /// <param name="X">Zbiór wierzchołków pominietych</param>
        /// <param name="adjacencyMatrix">Macierz sąsiedztwa grafu</param>
        /// <param name="n">Ilość wierzchołków</param>
        static void BronKerboschWithPivoting(List<int> R, List<int> P, List<int> X, int[,] adjacencyMatrix, int n)
        {
            // if P and X are both empty:
            if (P.Count == 0 && X.Count == 0)
            {
                // report R as a maximal clique
                Console.Write($"Klika : ");
                foreach (var item in R.OrderBy(x => x))
                {
                    Console.Write($"{item} ");
                }
                Console.WriteLine();
            }

            // choose a pivot vertex u in PuX
            int maxnNighborsCount = 0;
            int verU = 0;
            List<int> PuX = new List<int>();
            PuX.AddRange(P);
            PuX.AddRange(X);
            foreach (var item in PuX.ToList())
            {
                int currentNeighborsCount = 0;
                for (int i = 0; i < n; i++)
                {
                    if (adjacencyMatrix[item, i] == 1)
                    {
                        currentNeighborsCount++;
                    }
                }

                if (currentNeighborsCount > maxnNighborsCount)
                {
                    maxnNighborsCount = currentNeighborsCount;
                    verU = item;
                }
            }

            List<int> PWithoutNU = new List<int>();
            PWithoutNU.AddRange(P);
            for (int i = 0; i < n; i++)
            {
                if (adjacencyMatrix[verU, i] == 1)
                {
                    PWithoutNU.Remove(i);
                }
            }

            foreach (var v in PWithoutNU)
            {
                List<int> SendR = new List<int>();
                SendR.AddRange(R);
                SendR.Add(v);

                List<int> neighborsV = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    if (adjacencyMatrix[v, i] == 1)
                    {
                        neighborsV.Add(i);
                    }
                }

                List<int> SendP = new List<int>();
                SendP = P.Intersect(neighborsV).ToList();


                List<int> SendX = new List<int>();
                SendX = X.Intersect(neighborsV).ToList();


                BronKerboschWithPivoting(SendR, SendP, SendX, adjacencyMatrix, n);


                P.Remove(v);
                X.Add(v);
            }

        }


        static void Main(string[] args)
        {
            // Przygotowanie stoperów
            Stopwatch parseTimer = new Stopwatch();
            Stopwatch BKTimer = new Stopwatch();

            parseTimer.Start();
            // Przyjęcie pliku DOT
            string path = args[0].ToString();
            string file = File.ReadAllText(path);
            DotGraph<int> graphRaw = new DotGraph<int>();
            const int stack = 1024 * 1024 * 64;
            Thread thread = new Thread(
                    () =>
                    {
                        var parser = AntlrParserAdapter<int>.GetParser();
                        graphRaw = parser.Parse(file);
                    },
                    stack
                );
            thread.Start();
            thread.Join();


            // Utworzenie macierzy sąsiedztwa
            int[,] adjacencyMatrix = new int[graphRaw.Vertices.Count(), graphRaw.Vertices.Count()];
            foreach (var item in graphRaw.VerticesEdges)
            {
                int x1 = (int)item.Source.Id;
                int x2 = (int)item.Destination.Id;
                adjacencyMatrix[x1, x2] = 1;
                adjacencyMatrix[x2, x1] = 1;
            }


            // Zapisanie ilości wierzchołków
            int n = adjacencyMatrix.GetLength(0);
            parseTimer.Stop();


            // Utowrzenie wypełninej listy potencjalnych wierzchołków oraz pustych list X i R
            List<int> P = new List<int>();
            for (int i = 0; i < n; i++)
            {
                P.Add(i);
            }
            List<int> X = new List<int>();
            List<int> R = new List<int>();


            // Uruchomienie algorytmu bez pivota, dzięki temu nie wymaga sortowania wyniku
            BKTimer.Start();
            Console.WriteLine("--BronKerboschWithoutPivot--");
            BronKerboschWithoutPivoting(R, P, X, adjacencyMatrix, n);
            BKTimer.Stop();


            // Wypisanie diagnostyki
            Console.WriteLine($"Czas działania:\n\tParser:\t\t{parseTimer.Elapsed}\n\tBronKerbosch:\t{BKTimer.Elapsed}");

        }
    }
}
