namespace Mke.Xyz.PointSource
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using Interfaces;

    internal class Program
    {
        static ISolution<PointSolutionParams> _solution;

        private static double ro;

        public static void Main(string[] args)
        {
            try
            {
                var (solutionParams,middle) = ReadParamsFromJson();
                ro = solutionParams.Ro;

                var (q, _) = new Solution { SolutionParams = solutionParams }.Calculate();

                var xLen = solutionParams.x.Length;
                var yLen = solutionParams.y.Length;
                var zLen = solutionParams.z.Length;

                var startNode = (zLen - 1) * xLen * yLen + yLen / 2 * xLen + middle;
                var endNode = startNode + middle;
                var x = solutionParams.x;

//                for (int i = 0; i < q.Length; i++)
//                {
//                    Console.WriteLine(q[i]);
//                }

                using (var sw = new StreamWriter("C:\\Users\\Arthur\\Desktop\\1.txt", false, System.Text.Encoding.Default))
                {
                    sw.WriteLine();
                    sw.WriteLine();

                    for (int i = startNode, j = middle; i < endNode; i++, j++)
                    {
                        sw.WriteLine($"{x[j]} {q[i]}");
                        Console.WriteLine($"{x[j]:N5}\t{q[i]}\t");
                    }
                }
//
//                using (var sw = new StreamWriter("C:\\Users\\Arthur\\Desktop\\solution.txt", false, System.Text.Encoding.Default))
//                {
//                    sw.WriteLine();
//                    sw.WriteLine();
//
//                    for (int i = startNode, j = middle; i < endNode; i++, j++)
//                    {
//                        sw.WriteLine($"{x[j]} {u(x[j])}");
//                    }
//                }

                Console.WriteLine("Запись в файл произведена");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

//        static PointSolutionParams ReadParamsFromJson()
//        {
//            var jsonFormatter = new DataContractJsonSerializer(typeof(PointSolutionParams));
//
//            PointSolutionParams solutionParams;
//
//            using (var fs = new FileStream("SolutionParams.json", FileMode.Open))
//            {
//                solutionParams = (PointSolutionParams)jsonFormatter.ReadObject(fs);
//            }
//
//            var size = 21;
//            var half = size / 2;
//
//            var x = new double[3];
//            var y = new double[3];
//            var z = new double[3];
//
////            for (int i = 0; i < size; i++)
////            {
////                y[i] = i - half;
////            }
//
//            z[0] = 0;
//            z[1] = 1;
//            z[2] = 2;
//            y[0] = 0;
//            y[1] = 1;
//            y[2] = 2;
//            x[0] = 0;
//            x[1] = 1;
//            x[2] = 2;
//
//            solutionParams.x = x;
//            solutionParams.y = y;
//            solutionParams.z = z;
//
//            return solutionParams;
//        }
        static (PointSolutionParams, int) ReadParamsFromJson()
        {
            var jsonFormatter = new DataContractJsonSerializer(typeof(PointSolutionParams));

            PointSolutionParams solutionParams;

            using (var fs = new FileStream("SolutionParams.json", FileMode.Open))
            {
                solutionParams = (PointSolutionParams)jsonFormatter.ReadObject(fs);
            }

            var size = 21;
            var half = size / 2;

            var y = new double[size];
            var z = new double[2];

            for (int i = 0; i < size; i++)
            {
                y[i] = i - half;
            }
            z[0] = 0;
            z[1] = 1;
            var (x, middle) = BuildGrid();

            solutionParams.y = y;
            solutionParams.z = z;
            solutionParams.x = x;

            return (solutionParams, middle);
        }

        static double u(double r)
        {
            return ro / (4 * Math.PI * r);
        }

        static (double[], int) BuildGrid()
        {
            const double kr = 1.1;

            var xList = new List<double>();
            var coord = -500.0;
            var h = 53.0;

            do
            {
                xList.Add(coord);
                h /= kr;
                coord += h;
            } while (coord < 0);

            var middle = xList.Count;

            xList.Add(coord - h / 2);

            do
            {
                xList.Add(coord);
                h *= kr;
                coord += h;
            } while (coord < 500);

            return (xList.ToArray(), middle);
        }
    }
}