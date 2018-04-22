namespace MkeUi
{
    using Mke;
    using Mke.Interfaces;
    using Mke.Services;
    using Mke.Extensions;

    using System.Collections.Generic;
    using System;

    internal sealed class Solution : ISolution
    {
        public SolutionParams SolutionParams { get; set; }

        public (double[] q, double[] U) Calculate()
        {
            var r = SolutionParams.r;
            var z = SolutionParams.z;

            var Kr = r.Length - 1;
            var Kz = z.Length - 1;

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
                if (SolutionParams.BottomFirst)
                {
                    kuslau1(slae, i, i + 1);
                }

                if (SolutionParams.BottomSecond)
                {
                    kuslau2(slae, i, i + 1, EEdge.Bottom);
                }

                if (SolutionParams.BottomThird)
                {
                    kuslau3(slae, i, i + 1, EEdge.Bottom);
                }
            }

            //верхняя граница
            for (int i = r.Length * z.Length - r.Length; i < r.Length * z.Length - 1; i++)
            {
                if (SolutionParams.TopFirst)
                {
                    kuslau1(slae, i, i + 1);
                }

                if (SolutionParams.TopSecond)
                {
                    kuslau2(slae, i, i + 1, EEdge.Top);
                }

                if (SolutionParams.TopThird)
                {
                    kuslau3(slae, i, i + 1, EEdge.Top);
                }
            }

            //правая граница
            for (int i = r.Length - 1; i < r.Length * z.Length - 1; i += r.Length)
            {
                if (SolutionParams.RightFirst)
                {
                    kuslau1(slae, i, i + r.Length);
                }

                if (SolutionParams.RightSecond)
                {
                    kuslau2(slae, i, i + r.Length, EEdge.Right);
                }

                if (SolutionParams.RightThird)
                {
                    kuslau3(slae, i, i + r.Length, EEdge.Right);
                }
            }

            //левая граница
            for (int i = 0; i < r.Length * z.Length - r.Length; i += r.Length)
            {
                if (SolutionParams.LeftFirst)
                {
                    kuslau1(slae, i, i + r.Length);
                }

                if (SolutionParams.LeftSecond)
                {
                    kuslau2(slae, i, i + r.Length, EEdge.Left);
                }

                if (SolutionParams.LeftThird)
                {
                    kuslau3(slae, i, i + r.Length, EEdge.Left);
                }
            }

            #endregion

            #region Решение СЛАУ

            slae.CalculateLU(true);
            slae.CalculateLOS(200, 1e-10, out var iterationCount, out var discrepancy);

            #endregion

            var u = new double[r.Length * z.Length];
            for (int i = 0; i < r.Length * z.Length; i++)
            {
                u[i] = (U(r[i % r.Length], z[i / r.Length]));
            }

            return (slae.q, u);
        }

        private SlaeService GeneratePortrait()
        {
            var r = SolutionParams.r;
            var z = SolutionParams.z;

            var Kr = r.Length - 1;
            var Kz = z.Length - 1;

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

        private void AddLocal(ISlaeService slae, int number)
        {
            var r = SolutionParams.r;
            var z = SolutionParams.z;

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

            var Mr = GetMr(r1, r2);

            double[,] Gz = {
                { 1 / hz, -1 / hz },
                { -1 / hz, 1 / hz }
            };

            var Mz = GetMz(z1, z2);

            #region Матрицы жёсткости и массы

            G[0, 0] = SolutionParams.Lambda * (Gr[0, 0] * Mz[0, 0] + Mr[0, 0] * Gz[0, 0]);
            G[0, 1] = SolutionParams.Lambda * (Gr[0, 1] * Mz[0, 0] + Mr[0, 1] * Gz[0, 0]);
            G[0, 2] = SolutionParams.Lambda * (Gr[0, 0] * Mz[0, 1] + Mr[0, 0] * Gz[0, 1]);
            G[0, 3] = SolutionParams.Lambda * (Gr[0, 1] * Mz[0, 1] + Mr[0, 1] * Gz[0, 1]);
            G[1, 0] = G[0, 1];
            G[1, 1] = SolutionParams.Lambda * (Gr[1, 1] * Mz[0, 0] + Mr[1, 1] * Gz[0, 0]);
            G[1, 2] = SolutionParams.Lambda * (Gr[1, 0] * Mz[0, 1] + Mr[1, 0] * Gz[0, 1]);
            G[1, 3] = SolutionParams.Lambda * (Gr[1, 1] * Mz[0, 1] + Mr[1, 1] * Gz[0, 1]);
            G[2, 0] = G[0, 2];
            G[2, 1] = G[1, 2];
            G[2, 2] = SolutionParams.Lambda * (Gr[0, 0] * Mz[1, 1] + Mr[0, 0] * Gz[1, 1]);
            G[2, 3] = SolutionParams.Lambda * (Gr[0, 1] * Mz[1, 1] + Mr[0, 1] * Gz[1, 1]);
            G[3, 0] = G[0, 3];
            G[3, 1] = G[1, 3];
            G[3, 2] = G[2, 3];
            G[3, 3] = SolutionParams.Lambda * (Gr[1, 1] * Mz[1, 1] + Mr[1, 1] * Gz[1, 1]);

            M[0, 0] = SolutionParams.Gamma * Mr[0, 0] * Mz[0, 0];
            M[0, 1] = SolutionParams.Gamma * Mr[0, 1] * Mz[0, 0];
            M[0, 2] = SolutionParams.Gamma * Mr[0, 0] * Mz[0, 1];
            M[0, 3] = SolutionParams.Gamma * Mr[0, 1] * Mz[0, 1];
            M[1, 0] = M[0, 1];
            M[1, 1] = SolutionParams.Gamma * Mr[1, 1] * Mz[0, 0];
            M[1, 2] = SolutionParams.Gamma * Mr[1, 0] * Mz[0, 1];
            M[1, 3] = SolutionParams.Gamma * Mr[1, 1] * Mz[0, 1];
            M[2, 0] = M[0, 2];
            M[2, 1] = M[1, 2];
            M[2, 2] = SolutionParams.Gamma * Mr[0, 0] * Mz[1, 1];
            M[2, 3] = SolutionParams.Gamma * Mr[0, 1] * Mz[1, 1];
            M[3, 0] = M[0, 3];
            M[3, 1] = M[1, 3];
            M[3, 2] = M[2, 3];
            M[3, 3] = SolutionParams.Gamma * Mr[1, 1] * Mz[1, 1];

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

        private double f(double r, double z)
        {
            var gamma = SolutionParams.Gamma;

            //1-st case
            //return -gamma / r * (1 + z) + gamma * U(r, z);

            //2-nd case
            return gamma * z;

            //3-nd case
            //return -gamma / r + gamma * r;
        }

        private double U(double r, double z)
        {
            //1-st case
            //return r + z + r * z + 1;

            //2-nd case
            return z;

            //3-rd case
            //return r;
        }

        private double teta(double r, double z, EEdge edge)
        {
            var lambda = SolutionParams.Lambda;

            switch (edge)
            {
                case EEdge.Right:
                    //1-st case
                    //return lambda * (1 + z);

                    //2-nd case
                    return 0;

                //3-rd case
                //return lambda * 1;

                case EEdge.Top:
                    //1-st case
                    //return lambda * (1 + r);

                    //2-nd case
                    return lambda * 1;

                //3-rd case
                //return 0;

                case EEdge.Left:
                    //1-st case
                    //return -lambda * (1 + z);

                    //2-nd case
                    return 0;

                //3-rd case
                //return -lambda * 1;

                case EEdge.Bottom:
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

        private double Ubeta(double r, double z, EEdge edge) => U(r, z) + teta(r, z, edge) / SolutionParams.Beta;

        private void kuslau1(ISlaeService slae, int number1, int number2)
        {
            var r = SolutionParams.r;
            var z = SolutionParams.z;

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

        private void kuslau2(ISlaeService slae, int node1, int node2, EEdge edge)
        {
            var r = SolutionParams.r;
            var z = SolutionParams.z;

            var r1 = r[node1 % r.Length];
            var z1 = z[node1 / r.Length];
            var r2 = r[node2 % r.Length];
            var z2 = z[node2 / r.Length];

            var teta1 = teta(r1, z1, edge);
            var teta2 = teta(r2, z2, edge);

            if (edge == EEdge.Right || edge == EEdge.Left)
            {
                var Mz = GetMz(z1, z2);
                slae.b[node1] += Mz[0, 0] * r1 * teta1 + Mz[0, 1] * r1 * teta2;
                slae.b[node2] += Mz[1, 0] * r1 * teta1 + Mz[1, 1] * r1 * teta2;
            }
            else
            {
                var Mr = GetMr(r1, r2);
                slae.b[node1] += Mr[0, 0] * teta1 + Mr[0, 1] * teta2;
                slae.b[node2] += Mr[1, 0] * teta1 + Mr[1, 1] * teta2;
            }
        }

        private void kuslau3(ISlaeService slae, int node1, int node2, EEdge edge)
        {
            var r = SolutionParams.r;
            var z = SolutionParams.z;

            var r1 = r[node1 % r.Length];
            var z1 = z[node1 / r.Length];
            var r2 = r[node2 % r.Length];
            var z2 = z[node2 / r.Length];

            var ubeta1 = Ubeta(r1, z1, edge);
            var ubeta2 = Ubeta(r2, z2, edge);

            if (edge == EEdge.Right || edge == EEdge.Left)
            {
                var Mz = GetMz(z1, z2);
                slae.GetElementOfA(node1, node1) += SolutionParams.Beta * r1 * Mz[0, 0];
                slae.GetElementOfA(node1, node2) += SolutionParams.Beta * r1 * Mz[0, 1];
                slae.GetElementOfA(node2, node1) += SolutionParams.Beta * r1 * Mz[1, 0];
                slae.GetElementOfA(node2, node2) += SolutionParams.Beta * r1 * Mz[1, 1];

                slae.b[node1] += SolutionParams.Beta * (Mz[0, 0] * r1 * ubeta1 + Mz[0, 1] * r1 * ubeta2);
                slae.b[node2] += SolutionParams.Beta * (Mz[1, 0] * r1 * ubeta1 + Mz[1, 1] * r1 * ubeta2);
            }
            else
            {
                var Mr = GetMr(r1, r2);
                slae.GetElementOfA(node1, node1) += SolutionParams.Beta * Mr[0, 0];
                slae.GetElementOfA(node1, node2) += SolutionParams.Beta * Mr[0, 1];
                slae.GetElementOfA(node2, node1) += SolutionParams.Beta * Mr[1, 0];
                slae.GetElementOfA(node2, node2) += SolutionParams.Beta * Mr[1, 1];

                slae.b[node1] += SolutionParams.Beta * (Mr[0, 0] * ubeta1 + Mr[0, 1] * ubeta2);
                slae.b[node2] += SolutionParams.Beta * (Mr[1, 0] * ubeta1 + Mr[1, 1] * ubeta2);
            }
        }

        private double[,] GetMr(double r1, double r2)
        {
            var hr = r2 - r1;
            return new[,]
            {
                { hr / 12 * (r2 + 3 * r1), hr / 12 * (r2 + r1) },
                { hr / 12 * (r2 + r1), hr / 12 * (3 * r2 + r1) }
            };
        }

        private double[,] GetMz(double z1, double z2)
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
