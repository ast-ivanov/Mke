namespace MkeRz
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    using Mke;
    using Mke.Extensions;
    using Mke.Interfaces;
    using Mke.Services;
    
    enum Edge { Right, Top, Left, Bottom }

    internal class Program
    {
        const double lambda = 1, gamma = 1, beta = 1;
        const int Kr = 4, Kz = 4;
        static double[] r = { 1, 2, 3, 4, 5 };
        static double[] z = { 1, 2, 3, 4, 5 };

        static void Main(string[] args)
        {
            var slae = GeneratePortrait();
            
            #region Формирование глобальной матрицы и вектора правой части

            for (var i = 0; i < Kz; i++)
            {
                for (var j = 0; j < Kr; j++)
                {
                    var number = i * r.Length + j;
                    AddLocal(slae, number);
                }
            }

            #endregion

            #region Краевые

            //нижняя граница
            for (int i = 0; i < r.Length - 1; i++)
            {
                //kuslau1(i, i + 1);
                //kuslau2(i, i + 1, Edge.Bottom);
                //kuslau3(i, i + 1, Edge.Bottom);
            }

            //верхняя граница
            for (int i = r.Length * z.Length - r.Length; i < r.Length * z.Length - 1; i++)
            {
                //kuslau1(i, i + 1);
                //kuslau2(i, i + 1, Edge.Top);
                //kuslau3(i, i + 1, Edge.Top);
            }

            //правая граница
            for (int i = r.Length - 1; i < r.Length * z.Length - 1; i += r.Length)
            {
                //kuslau1(i, i + r.Length);
                //kuslau2(i, i + r.Length, Edge.Right);
                //kuslau3(i, i + r.Length, Edge.Right);
            }

            //левая граница
            for (int i = 0; i < r.Length * z.Length - r.Length; i += r.Length)
            {
                //kuslau1(i, i + r.Length);
                //kuslau2(i, i + r.Length, Edge.Left);
                //kuslau3(i, i + r.Length, Edge.Left);
            }

            #endregion

            #region Решение СЛАУ

            slae.CalculateLU(true);
            slae.CalculateLOS(200, 1e-10, out var iterationCount, out var discrepancy);

            #endregion

            switch (args[0])
            {
                case "0":
                    ConsoleOut(slae);
                    break;
                case "1":
                    FileOut(slae);
                    break;
            }

            Console.ReadKey();
        }

        static void FileOut(ISlaeService slae)
        {
            using (var file = File.OpenWrite("LOS.txt"))
            {
                using (var sw = new StreamWriter(file))
                {
                    for (var i = 0; i < r.Length * z.Length; i++)
                    {
                        sw.Write($"{slae.q[i]:0.########}\t{U(r[i % r.Length], z[i / r.Length]):0.###}");
                        sw.WriteLine();
                    }
                }
            }
        }

        static void ConsoleOut(ISlaeService slae)
        {
            for (int i = 0; i < r.Length * z.Length; i++)
            {
                Console.Write($"{slae.q[i]:0.########}\t{U(r[i % r.Length], z[i / r.Length]):0.###}");
                Console.WriteLine();
            }
        }

        static SlaeService GeneratePortrait()
        {
            var pairs = new HashSet<Pair>();

            //цикл по конечным элементам
            for (var i = 0; i < Kz; i++)
            {
                for (var j = 0; j < Kr; j++)
                {
                    var number = i * r.Length + j;

                    pairs.Add(new Pair(number + 1, number));
                    pairs.Add(new Pair(number + r.Length, number));
                    pairs.Add(new Pair(number + r.Length + 1, number));
                    pairs.Add(new Pair(number + r.Length + 1, number + 1));
                    pairs.Add(new Pair(number + r.Length + 1, number + r.Length));
                    pairs.Add(new Pair(number + r.Length, number + 1));
                }
            }

            var bind = new List<Pair>(pairs);
            bind.SortPairs(r.Length * z.Length);
            var ggl = new double[bind.Count];
            var jg = new int[bind.Count];
            var ig = new int[r.Length * z.Length + 1];

            var count = 2;
            for (var i = 0; i < bind.Count; i++)
            {
                jg[i] = bind[i].Second;
                
                if (i != 0 && bind[i].First > bind[i - 1].First)
                {
                    ig[count++] = i;
                }
            }
            ig[r.Length * z.Length] = bind.Count;

            return new SlaeService(r.Length * z.Length, ggl, ig, jg);
        }

        static void AddLocal(ISlaeService slae, int number)
        {
            var r1 = r[number % r.Length];
            var z1 = z[number / r.Length];
            var r2 = r[number % r.Length + 1];
            var z2 = z[number / r.Length + 1];
            var hr = r2 - r1;
            var hz = z2 - z1;

            var G = new double[4, 4];
            var M = new double[4, 4];

            double[,] Gr = {
                { (r2 + r1) / 2 / hr, -(r2 + r1) / 2 / hr },
                { -(r2 + r1) / 2 / hr, (r2 + r1) / 2 / hr }
            };

            var Mr = Program.Mr(r1, r2);

            double[,] Gz = {
                { 1 / hz, -1 / hz },
                { -1 / hz, 1 / hz }
            };

            var Mz = Program.Mz(z1, z2);

            #region Матрицы жёсткости и массы

            G[0, 0] = lambda * (Gr[0, 0] * Mz[0, 0] + Mr[0, 0] * Gz[0, 0]);
            G[0, 1] = lambda * (Gr[0, 1] * Mz[0, 0] + Mr[0, 1] * Gz[0, 0]);
            G[0, 2] = lambda * (Gr[0, 0] * Mz[0, 1] + Mr[0, 0] * Gz[0, 1]);
            G[0, 3] = lambda * (Gr[0, 1] * Mz[0, 1] + Mr[0, 1] * Gz[0, 1]);
            G[1, 0] = G[0, 1];
            G[1, 1] = lambda * (Gr[1, 1] * Mz[0, 0] + Mr[1, 1] * Gz[0, 0]);
            G[1, 2] = lambda * (Gr[1, 0] * Mz[0, 1] + Mr[1, 0] * Gz[0, 1]);
            G[1, 3] = lambda * (Gr[1, 1] * Mz[0, 1] + Mr[1, 1] * Gz[0, 1]);
            G[2, 0] = G[0, 2];
            G[2, 1] = G[1, 2];
            G[2, 2] = lambda * (Gr[0, 0] * Mz[1, 1] + Mr[0, 0] * Gz[1, 1]);
            G[2, 3] = lambda * (Gr[0, 1] * Mz[1, 1] + Mr[0, 1] * Gz[1, 1]);
            G[3, 0] = G[0, 3];
            G[3, 1] = G[1, 3];
            G[3, 2] = G[2, 3];
            G[3, 3] = lambda * (Gr[1, 1] * Mz[1, 1] + Mr[1, 1] * Gz[1, 1]);

            M[0, 0] = gamma * Mr[0, 0] * Mz[0, 0];
            M[0, 1] = gamma * Mr[0, 1] * Mz[0, 0];
            M[0, 2] = gamma * Mr[0, 0] * Mz[0, 1];
            M[0, 3] = gamma * Mr[0, 1] * Mz[0, 1];
            M[1, 0] = M[0, 1];
            M[1, 1] = gamma * Mr[1, 1] * Mz[0, 0];
            M[1, 2] = gamma * Mr[1, 0] * Mz[0, 1];
            M[1, 3] = gamma * Mr[1, 1] * Mz[0, 1];
            M[2, 0] = M[0, 2];
            M[2, 1] = M[1, 2];
            M[2, 2] = gamma * Mr[0, 0] * Mz[1, 1];
            M[2, 3] = gamma * Mr[0, 1] * Mz[1, 1];
            M[3, 0] = M[0, 3];
            M[3, 1] = M[1, 3];
            M[3, 2] = M[2, 3];
            M[3, 3] = gamma * Mr[1, 1] * Mz[1, 1];

            #endregion

            #region Добавление в матрицу А

            slae.GetElementOfA(number, number) += G[0, 0] + M[0, 0];
            slae.GetElementOfA(number, number + 1) += G[0, 1] + M[0, 1];
            slae.GetElementOfA(number, number + r.Length) += G[0, 2] + M[0, 2];
            slae.GetElementOfA(number, number + r.Length + 1) += G[0, 3] + M[0, 3];
            slae.GetElementOfA(number + 1, number) += G[1, 0] + M[1, 0];
            slae.GetElementOfA(number + 1, number + 1) += G[1, 1] + M[1, 1];
            slae.GetElementOfA(number + 1, number + r.Length) += G[1, 2] + M[1, 2];
            slae.GetElementOfA(number + 1, number + r.Length + 1) += G[1, 3] + M[1, 3];
            slae.GetElementOfA(number + r.Length, number) += G[2, 0] + M[2, 0];
            slae.GetElementOfA(number + r.Length, number + 1) += G[2, 1] + M[2, 1];
            slae.GetElementOfA(number + r.Length, number + r.Length) += G[2, 2] + M[2, 2];
            slae.GetElementOfA(number + r.Length, number + r.Length + 1) += G[2, 3] + M[2, 3];
            slae.GetElementOfA(number + r.Length + 1, number) += G[3, 0] + M[3, 0];
            slae.GetElementOfA(number + r.Length + 1, number + 1) += G[3, 1] + M[3, 1];
            slae.GetElementOfA(number + r.Length + 1, number + r.Length) += G[3, 2] + M[3, 2];
            slae.GetElementOfA(number + r.Length + 1, number + r.Length + 1) += G[3, 3] + M[3, 3];

            #endregion

            #region Добавление в вектор правой части

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    slae.b[number] += M[0, 2 * i + j] * f(r[number % r.Length + j], z[number / r.Length + i]);
                    slae.b[number + 1] += M[1, 2 * i + j] * f(r[number % r.Length + j], z[number / r.Length + i]);
                    slae.b[number + r.Length] += M[2, 2 * i + j] * f(r[number % r.Length + j], z[number / r.Length + i]);
                    slae.b[number + r.Length + 1] += M[3, 2 * i + j] * f(r[number % r.Length + j], z[number / r.Length + i]);
                }
            }

            #endregion
        }

        static double f(double r, double z)
        {
            //1-st case
            //return -lambda / r * (1 + z) + gamma * U(r, z);

            //2-nd case
            return gamma * z;

            //3-nd case
            //return -lambda / r + gamma * r;
        }

        static double U(double r, double z)
        {
            //1-st case
            //return r + z + r * z + 1;

            //2-nd case
            return z;

            //3-rd case
            //return r;
        }

        static double teta(double r, double z, Edge edge)
        {
            switch (edge)
            {
                case Edge.Right:
                    //1-st case
                    //return lambda * (1 + z);

                    //2-nd case
                    return 0;

                    //3-rd case
                    //return lambda * 1;

                case Edge.Top:
                    //1-st case
                    //return lambda * (1 + r);

                    //2-nd case
                    return lambda * 1;

                    //3-rd case
                    //return 0;

                case Edge.Left:
                    //1-st case
                    //return -lambda * (1 + z);

                    //2-nd case
                    return 0;

                    //3-rd case
                    //return -lambda * 1;

                case Edge.Bottom:
                    //1-st case
                    //return -lambda * (1 + r);

                    //2-nd case
                    return -lambda * 1;

                    //3-rd case
                    //return 0;

                default:
                    throw new ArgumentException();
            }
        }

        static double Ubeta(double r, double z, Edge edge)
        {
            return U(r, z) + teta(r, z, edge) / beta;
        }

        static void kuslau1(ISlaeService slae, int number1, int number2)
        {
            var r1 = r[number1 % r.Length];
            var z1 = z[number1 / r.Length];
            var r2 = r[number2 % r.Length];
            var z2 = z[number2 / r.Length];
            for (int i = 0; i < r.Length * z.Length; i++)
            {
                slae.GetElementOfA(number1, i) = i != number1 ? 0 : 1;
                slae.GetElementOfA(number2, i) = i != number2 ? 0 : 1;
            }
            slae.b[number1] = U(r1, z1);
            slae.b[number2] = U(r2, z2);
        }

        static void kuslau2(ISlaeService slae, int node1, int node2, Edge edge)
        {
            var r1 = r[node1 % r.Length];
            var z1 = z[node1 / r.Length];
            var r2 = r[node2 % r.Length];
            var z2 = z[node2 / r.Length];

            var teta1 = teta(r1, z1, edge);
            var teta2 = teta(r2, z2, edge);

            if (edge == Edge.Right || edge == Edge.Left)
            {
                var Mz = Program.Mz(z1, z2);
                slae.b[node1] += Mz[0, 0] * r1 * teta1 + Mz[0, 1] * r1 * teta2;
                slae.b[node2] += Mz[1, 0] * r1 * teta1 + Mz[1, 1] * r1 * teta2;
            }
            else
            {
                var Mr = Program.Mr(r1, r2);
                slae.b[node1] += Mr[0, 0] * teta1 + Mr[0, 1] * teta2;
                slae.b[node2] += Mr[1, 0] * teta1 + Mr[1, 1] * teta2;
            }
        }

        static void kuslau3(ISlaeService slae, int node1, int node2, Edge edge)
        {
            var r1 = r[node1 % r.Length];
            var z1 = z[node1 / r.Length];
            var r2 = r[node2 % r.Length];
            var z2 = z[node2 / r.Length];

            var ubeta1 = Ubeta(r1, z1, edge);
            var ubeta2 = Ubeta(r2, z2, edge);

            if (edge == Edge.Right || edge == Edge.Left)
            {
                var Mz = Program.Mz(z1, z2);
                slae.GetElementOfA(node1, node1) += beta * r1 * Mz[0, 0];
                slae.GetElementOfA(node1, node2) += beta * r1 * Mz[0, 1];
                slae.GetElementOfA(node2, node1) += beta * r1 * Mz[1, 0];
                slae.GetElementOfA(node2, node2) += beta * r1 * Mz[1, 1];

                slae.b[node1] += beta * (Mz[0, 0] * r1 * ubeta1 + Mz[0, 1] * r1 * ubeta2);
                slae.b[node2] += beta * (Mz[1, 0] * r1 * ubeta1 + Mz[1, 1] * r1 * ubeta2);
            }
            else
            {
                var Mr = Program.Mr(r1, r2);
                slae.GetElementOfA(node1, node1) += beta * Mr[0, 0];
                slae.GetElementOfA(node1, node2) += beta * Mr[0, 1];
                slae.GetElementOfA(node2, node1) += beta * Mr[1, 0];
                slae.GetElementOfA(node2, node2) += beta * Mr[1, 1];

                slae.b[node1] += beta * (Mr[0, 0] * ubeta1 + Mr[0, 1] * ubeta2);
                slae.b[node2] += beta * (Mr[1, 0] * ubeta1 + Mr[1, 1] * ubeta2);
            }
        }

        static double[,] Mr(double r1, double r2)
        {
            var hr = r2 - r1;
            return new[,]
            {
                { hr / 12 * (r2 + 3 * r1), hr / 12 * (r2 + r1) },
                { hr / 12 * (r2 + r1), hr / 12 * (3 * r2 + r1) }
            };
        }

        static double[,] Mz(double z1, double z2)
        {
            var hz = z2 - z1;
            return new[,]
            {
                { hz / 3, hz / 6 },
                { hz / 6, hz / 3 }
            };
        }
    }
}
