using System;
using System.Linq;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;

namespace kNN
{
    // Klasa do importu danych z zestawy iris.csv
    public class DataPoint
    {
        // | sepal_length | sepal_width | petal_length | petal_width | species |
        // |     5.1      |     3.5     |      1.4     |      0.2    |  setosa |
        public double sepal_length { get; set; }
        public double sepal_width { get; set; }
        public double petal_length { get; set; }
        public double petal_width { get; set; }
        public string species { get; set; }
    }

    // Klasa wymagana do ewaluacji danych
    public class EvaluatedPoint : DataPoint
    {
        public double Distance { get; set; }
    }


    class Program
    {
        /// <summary>
        /// Ładuje zestaw danych Iris z plku csv
        /// </summary>
        /// <returns>Listę typu DataPoint z danymi</returns>
        public static List<DataPoint> LoadData()
        {
            using var reader = new StreamReader("iris.csv");
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            List<DataPoint> dataSet = csvReader.GetRecords<DataPoint>().ToList();
            return dataSet;
        }

        /// <summary>
        /// Dzieli podany zestaw danych według parametrów
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="testDataSize"></param>
        /// <param name="userObj"></param>
        public static void CrossValidate(List<DataPoint> dataSet, int testDataSize, DataPoint userObj, int kValue)
        {
            List<DataPoint> testSet = new List<DataPoint>();
            List<DataPoint> trainSet = new List<DataPoint>();

            // Obliczenie wymaganych iteracji
            int numOfFolds = dataSet.Count() / testDataSize;

            // Wykonanie iteracji
            for (int i = 0; i < numOfFolds; i++)
            {
                Console.WriteLine($"\n\nCrossValidacja nr. {i}");
                testSet = dataSet.Skip(testDataSize * i).Take(testDataSize).ToList();
                trainSet = dataSet.Except(testSet).ToList();

                EvaluateInput(dataSet, testSet, userObj, kValue);
                EvaluateTest(dataSet, testSet, kValue, trainSet);
            }


        }


        /// <summary>
        /// Sprawdzania do jakiej klasy pasuje podany przez uzytkownika punkt
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="testSet"></param>
        /// <param name="userObj"></param>
        /// <param name="kValue"></param>
        public static void EvaluateInput(List<DataPoint> dataSet, List<DataPoint> testSet, DataPoint userObj, int kValue)
        {
            double sl, sw, pl, pw, distance;
            List<EvaluatedPoint> evaluation = new List<EvaluatedPoint>();

            foreach (var point in dataSet)
            {
                if (!testSet.Contains(point))
                {
                    // Obliczanie metryki eukledisowej
                    sl = Math.Pow(point.sepal_length - userObj.sepal_length, 2);
                    sw = Math.Pow(point.sepal_width - userObj.sepal_width, 2);
                    pl = Math.Pow(point.petal_length - userObj.petal_length, 2);
                    pw = Math.Pow(point.petal_width - userObj.petal_width, 2);

                    distance = Math.Sqrt(sl + sw + pl + pw);

                    EvaluatedPoint evaluatedPoint = new EvaluatedPoint()
                    {
                        sepal_length = point.sepal_length,
                        sepal_width = point.sepal_width,
                        petal_length = point.petal_length,
                        petal_width = point.petal_width,
                        species = point.species,
                        Distance = distance
                    };

                    evaluation.Add(evaluatedPoint);
                }
            }

            // Lista opracowanych rezultatów
            var results = evaluation
                .OrderBy(x => x.Distance)           // Posortuj
                .Take(kValue)                       // Weź k najlepszych
                .GroupBy(x=>x.species)              // Pogrupuj
                .OrderByDescending(x=>x.Count())    // Znajdz najlepszych
                .ToList();

            // Ilość punktów Virginica
            double virginicaAmount = evaluation
                .OrderBy(x=>x.Distance)
                .Take(kValue)
                .Where(x=>x.species == "virginica")
                .Count();
            // Ilość punktów Versicolor
            double versicolorAmount = evaluation
                .OrderBy(x => x.Distance)
                .Take(kValue)
                .Where(x => x.species == "versicolor")
                .Count(); ;
            // Ilość punktów Setosa
            double setosaAmount = evaluation
                .OrderBy(x => x.Distance)
                .Take(kValue)
                .Where(x => x.species == "setosa")
                .Count();

            string bestGuess = results
                .OrderByDescending(x => x.Select(x => x.Distance))
                .Select(x => x.Select(x => x.species))
                .Take(1)
                .ToString();

            Console.WriteLine("\nPodany punkt (procentowa szansa zgodności):");
            Console.WriteLine($"   Virginica:  {(virginicaAmount / kValue) * 100}%");
            Console.WriteLine($"   Versicolor: {(versicolorAmount / kValue) * 100}%");
            Console.WriteLine($"   Setosa:     {(setosaAmount / kValue) * 100}%");
        }

        public static void EvaluateTest(List<DataPoint> dataSet, List<DataPoint> testSet, int kValue, List<DataPoint> trainSet)
        {
            int goodGuess = 0;

            foreach (var testPoint in testSet)
            {
                double sl, sw, pl, pw, distance;
                List<EvaluatedPoint> evaluation = new List<EvaluatedPoint>();

                foreach (var dataPoints in dataSet.Except(testSet).ToList())
                {
                    if (true)
                    {
                        // Obliczanie metryki eukledisowej
                        sl = Math.Abs(Math.Pow(dataPoints.sepal_length - testPoint.sepal_length, 2));
                        sw = Math.Abs(Math.Pow(dataPoints.sepal_width - testPoint.sepal_width, 2));
                        pl = Math.Abs(Math.Pow(dataPoints.petal_length - testPoint.petal_length, 2));
                        pw = Math.Abs(Math.Pow(dataPoints.petal_width - testPoint.petal_width, 2));

                        distance = Math.Sqrt(sl + sw + pl + pw);

                        EvaluatedPoint evaluatedPoint = new EvaluatedPoint()
                        {
                            sepal_length = dataPoints.sepal_length,
                            sepal_width = dataPoints.sepal_width,
                            petal_length = dataPoints.petal_length,
                            petal_width = dataPoints.petal_width,
                            species = dataPoints.species,
                            Distance = distance
                        };

                        evaluation.Add(evaluatedPoint);
                    }
                }

                // Lista opracowanych rezultatów
                var results = evaluation
                    .OrderBy(x => x.Distance)           // Posortuj
                    .Take(kValue)                       // Weź k najlepszych
                    .GroupBy(x => x.species)              // Pogrupuj
                    .OrderByDescending(x => x.Count())    // Znajdz najlepszych
                    .ToList();

                // Ilość punktów Virginica
                double virginicaAmount = evaluation
                    .OrderBy(x => x.Distance)
                    .Take(kValue)
                    .Where(x => x.species == "virginica")
                    .Count();
                // Ilość punktów Versicolor
                double versicolorAmount = evaluation
                    .OrderBy(x => x.Distance)
                    .Take(kValue)
                    .Where(x => x.species == "versicolor")
                    .Count(); ;
                // Ilość punktów Setosa
                double setosaAmount = evaluation
                    .OrderBy(x => x.Distance)
                    .Take(kValue)
                    .Where(x => x.species == "setosa")
                    .Count();

                string bestGuess = "";
                if (virginicaAmount > versicolorAmount && virginicaAmount > setosaAmount)
                {
                    bestGuess = "virginica";
                }
                else if (versicolorAmount > virginicaAmount && versicolorAmount > setosaAmount)
                {
                    bestGuess = "versicolor";
                }
                else if (setosaAmount > virginicaAmount && setosaAmount > versicolorAmount)
                {
                    bestGuess = "setosa";
                }

                //DataPoint guess = new DataPoint()
                //{
                //    sepal_length = testPoint.sepal_length,
                //    sepal_width = testPoint.sepal_width,
                //    petal_length = testPoint.petal_length,
                //    petal_width = testPoint.petal_width,
                //    species = bestGuess,
                //};

                //if (dataSet.Contains(guess))
                //{
                //    goodGuess++;
                //}

                goodGuess += dataSet
                    .Where(x => x.petal_length == testPoint.petal_length)
                    .Where(x => x.petal_width == testPoint.petal_width)
                    .Where(x => x.sepal_length == testPoint.sepal_length)
                    .Where(x => x.sepal_width == testPoint.sepal_width)
                    .Where(x => x.species == bestGuess)
                    .Distinct()
                    .Count();
            }
            Console.WriteLine($"Test set: {goodGuess }");
        }

        static void Main(string[] args)
        {
            // Stopwatch dla statystyki
            var sw = new Stopwatch();


            // Ladowanie zestawu danych do zmiennej dataSet
            List<DataPoint> dataSet = new List<DataPoint>();
            dataSet = LoadData();
            Console.WriteLine($"Załadowano iris.csv, {dataSet.Count()} wierszy");


            // Ustalenie zmiennych parametrów
            int kValue;
            int testDataSize;
            double sepal_length, sepal_width, petal_length, petal_width;


            // Pobieranie danych od użytkownika
            Console.Write("\nPodaj K (ilość szukanych sąsiadów): ");
            kValue = Convert.ToInt32(Console.ReadLine());

            Console.Write("\nPodaj rozmiar zestawu testowego (polecane: 75, 50, 30, 25, 10): ");
            testDataSize = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("\nPodaj sprawdzany punkt");
            Console.Write("   sepal_length: ");
            sepal_length = Convert.ToDouble(Console.ReadLine());
            Console.Write("   sepal_width:  ");
            sepal_width = Convert.ToDouble(Console.ReadLine());
            Console.Write("   petal_length: ");
            petal_length = Convert.ToDouble(Console.ReadLine());
            Console.Write("   petal_width:  ");
            petal_width = Convert.ToDouble(Console.ReadLine());

            DataPoint userObj = new DataPoint()
            {
                sepal_length = sepal_length,
                sepal_width = sepal_width,
                petal_length = petal_length,
                petal_width = petal_width
            };

            // Wykonanie crossvalidacji i sprawdzania podanego punktu
            CrossValidate(dataSet, testDataSize, userObj, kValue);
        }
    }
}
