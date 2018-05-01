namespace MkeXyzUi
{
    using Mke;
    using Mke.Interfaces;
    using Mke.Services;
    using Mke.Extensions;

    using System.Collections.Generic;
    using System;

    /// <inheritdoc />
    internal sealed class Solution : ISolution
    {
        public SolutionParams SolutionParams { get; set; }

        public (double[] q, double[] U) Calculate()
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            var Kx = x.Length - 1;
            var Ky = y.Length - 1;
            var Kz = z.Length - 1;

            var slae = GeneratePortrait();

            #region Формирование глобальной матрицы и вектора правой части

            for (var i = 0; i < Kz; i++)
            {
                for (int j = 0; j < Ky; j++)
                {
                    for (var k = 0; k < Kx; k++)
                    {
                        var number = i * x.Length * y.Length + j * x.Length + k;
                        AddLocal(slae, number);
                    }
                }
            }

            #endregion

            #region Краевые

            //нижняя граница
            var startNode = 0;
            var endNode = x.Length * y.Length - 1;
            var delta = 1;

            for (int i = startNode; i < endNode; i += delta)
            {
                if (SolutionParams.BottomSecond && (i + 1) % x.Length != 0)
                {
                    kuslau2(slae, (i, i + 1, i + x.Length, i + x.Length + 1), EEdge.Bottom);
                }

                if (SolutionParams.BottomThird && (i + 1) % x.Length != 0)
                {
                    kuslau3(slae, (i, i + 1, i + x.Length, i + x.Length + 1), EEdge.Bottom);
                }

                if (SolutionParams.BottomFirst)
                {
                    kuslau1(slae, i);
                }
            }

            //верхняя граница
            startNode = x.Length * y.Length * z.Length - x.Length * y.Length;
            endNode = x.Length * y.Length * z.Length - 1;

            for (int i = startNode; i < endNode; i += delta)
            {
                if (SolutionParams.TopSecond && (i + 1) % x.Length != 0)
                {
                    kuslau2(slae, (i, i + 1, i + x.Length, i + x.Length + 1), EEdge.Top);
                }

                if (SolutionParams.TopThird && (i + 1) % x.Length != 0)
                {
                    kuslau3(slae, (i, i + 1, i + x.Length, i + x.Length + 1), EEdge.Top);
                }

                if (SolutionParams.TopFirst)
                {
                    kuslau1(slae, i);
                }
            }

            //левая граница
            startNode = 0;
            endNode = x.Length * y.Length * z.Length - x.Length;
            delta = x.Length;

            for (int i = startNode; i < endNode; i += delta)
            {
                if (SolutionParams.LeftSecond)
                {
                    kuslau2(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Left);
                }

                if (SolutionParams.LeftThird)
                {
                    kuslau3(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Left);
                }

                if (SolutionParams.LeftFirst)
                {
                    kuslau1(slae, i);
                }
            }

            //правая граница
            startNode = x.Length - 1;
            endNode = x.Length * y.Length * z.Length;

            for (int i = startNode; i < endNode; i += delta)
            {
                if (SolutionParams.RightSecond)
                {
                    kuslau2(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Right);
                }

                if (SolutionParams.RightThird)
                {
                    kuslau3(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Right);
                }

                if (SolutionParams.RightFirst)
                {
                    kuslau1(slae, i);
                }
            }

            //задняя граница
            startNode = x.Length - 1;
            endNode = x.Length * y.Length * z.Length;

            for (int i = startNode; i < endNode; i += delta)
            {
                if (SolutionParams.BackSecond)
                {
                    kuslau2(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Back);
                }

                if (SolutionParams.BackThird)
                {
                    kuslau3(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Back);
                }

                if (SolutionParams.BackFirst)
                {
                    kuslau1(slae, i);
                }
            }

            //передняя граница
            startNode = x.Length - 1;
            endNode = x.Length * y.Length * z.Length;

            for (int i = startNode; i < endNode; i += delta)
            {
                if (SolutionParams.FrontSecond)
                {
                    kuslau2(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Front);
                }

                if (SolutionParams.FrontThird)
                {
                    kuslau3(slae, (i, i + x.Length, i + x.Length * y.Length, i + x.Length * y.Length + x.Length), EEdge.Front);
                }

                if (SolutionParams.FrontFirst)
                {
                    kuslau1(slae, i);
                }
            }

            #endregion

            #region Решение СЛАУ

            slae.CalculateLU(true);
            slae.CalculateLOS(200, 1e-10, out var iterationCount, out var discrepancy);

            #endregion

            var n = SolutionParams.N;
            var u = new double[n];
            for (var i = 0; i < n; i++)
            {
                var (xi, yi, zi) = GetNodeCoordinates(i);
                u[i] = U(xi, yi, zi);
            }

            return (slae.q, u);
        }

        private SlaeService GeneratePortrait()
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            var xLength = x.Length;
            var yLength = y.Length;
            var zLength = z.Length;

            var Kx = xLength - 1;
            var Ky = yLength - 1;
            var Kz = zLength - 1;

            var pairs = new HashSet<Pair>();

            //цикл по конечным элементам
            for (var i = 0; i < Kz; i++)
            {
                for (var j = 0; j < Ky; j++)
                {
                    for (var k = 0; k < Kx; k++)
                    {
                        var number = i * xLength * yLength + j * xLength + k;

                        pairs.Add(new Pair(number + 1, number));
                        pairs.Add(new Pair(number + xLength, number));
                        pairs.Add(new Pair(number + xLength + 1, number));
                        pairs.Add(new Pair(number + xLength * yLength, number));
                        pairs.Add(new Pair(number + xLength * yLength + 1, number));
                        pairs.Add(new Pair(number + xLength * yLength + xLength, number));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number));
                        pairs.Add(new Pair(number + xLength, number + 1));
                        pairs.Add(new Pair(number + xLength + 1, number + 1));
                        pairs.Add(new Pair(number + xLength * yLength, number + 1));
                        pairs.Add(new Pair(number + xLength * yLength + 1, number + 1));
                        pairs.Add(new Pair(number + xLength * yLength + xLength, number + 1));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number + 1));
                        pairs.Add(new Pair(number + xLength + 1, number + xLength));
                        pairs.Add(new Pair(number + xLength * yLength, number + xLength));
                        pairs.Add(new Pair(number + xLength * yLength + 1, number + xLength));
                        pairs.Add(new Pair(number + xLength * yLength + xLength, number + xLength));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number + xLength));
                        pairs.Add(new Pair(number + xLength * yLength, number + xLength + 1));
                        pairs.Add(new Pair(number + xLength * yLength + 1, number + xLength + 1));
                        pairs.Add(new Pair(number + xLength * yLength + xLength, number + xLength + 1));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number + xLength + 1));
                        pairs.Add(new Pair(number + xLength * yLength + 1, number + xLength * yLength));
                        pairs.Add(new Pair(number + xLength * yLength + xLength, number + xLength * yLength));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number + xLength * yLength));
                        pairs.Add(new Pair(number + xLength * yLength + xLength, number + xLength * yLength + 1));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number + xLength * yLength + 1));
                        pairs.Add(new Pair(number + xLength * yLength + xLength + 1, number + xLength * yLength + xLength));
                    }
                }
            }

            var bind = new List<Pair>(pairs);
            bind.SortPairs(xLength * yLength * zLength);
            var ggl = new double[bind.Count];
            var jg = new int[bind.Count];
            var ig = new int[xLength * yLength * zLength + 1];

            var count = 2;
            for (var i = 0; i < bind.Count; i++)
            {
                jg[i] = bind[i].Second;

                if (i != 0 && bind[i].First > bind[i - 1].First)
                {
                    ig[count++] = i;
                }
            }
            ig[xLength * yLength * zLength] = bind.Count;

            return new SlaeService(xLength * yLength * zLength, ggl, ig, jg);
        }

        private void AddLocal(ISlaeService slae, int number)
        {
            var (x1, y1, z1) = GetNodeCoordinates(number);
            var (x2, y2, z2) = GetOppositeNodeCoordinates(number);

            var Gx = GetLocalG(x1, x2);
            var Gy = GetLocalG(y1, y2);
            var Gz = GetLocalG(z1, z2);

            var Mx = GetLocalM(x1, x2);
            var My = GetLocalM(y1, y2);
            var Mz = GetLocalM(z1, z2);

            #region Матрицы жёсткости и массы

            var G = new double[8, 8];
            var M = new double[8, 8];

            G[0, 0] = SolutionParams.Lambda * (Gx[0, 0] * My[0, 0] * Mz[0, 0] +
                                               Mx[0, 0] * Gy[0, 0] * Mz[0, 0] +
                                               Mx[0, 0] * My[0, 0] * Gz[0, 0]);

            G[0, 1] = SolutionParams.Lambda * (Gx[0, 1] * My[0, 0] * Mz[0, 0] +
                                               Mx[0, 1] * Gy[0, 0] * Mz[0, 0] +
                                               Mx[0, 1] * My[0, 0] * Gz[0, 0]);

            G[0, 2] = SolutionParams.Lambda * (Gx[0, 0] * My[0, 1] * Mz[0, 0] +
                                               Mx[0, 0] * Gy[0, 1] * Mz[0, 0] +
                                               Mx[0, 0] * My[0, 1] * Gz[0, 0]);

            G[0, 3] = SolutionParams.Lambda * (Gx[0, 1] * My[0, 1] * Mz[0, 0] +
                                               Mx[0, 1] * Gy[0, 1] * Mz[0, 0] +
                                               Mx[0, 1] * My[0, 1] * Gz[0, 0]);

            G[0, 4] = SolutionParams.Lambda * (Gx[0, 0] * My[0, 0] * Mz[0, 1] +
                                               Mx[0, 0] * Gy[0, 0] * Mz[0, 1] +
                                               Mx[0, 0] * My[0, 0] * Gz[0, 1]);

            G[0, 5] = SolutionParams.Lambda * (Gx[0, 1] * My[0, 0] * Mz[0, 1] +
                                               Mx[0, 1] * Gy[0, 0] * Mz[0, 1] +
                                               Mx[0, 1] * My[0, 0] * Gz[0, 1]);

            G[0, 6] = SolutionParams.Lambda * (Gx[0, 0] * My[0, 1] * Mz[0, 1] +
                                               Mx[0, 0] * Gy[0, 1] * Mz[0, 1] +
                                               Mx[0, 0] * My[0, 1] * Gz[0, 1]);

            G[0, 7] = SolutionParams.Lambda * (Gx[0, 1] * My[0, 1] * Mz[0, 1] +
                                               Mx[0, 1] * Gy[0, 1] * Mz[0, 1] +
                                               Mx[0, 1] * My[0, 1] * Gz[0, 1]);

            G[1, 0] = G[0, 1];

            G[1, 1] = SolutionParams.Lambda * (Gx[1, 1] * My[0, 0] * Mz[0, 0] +
                                               Mx[1, 1] * Gy[0, 0] * Mz[0, 0] +
                                               Mx[1, 1] * My[0, 0] * Gz[0, 0]);

            G[1, 2] = SolutionParams.Lambda * (Gx[1, 0] * My[0, 1] * Mz[0, 0] +
                                               Mx[1, 0] * Gy[0, 1] * Mz[0, 0] +
                                               Mx[1, 0] * My[0, 1] * Gz[0, 0]);

            G[1, 3] = SolutionParams.Lambda * (Gx[1, 1] * My[0, 1] * Mz[0, 0] +
                                               Mx[1, 1] * Gy[0, 1] * Mz[0, 0] +
                                               Mx[1, 1] * My[0, 1] * Gz[0, 0]);

            G[1, 4] = SolutionParams.Lambda * (Gx[1, 0] * My[0, 0] * Mz[0, 1] +
                                               Mx[1, 0] * Gy[0, 0] * Mz[0, 1] +
                                               Mx[1, 0] * My[0, 0] * Gz[0, 1]);

            G[1, 5] = SolutionParams.Lambda * (Gx[1, 1] * My[0, 0] * Mz[0, 1] +
                                               Mx[1, 1] * Gy[0, 0] * Mz[0, 1] +
                                               Mx[1, 1] * My[0, 0] * Gz[0, 1]);

            G[1, 6] = SolutionParams.Lambda * (Gx[1, 0] * My[0, 1] * Mz[0, 1] +
                                               Mx[1, 0] * Gy[0, 1] * Mz[0, 1] +
                                               Mx[1, 0] * My[0, 1] * Gz[0, 1]);

            G[1, 7] = SolutionParams.Lambda * (Gx[1, 1] * My[0, 1] * Mz[0, 1] +
                                               Mx[1, 1] * Gy[0, 1] * Mz[0, 1] +
                                               Mx[1, 1] * My[0, 1] * Gz[0, 1]);

            G[2, 0] = G[0, 2];

            G[2, 1] = G[1, 2];

            G[2, 2] = SolutionParams.Lambda * (Gx[0, 0] * My[1, 1] * Mz[0, 0] +
                                               Mx[0, 0] * Gy[1, 1] * Mz[0, 0] +
                                               Mx[0, 0] * My[1, 1] * Gz[0, 0]);

            G[2, 3] = SolutionParams.Lambda * (Gx[0, 1] * My[1, 1] * Mz[0, 0] +
                                               Mx[0, 1] * Gy[1, 1] * Mz[0, 0] +
                                               Mx[0, 1] * My[1, 1] * Gz[0, 0]);

            G[2, 4] = SolutionParams.Lambda * (Gx[0, 0] * My[1, 0] * Mz[0, 1] +
                                               Mx[0, 0] * Gy[1, 0] * Mz[0, 1] +
                                               Mx[0, 0] * My[1, 0] * Gz[0, 1]);

            G[2, 5] = SolutionParams.Lambda * (Gx[0, 1] * My[1, 0] * Mz[0, 1] +
                                               Mx[0, 1] * Gy[1, 0] * Mz[0, 1] +
                                               Mx[0, 1] * My[1, 0] * Gz[0, 1]);

            G[2, 6] = SolutionParams.Lambda * (Gx[0, 0] * My[1, 1] * Mz[0, 1] +
                                               Mx[0, 0] * Gy[1, 1] * Mz[0, 1] +
                                               Mx[0, 0] * My[1, 1] * Gz[0, 1]);

            G[2, 7] = SolutionParams.Lambda * (Gx[0, 1] * My[1, 1] * Mz[0, 1] +
                                               Mx[0, 1] * Gy[1, 1] * Mz[0, 1] +
                                               Mx[0, 1] * My[1, 1] * Gz[0, 1]);

            G[3, 0] = G[0, 3];

            G[3, 1] = G[1, 3];

            G[3, 2] = G[2, 3];

            G[3, 3] = SolutionParams.Lambda * (Gx[1, 1] * My[1, 1] * Mz[0, 0] +
                                               Mx[1, 1] * Gy[1, 1] * Mz[0, 0] +
                                               Mx[1, 1] * My[1, 1] * Gz[0, 0]);

            G[3, 4] = SolutionParams.Lambda * (Gx[1, 0] * My[1, 0] * Mz[0, 1] +
                                               Mx[1, 0] * Gy[1, 0] * Mz[0, 1] +
                                               Mx[1, 0] * My[1, 0] * Gz[0, 1]);

            G[3, 5] = SolutionParams.Lambda * (Gx[1, 1] * My[1, 0] * Mz[0, 1] +
                                               Mx[1, 1] * Gy[1, 0] * Mz[0, 1] +
                                               Mx[1, 1] * My[1, 0] * Gz[0, 1]);

            G[3, 6] = SolutionParams.Lambda * (Gx[1, 0] * My[1, 1] * Mz[0, 1] +
                                               Mx[1, 0] * Gy[1, 1] * Mz[0, 1] +
                                               Mx[1, 0] * My[1, 1] * Gz[0, 1]);

            G[3, 7] = SolutionParams.Lambda * (Gx[1, 1] * My[1, 1] * Mz[0, 1] +
                                               Mx[1, 1] * Gy[1, 1] * Mz[0, 1] +
                                               Mx[1, 1] * My[1, 1] * Gz[0, 1]);

            G[4, 0] = G[0, 4];

            G[4, 1] = G[1, 4];

            G[4, 2] = G[2, 4];

            G[4, 3] = G[3, 4];

            G[4, 4] = SolutionParams.Lambda * (Gx[0, 0] * My[0, 0] * Mz[1, 1] +
                                               Mx[0, 0] * Gy[0, 0] * Mz[1, 1] +
                                               Mx[0, 0] * My[0, 0] * Gz[1, 1]);

            G[4, 5] = SolutionParams.Lambda * (Gx[0, 1] * My[0, 0] * Mz[1, 1] +
                                               Mx[0, 1] * Gy[0, 0] * Mz[1, 1] +
                                               Mx[0, 1] * My[0, 0] * Gz[1, 1]);

            G[4, 6] = SolutionParams.Lambda * (Gx[0, 0] * My[0, 1] * Mz[1, 1] +
                                               Mx[0, 0] * Gy[0, 1] * Mz[1, 1] +
                                               Mx[0, 0] * My[0, 1] * Gz[1, 1]);

            G[4, 7] = SolutionParams.Lambda * (Gx[0, 1] * My[0, 1] * Mz[1, 1] +
                                               Mx[0, 1] * Gy[0, 1] * Mz[1, 1] +
                                               Mx[0, 1] * My[0, 1] * Gz[1, 1]);

            G[5, 0] = G[0, 5];

            G[5, 1] = G[1, 5];

            G[5, 2] = G[2, 5];

            G[5, 3] = G[3, 5];

            G[5, 4] = G[4, 5];

            G[5, 5] = SolutionParams.Lambda * (Gx[1, 1] * My[0, 0] * Mz[1, 1] +
                                               Mx[1, 1] * Gy[0, 0] * Mz[1, 1] +
                                               Mx[1, 1] * My[0, 0] * Gz[1, 1]);

            G[5, 6] = SolutionParams.Lambda * (Gx[1, 0] * My[0, 1] * Mz[1, 1] +
                                               Mx[1, 0] * Gy[0, 1] * Mz[1, 1] +
                                               Mx[1, 0] * My[0, 1] * Gz[1, 1]);

            G[5, 7] = SolutionParams.Lambda * (Gx[1, 1] * My[0, 1] * Mz[1, 1] +
                                               Mx[1, 1] * Gy[0, 1] * Mz[1, 1] +
                                               Mx[1, 1] * My[0, 1] * Gz[1, 1]);

            G[6, 0] = G[0, 6];

            G[6, 1] = G[1, 6];

            G[6, 2] = G[2, 6];

            G[6, 3] = G[3, 6];

            G[6, 4] = G[4, 6];

            G[6, 5] = G[5, 6];

            G[6, 6] = SolutionParams.Lambda * (Gx[0, 0] * My[1, 1] * Mz[1, 1] +
                                               Mx[0, 0] * Gy[1, 1] * Mz[1, 1] +
                                               Mx[0, 0] * My[1, 1] * Gz[1, 1]);

            G[6, 7] = SolutionParams.Lambda * (Gx[0, 1] * My[1, 1] * Mz[1, 1] +
                                               Mx[0, 1] * Gy[1, 1] * Mz[1, 1] +
                                               Mx[0, 1] * My[1, 1] * Gz[1, 1]);

            G[7, 0] = G[0, 7];

            G[7, 1] = G[1, 7];

            G[7, 2] = G[2, 7];

            G[7, 3] = G[3, 7];

            G[7, 4] = G[4, 7];

            G[7, 5] = G[5, 7];

            G[7, 6] = G[6, 7];

            G[7, 7] = SolutionParams.Lambda * (Gx[1, 1] * My[1, 1] * Mz[1, 1] +
                                               Mx[1, 1] * Gy[1, 1] * Mz[1, 1] +
                                               Mx[1, 1] * My[1, 1] * Gz[1, 1]);

            M[0, 0] = SolutionParams.Gamma * Mx[0, 0] * My[0, 0] * Mz[0, 0];
            M[0, 1] = SolutionParams.Gamma * Mx[0, 1] * My[0, 0] * Mz[0, 0];
            M[0, 2] = SolutionParams.Gamma * Mx[0, 0] * My[0, 1] * Mz[0, 0];
            M[0, 3] = SolutionParams.Gamma * Mx[0, 1] * My[0, 1] * Mz[0, 0];
            M[0, 4] = SolutionParams.Gamma * Mx[0, 0] * My[0, 0] * Mz[0, 1];
            M[0, 5] = SolutionParams.Gamma * Mx[0, 1] * My[0, 0] * Mz[0, 1];
            M[0, 6] = SolutionParams.Gamma * Mx[0, 0] * My[0, 1] * Mz[0, 1];
            M[0, 7] = SolutionParams.Gamma * Mx[0, 1] * My[0, 1] * Mz[0, 1];

            M[1, 0] = M[0, 1];
            M[1, 1] = SolutionParams.Gamma * Mx[1, 0] * My[0, 0] * Mz[0, 0];
            M[1, 2] = SolutionParams.Gamma * Mx[1, 0] * My[0, 1] * Mz[0, 0];
            M[1, 3] = SolutionParams.Gamma * Mx[1, 1] * My[0, 1] * Mz[0, 0];
            M[1, 4] = SolutionParams.Gamma * Mx[1, 0] * My[0, 0] * Mz[0, 1];
            M[1, 5] = SolutionParams.Gamma * Mx[1, 1] * My[0, 0] * Mz[0, 1];
            M[1, 6] = SolutionParams.Gamma * Mx[1, 0] * My[0, 1] * Mz[0, 1];
            M[1, 7] = SolutionParams.Gamma * Mx[1, 1] * My[0, 1] * Mz[0, 1];

            M[2, 0] = M[0, 1];
            M[2, 1] = M[1, 2];
            M[2, 2] = SolutionParams.Gamma * Mx[0, 0] * My[1, 1] * Mz[0, 0];
            M[2, 3] = SolutionParams.Gamma * Mx[0, 1] * My[1, 1] * Mz[0, 0];
            M[2, 4] = SolutionParams.Gamma * Mx[0, 0] * My[1, 0] * Mz[0, 1];
            M[2, 5] = SolutionParams.Gamma * Mx[0, 1] * My[1, 0] * Mz[0, 1];
            M[2, 6] = SolutionParams.Gamma * Mx[0, 0] * My[1, 1] * Mz[0, 1];
            M[2, 7] = SolutionParams.Gamma * Mx[0, 1] * My[1, 1] * Mz[0, 1];

            M[3, 0] = M[0, 3];
            M[3, 1] = M[1, 3];
            M[3, 2] = M[2, 3];
            M[3, 3] = SolutionParams.Gamma * Mx[1, 1] * My[1, 1] * Mz[0, 0];
            M[3, 4] = SolutionParams.Gamma * Mx[1, 0] * My[1, 0] * Mz[0, 1];
            M[3, 5] = SolutionParams.Gamma * Mx[1, 1] * My[1, 0] * Mz[0, 1];
            M[3, 6] = SolutionParams.Gamma * Mx[1, 0] * My[1, 1] * Mz[0, 1];
            M[3, 7] = SolutionParams.Gamma * Mx[1, 1] * My[1, 1] * Mz[0, 1];

            M[4, 0] = M[0, 4];
            M[4, 1] = M[1, 4];
            M[4, 2] = M[2, 4];
            M[4, 3] = M[3, 4];
            M[4, 4] = SolutionParams.Gamma * Mx[0, 0] * My[0, 0] * Mz[1, 1];
            M[4, 5] = SolutionParams.Gamma * Mx[0, 1] * My[0, 0] * Mz[1, 1];
            M[4, 6] = SolutionParams.Gamma * Mx[0, 0] * My[0, 1] * Mz[1, 1];
            M[4, 7] = SolutionParams.Gamma * Mx[0, 1] * My[0, 1] * Mz[1, 1];

            M[5, 0] = M[0, 5];
            M[5, 1] = M[1, 5];
            M[5, 2] = M[2, 5];
            M[5, 3] = M[3, 5];
            M[5, 4] = M[4, 5];
            M[5, 5] = SolutionParams.Gamma * Mx[1, 1] * My[0, 0] * Mz[1, 1];
            M[5, 6] = SolutionParams.Gamma * Mx[1, 0] * My[0, 1] * Mz[1, 1];
            M[5, 7] = SolutionParams.Gamma * Mx[1, 1] * My[0, 1] * Mz[1, 1];

            M[6, 0] = M[0, 6];
            M[6, 1] = M[1, 6];
            M[6, 2] = M[2, 6];
            M[6, 3] = M[3, 6];
            M[6, 4] = M[4, 6];
            M[6, 5] = M[5, 6];
            M[6, 6] = SolutionParams.Gamma * Mx[0, 0] * My[1, 1] * Mz[1, 1];
            M[6, 7] = SolutionParams.Gamma * Mx[0, 1] * My[1, 1] * Mz[1, 1];

            M[7, 0] = M[0, 7];
            M[7, 1] = M[1, 7];
            M[7, 2] = M[2, 7];
            M[7, 3] = M[3, 7];
            M[7, 4] = M[4, 7];
            M[7, 5] = M[5, 7];
            M[7, 6] = M[6, 7];
            M[7, 7] = SolutionParams.Gamma * Mx[1, 1] * My[1, 1] * Mz[1, 1];

            #endregion

            #region Добавление в матрицу А

            var nodesNumbers = GetNodesNumbers(number);
            var n = nodesNumbers.Length;
            for (var i = 0; i < n; i++)
            {
                var row = nodesNumbers[i];
                for (var j = 0; j < n; j++)
                {
                    var column = nodesNumbers[j];
                    slae.GetElementOfA(row, column) += G[i, j] + M[i, j];
                }
            }

            #endregion

            #region Добавление в вектор правой части

            for (var j = 0; j < n; j++)
            {
                var row = nodesNumbers[j];
                var (x, y, z) = GetNodeCoordinates(row);
                var fValue = f(x, y, z);

                for (var i = 0; i < n; i++)
                {
                    slae.b[nodesNumbers[i]] += M[i, j] * fValue;
                }
            }

            #endregion
        }

        private int[] GetNodesNumbers(int startNumber)
        {
            var xLength = SolutionParams.x.Length;
            var yLength = SolutionParams.y.Length;

            return new[]
            {
                startNumber,
                startNumber + 1,
                startNumber + xLength,
                startNumber + xLength + 1,
                startNumber + xLength * yLength,
                startNumber + xLength * yLength + 1,
                startNumber + xLength * yLength + xLength,
                startNumber + xLength * yLength + xLength + 1
            };
        }

        private double f(double x, double y, double z)
        {
            var gamma = SolutionParams.Gamma;

            //1-st case
            //return -gamma / r * (1 + z) + gamma * U(r, z);

            //2-nd case
            return gamma * z;

            //3-rd case
            //return gamma * (x + y + z);

            //4-th case
            //return gamma * x * y * z;
        }

        private double U(double x, double y, double z)
        {
            //1-st case
            //return x + y + z + x * y + x * z + y * z + x * y * z + 1;

            //2-nd case
            return z;

            //3-rd case
            //return x * y * z;

            //4-th case
            //return gamma * (x + y + z);
        }

        private double teta(double x, double y, double z, EEdge edge)
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

        private double Ubeta(double x, double y, double z, EEdge edge) => U(x, y, z) + teta(x, y, z, edge) / SolutionParams.Beta;

        private void kuslau1(ISlaeService slae, int node)
        {
            var n = SolutionParams.N;

            var (x1, y1, z1) = GetNodeCoordinates(node);

            for (int i = 0; i < n; i++)
            {
                slae.GetElementOfA(node, i) = i == node ? 1 : 0;
            }
            slae.b[node] = U(x1, y1, z1);
        }

        private void kuslau2(ISlaeService slae, (int, int, int, int) nodes, EEdge edge)
        {
            var (x1, y1, z1) = GetNodeCoordinates(nodes.Item1);
            var (x2, y2, z2) = GetNodeCoordinates(nodes.Item2);
            var (x3, y3, z3) = GetNodeCoordinates(nodes.Item3);
            var (x4, y4, z4) = GetNodeCoordinates(nodes.Item4);

            var teta1 = teta(x1, y1, z1, edge);
            var teta2 = teta(x2, y2, z2, edge);
            var teta3 = teta(x3, y3, z3, edge);
            var teta4 = teta(x4, y4, z4, edge);

            double h1 = 0;
            double h2 = 0;

            switch (edge)
            {
                case EEdge.Right:
                case EEdge.Left:
                    h1 = y2 - y1;
                    h2 = z3 - z1;
                    break;
                case EEdge.Back:
                case EEdge.Front:
                    h1 = x2 - x1;
                    h2 = z3 - z1;
                    break;
                case EEdge.Bottom:
                case EEdge.Top:
                    h1 = x2 - x1;
                    h2 = y3 - y1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }

            if (h1 == 0 || h2 == 0)
            {
                throw new Exception();
            }

            var Mz = GetLocalM(z1, z2);
            slae.b[nodes.Item1] += h1 * (M2x[0, 0] * teta1 + M2x[0, 1] * teta2 + M2x[0, 2] * teta3 + M2x[0, 3] * teta4);
            slae.b[nodes.Item2] += h1 * (M2x[1, 0] * teta1 + M2x[1, 1] * teta2 + M2x[1, 2] * teta3 + M2x[1, 3] * teta4);
            slae.b[nodes.Item3] += h1 * (M2x[2, 0] * teta1 + M2x[2, 1] * teta2 + M2x[2, 2] * teta3 + M2x[2, 3] * teta4);
            slae.b[nodes.Item4] += h1 * (M2x[3, 0] * teta1 + M2x[3, 1] * teta2 + M2x[3, 2] * teta3 + M2x[3, 3] * teta4);
        }

        private void kuslau3(ISlaeService slae, (int, int, int, int) nodes, EEdge edge)
        {
            var (x1, y1, z1) = GetNodeCoordinates(nodes.Item1);
            var (x2, y2, z2) = GetNodeCoordinates(nodes.Item2);
            var (x3, y3, z3) = GetNodeCoordinates(nodes.Item3);
            var (x4, y4, z4) = GetNodeCoordinates(nodes.Item4);

            var ubeta1 = Ubeta(x1, y1, z1, edge);
            var ubeta2 = Ubeta(x2, y2, z2, edge);
            var ubeta3 = Ubeta(x3, y3, z3, edge);
            var ubeta4 = Ubeta(x4, y4, z4, edge);

            double h1;
            double h2;

            switch (edge)
            {
                case EEdge.Right:
                case EEdge.Left:
                    h1 = y2 - y1;
                    h2 = z3 - z1;
                    break;
                case EEdge.Back:
                case EEdge.Front:
                    h1 = x2 - x1;
                    h2 = z3 - z1;
                    break;
                case EEdge.Bottom:
                case EEdge.Top:
                    h1 = x2 - x1;
                    h2 = y3 - y1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }

            if (h1 == 0 || h2 == 0)
            {
                throw new Exception();
            }

            slae.GetElementOfA(nodes.Item1, nodes.Item1) += SolutionParams.Beta * M2x[0, 0] * h1 * h2;
            slae.GetElementOfA(nodes.Item1, nodes.Item2) += SolutionParams.Beta * M2x[0, 1] * h1 * h2;
            slae.GetElementOfA(nodes.Item1, nodes.Item3) += SolutionParams.Beta * M2x[0, 2] * h1 * h2;
            slae.GetElementOfA(nodes.Item1, nodes.Item4) += SolutionParams.Beta * M2x[0, 3] * h1 * h2;
            slae.GetElementOfA(nodes.Item2, nodes.Item1) += SolutionParams.Beta * M2x[1, 0] * h1 * h2;
            slae.GetElementOfA(nodes.Item2, nodes.Item2) += SolutionParams.Beta * M2x[1, 1] * h1 * h2;
            slae.GetElementOfA(nodes.Item2, nodes.Item3) += SolutionParams.Beta * M2x[1, 2] * h1 * h2;
            slae.GetElementOfA(nodes.Item2, nodes.Item4) += SolutionParams.Beta * M2x[1, 3] * h1 * h2;
            slae.GetElementOfA(nodes.Item3, nodes.Item1) += SolutionParams.Beta * M2x[2, 0] * h1 * h2;
            slae.GetElementOfA(nodes.Item3, nodes.Item2) += SolutionParams.Beta * M2x[2, 1] * h1 * h2;
            slae.GetElementOfA(nodes.Item3, nodes.Item3) += SolutionParams.Beta * M2x[2, 2] * h1 * h2;
            slae.GetElementOfA(nodes.Item3, nodes.Item4) += SolutionParams.Beta * M2x[2, 3] * h1 * h2;
            slae.GetElementOfA(nodes.Item4, nodes.Item1) += SolutionParams.Beta * M2x[3, 0] * h1 * h2;
            slae.GetElementOfA(nodes.Item4, nodes.Item2) += SolutionParams.Beta * M2x[3, 1] * h1 * h2;
            slae.GetElementOfA(nodes.Item4, nodes.Item3) += SolutionParams.Beta * M2x[3, 2] * h1 * h2;
            slae.GetElementOfA(nodes.Item4, nodes.Item4) += SolutionParams.Beta * M2x[3, 3] * h1 * h2;

            slae.b[nodes.Item1] += h1 * (M2x[0, 0] * ubeta1 + M2x[0, 1] * ubeta2 + M2x[0, 2] * ubeta3 + M2x[0, 3] * ubeta4);
            slae.b[nodes.Item2] += h1 * (M2x[1, 0] * ubeta1 + M2x[1, 1] * ubeta2 + M2x[1, 2] * ubeta3 + M2x[1, 3] * ubeta4);
            slae.b[nodes.Item3] += h1 * (M2x[2, 0] * ubeta1 + M2x[2, 1] * ubeta2 + M2x[2, 2] * ubeta3 + M2x[2, 3] * ubeta4);
            slae.b[nodes.Item4] += h1 * (M2x[3, 0] * ubeta1 + M2x[3, 1] * ubeta2 + M2x[3, 2] * ubeta3 + M2x[3, 3] * ubeta4);
        }

        private double[,] GetLocalM(double x1, double x2)
        {
            var h = x2 - x1;

            return new[,]
            {
                { h / 3, h / 6 },
                { h / 6, h / 3 }
            };
        }

        private double[,] GetLocalG(double x1, double x2)
        {
            var h = x2 - x1;

            return new[,]
            {
                { 1 / h, -1 / h },
                { -1 / h, 1 / h }
            };
        }

        private int[,] M2x = new[,]
        {
            { 4, 2, 2, 1 },
            { 2, 4, 1, 2 },
            { 2, 1, 4, 2 },
            { 1, 2, 2, 4 }
        };

        private (double x, double y, double z) GetNodeCoordinates(int node)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            var x1 = x[node % x.Length];
            var y1 = y[node % (x.Length * y.Length) / x.Length];
            var z1 = z[node / (x.Length * y.Length)];

            return (x1, y1, z1);
        }

        private (double x, double y, double z) GetOppositeNodeCoordinates(int node)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            var x1 = x[node % x.Length + 1];
            var y1 = y[node % (x.Length * y.Length) / x.Length + 1];
            var z1 = z[node / (x.Length * y.Length) + 1];

            return (x1, y1, z1);
        }
    }
}
