//// CHC Algorythm made in C#
// This algorythm uses parent population and child population (which is made from random parents) to create new population using HUX Crossover.
// HUX Crossover takes two parents, selects chromosomeLenght/2 bytes form first and second parent and exchanges them making two children.
// Initial parent array is randomly generated.
// If convergance is detected initial parent array is made based on the best individual from previous population.
//
//// Paweł Krzempek @ 2020

using System;
using System.Diagnostics;
using System.Linq;

namespace CHC_Algorithm
{
    class Program
    {
        /// <summary>
        /// Fills specified Population array with random bites of data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="popSize"></param>
        /// <param name="chromLenght"></param>
        static void randomFill(Population[] array, int chromLenght)
        {
            int popSize = array.Length;
            var rnd = new Random();
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < popSize; i++)
            {
                for (int j = 0; j < chromLenght; j++)
                {
                    sb.Append(rnd.Next(0, 2));
                }
                array[i].Value = sb.ToString();
                sb.Clear();
            }
        }

        /// <summary>
        /// Compares generated population with target string. Puts percent corectness in Corrctnes property in table
        /// </summary>
        /// <param name="array"></param>
        /// <param name="popSize"></param>
        /// <param name="chromLenght"></param>
        /// <param name="target"></param>
        static void CheckCorrectnes(Population[] array, int chromLenght, string target)
        {
            int popSize = array.Length;
            string stringToCheck;
            int amountCorrect;

            for (int i = 0; i < popSize; i++)
            {
                stringToCheck = array[i].Value;
                amountCorrect = 0;

                for (int j = 0; j < chromLenght; j++)
                {
                    if (stringToCheck[j] == target[j])
                    {
                        amountCorrect++;
                    }
                }

                array[i].Correctness = (100 * amountCorrect) / chromLenght;
            }
        }

        /// <summary>
        /// Calculates Hamming distance which is the differance between two strings
        /// </summary>
        /// <param name="person1"></param>
        /// <param name="person2"></param>
        /// <returns></returns>
        static int HammingDistance(string person1, string person2)
        {
            int distance = 0;
            for (int i = 0; i < person1.Length; i++)
            {
                if (person1[i] == person2[i])
                {
                    distance++;
                }
            }
            return distance;
        }

        /// <summary>
        /// Takes two parents and makes two children
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <param name="chromLenght"></param>
        /// <returns>(child1, child2)</returns>
        static (string, string) HUXCrossOver(string parent1, string parent2, int chromLenght)
        {
            var rnd = new Random();
            int generated = int.MaxValue;
            string child1 = string.Empty;
            string child2 = string.Empty;

            // Generates list of numbers which will be used in HUX Crossover
            int[] take = new int[chromLenght / 2];
            for (int i = 0; i < take.Length; i++)
            {
                do
                {
                    generated = rnd.Next(minValue: 0, maxValue: chromLenght);
                } while (!(!take.Contains(generated)) && generated >= 0 && generated < chromLenght);
                take[i] = generated;
            }

            // Generation of children
            for (int i = 0; i < chromLenght; i++)
            {
                if (take.Contains(i))
                {
                    child1 += parent1[i];
                    child2 += parent2[i];
                }
                else
                {
                    child1 += parent2[i];
                    child2 += parent1[i];
                }
            }
            
            return (child1, child2);
        }

        /// <summary>
        /// Generates child population using parent population and HUX CrossOver
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="popSize"></param>
        /// <param name="chromLenght"></param>
        /// <param name="delta"></param>
        /// <param name="child"></param>
        /// <returns>Amount of children generated</returns>
        static int childGeneration(Population[] parent, int chromLenght, int delta, Population[] child)
        {
            int popSize = parent.Length;
            int generatedChildren = 0;
            string parent1;
            string parent2;

            for (int i = 0; i < popSize/2; i++)
            {
                // Parents are devided into two parts, children are made from i-th element from both parts
                parent1 = parent[i].Value;
                parent2 = parent[i + (popSize / 2)].Value;

                if (HammingDistance(parent1, parent2) > delta)
                {
                    var HUXOutput = HUXCrossOver(parent1, parent2, chromLenght);
                    child[i].Value = HUXOutput.Item1;
                    child[i + (popSize / 2)].Value = HUXOutput.Item2;
                    generatedChildren++;
                }
            }

            return generatedChildren*2;
        }

        /// <summary>
        /// Creates new population combining best individuals from parent and children population
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="popSize"></param>
        /// <param name="newPop"></param>
        static void TakeBest(Population[] parent, Population[] child, int popSize, Population[] newPop)
        {
            var parentOrdered = parent.OrderByDescending(x=>x.Correctness).ToArray();
            var childOrdered = child.OrderByDescending(x=>x.Correctness).ToArray();

            int takenParent = 0;
            int takenChild = 0;

            for (int i = 0; i < popSize; i++)
            {
                if (parentOrdered[takenParent].Correctness >= childOrdered[takenChild].Correctness)
                {
                    newPop[i].Value = parentOrdered[takenParent].Value;
                    newPop[i].Correctness = parentOrdered[takenParent].Correctness;
                    takenParent++;
                }
                else
                {
                    newPop[i].Value = childOrdered[takenChild].Value;
                    newPop[i].Correctness = childOrdered[takenChild].Correctness;
                    takenChild++;
                }
            }
        }

        /// <summary>
        /// In case of low diversity, reinitialization takes one best individual from generated individuals, preserves 25% of its bytes and randomly shuffles the rest
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="popSize"></param>
        /// <param name="chromLenght"></param>
        /// <param name="newPop"></param>
        static void Reinitialization(Population[] parent, Population[] child, int popSize, int chromLenght, Population[] newPop)
        {
            var rnd = new Random();
            int generated = int.MaxValue;

            // Selects best individual
            Population[] orderedParents = parent.OrderByDescending(x => x.Correctness).Take(1).ToArray();
            Population[] orderedChildren = child.OrderByDescending(x => x.Correctness).Take(1).ToArray();
            Population bestIndividual;
            if (orderedParents[0].Correctness > orderedChildren[0].Correctness)
            {
                bestIndividual = orderedParents[0];
            }
            else
            {
                bestIndividual = orderedChildren[0];
            }
            newPop[0] = bestIndividual;

            // Retains 75% of bites from best individual
            int amountToFlip = (int)(75 * chromLenght) / 100;
            int[] toFlip = new int[amountToFlip];
            var sb = new System.Text.StringBuilder();
            for (int i = 1; i < popSize; i++)
            {
                // Make array with amountToFlip numbers
                for (int j = 0; j < amountToFlip; j++)
                {
                    do
                    {
                        generated = rnd.Next(minValue: 0, maxValue: chromLenght);
                    } while (!(!toFlip.Contains(generated)) && generated >= 0 && generated < chromLenght);
                    toFlip[j] = generated;
                    
                }

                toFlip = toFlip.OrderBy(x=>x).ToArray();

                for (int j = 0; j < chromLenght; j++)
                {
                    if (toFlip.Contains(j))
                    {
                        sb.Append(rnd.Next(0, 2));
                    }
                    else
                    {
                        sb.Append(bestIndividual.Value[j]);
                    }
                }
                newPop[i].Value = sb.ToString();
                sb.Clear();
            }
        }

        static void Main(string[] args)
        {
            // Initialization
            int wantedIterations;
            string targetString;
            string defaultString = "01010001000010000101011001000101111110110100100111001110010100101010010000101001100010001001101111001101000111100001001111011011000101001011100100111110001111000101000001101000001110111111101011001110000010110110101100100010010000111010011010010101000110110100110001000000111010000100011000010110101001100010011100010100110";
            int chromosomeLenght;
            int populationSize;
            int delta;
            Stopwatch stopwatch = new Stopwatch();

            
            // User input
            // Getting population size
            Console.Write("Population size: ");
            populationSize = Convert.ToInt32(Console.ReadLine());
            // Getting number of interations
            Console.Write("Number of iterations: ");
            wantedIterations = Convert.ToInt32(Console.ReadLine());
            // Getting target string
            Console.Write("Target string (empty for default): ");
            string inputString = Convert.ToString(Console.ReadLine());
            if (inputString == string.Empty)
            {
                targetString = defaultString;
            }
            else
            {
                targetString = inputString;
            }
            chromosomeLenght = targetString.Length;
            delta = chromosomeLenght / 4;
            // User input end


            Population[] parent = new Population[populationSize];
            Population[] child = new Population[populationSize];
            Population[] newPop = new Population[populationSize];

            randomFill(parent, chromosomeLenght);
            CheckCorrectnes(parent, chromosomeLenght, targetString);
            stopwatch.Start();
            // Inititialization end


            for (int i = 0; i < wantedIterations; i++)
            {
                // Children generation
                int amountGenerated = childGeneration(parent, chromosomeLenght, delta, child);
                

                // No children generated
                if (amountGenerated < populationSize)
                {
                    delta -= 1;
                }
                // Successful generation
                else
                {
                    CheckCorrectnes(child, chromosomeLenght, targetString);
                    TakeBest(parent, child, populationSize, newPop);
                    parent = newPop;
                }

                // Convergance detected
                if (delta == 0)
                {
                    Console.WriteLine("Convergance detected - Reseting");
                    // TODO: Reinitialization
                    Reinitialization(parent, child, populationSize, chromosomeLenght, newPop);
                    parent = newPop;
                    CheckCorrectnes(parent, chromosomeLenght, targetString);
                    i = 0;
                    delta = chromosomeLenght / 4;
                }

                // Print stats
                Console.WriteLine($"Iteration: {i}\tBest: {parent.Max(x => x.Correctness)}%\tWorst: {parent.Min(x => x.Correctness)}%\tDelta: {delta}");

                // Print best individual
                var best = parent.OrderByDescending(x=>x.Correctness).Take(1).ToList();
                Console.WriteLine($"{best[0].Value}\n");
                
                // Checks if perfect individual is made
                if (parent.Where(x=>x.Correctness == 100).Count() != 0)
                {
                    i = wantedIterations;
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        }
    }
}