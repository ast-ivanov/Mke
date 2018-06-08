namespace Mke.Xyz.PointSource
{
    using Mke;
    using Interfaces;
    using Services;

    using System.Collections.Generic;
    using System;
    using Helpers;

    /// <inheritdoc />
    internal sealed class Solution : ISolution<PointSolutionParams>
    {
        public PointSolutionParams SolutionParams { get; set; }

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

            int xN = x.Length / 2;
            int yN = y.Length / 2;
            int zN = 1;

            #region Краевые

            //нижняя граница
            CalculateKU(EEdge.Bottom, slae);

            //левая граница
            CalculateKU(EEdge.Left, slae);

            //правая граница
            CalculateKU(EEdge.Right, slae);

            //задняя граница
            CalculateKU(EEdge.Back, slae);

            //передняя граница
            CalculateKU(EEdge.Front, slae);

            //верхняя граница
            CalculateKU(EEdge.Top, slae);

            #endregion

            var node = zN * x.Length * y.Length + yN * x.Length + xN;
            slae.b[node] += SolutionParams.Ro;

            #region Решение СЛАУ

            slae.CalculateLOS(200, 1e-20, out var iterationCount, out var discrepancy);
//            slae.CalculateMSG(200, 1e-20, out var iterationCount, out var discrepancy);

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

        private void CalculateKU(EEdge edge, ISlaeService slae)
        {
            var startNode = GetStartNode(edge);
            var endNode = GetEndNode(edge);

            bool isFirst;

            switch (edge)
            {
                case EEdge.Bottom:
                    isFirst = SolutionParams.BottomFirst;
                    break;
                case EEdge.Top:
                    isFirst = SolutionParams.TopFirst;
                    break;
                case EEdge.Left:
                    isFirst = SolutionParams.LeftFirst;
                    break;
                case EEdge.Right:
                    isFirst = SolutionParams.RightFirst;
                    break;
                case EEdge.Front:
                    isFirst = SolutionParams.FrontFirst;
                    break;
                case EEdge.Back:
                    isFirst = SolutionParams.BackFirst;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }

            if (!isFirst)
            {
                return;
            }

            for (var i = startNode; i <= endNode; i = GetNextNode(i, edge))
            {
                kuslau1(slae, i);
            }
        }

        private int GetStartNode(EEdge edge)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            switch (edge)
            {
                case EEdge.Bottom:
                    return 0;
                case EEdge.Top:
                    return x.Length * y.Length * z.Length - x.Length * y.Length;
                case EEdge.Left:
                    return 0;
                case EEdge.Right:
                    return x.Length - 1;
                case EEdge.Front:
                    return 0;
                case EEdge.Back:
                    return x.Length * y.Length - x.Length;
                default:
                    throw new ArgumentException();
            }
        }

        private int GetEndNode(EEdge edge)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            switch (edge)
            {
                case EEdge.Bottom:
                    return x.Length * y.Length - 1;
                case EEdge.Top:
                    return x.Length * y.Length * z.Length - 1;
                case EEdge.Left:
                    return x.Length * y.Length * z.Length - x.Length;
                case EEdge.Right:
                    return x.Length * y.Length * z.Length - 1;
                case EEdge.Front:
                    return x.Length * y.Length * z.Length - x.Length * y.Length + x.Length - 1;
                case EEdge.Back:
                    return x.Length * y.Length * z.Length - 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }
        }

        private int GetNextNode(int node, EEdge edge)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;

            switch (edge)
            {
                case EEdge.Bottom:
                case EEdge.Top:
                    return ++node;
                case EEdge.Left:
                case EEdge.Right:
                    return node + x.Length;
                case EEdge.Front:
                case EEdge.Back:
                    if (++node % x.Length == 0)
                    {
                        return node + x.Length * y.Length - x.Length;
                    }

                    return node;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }
        }

        private ISlaeService GeneratePortrait()
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

            var n = xLength * yLength * zLength;

            var pairs = new SortedSet<Pair>(
                new PairComparer
                {
                    N = n
                });

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
            var ggl = new double[bind.Count];
            var jg = new int[bind.Count];
            var ig = new int[n + 1];

            var count = 2;
            for (var i = 0; i < bind.Count; i++)
            {
                jg[i] = bind[i].Second;

                if (i != 0 && bind[i].First > bind[i - 1].First)
                {
                    ig[count++] = i;
                }
            }
            ig[n] = bind.Count;

            return new SlaeService(n, ggl, ig, jg);
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

            #region Формирование матрицы жёсткости и массы

            const int nodesCount = 8;
            var G = new double[nodesCount, nodesCount];
            var M = new double[nodesCount, nodesCount];

            for (var i = 0; i < nodesCount; i++)
            {
                var iBinaryArray = ToBinaryConverter.ConvertToBinary(i, 3);
                var xi = iBinaryArray[0];
                var yi = iBinaryArray[1];
                var zi = iBinaryArray[2];

                for (var j = i; j < nodesCount; j++)
                {
                    var jBinaryArray = ToBinaryConverter.ConvertToBinary(j, 3);
                    var xj = jBinaryArray[0];
                    var yj = jBinaryArray[1];
                    var zj = jBinaryArray[2];

                    G[i, j] = SolutionParams.Lambda * (Gx[xi, xj] * My[yi, yj] * Mz[zi, zj] +
                                                       Mx[xi, xj] * Gy[yi, yj] * Mz[zi, zj] +
                                                       Mx[xi, xj] * My[yi, yj] * Gz[zi, zj]);

                    M[i, j] = SolutionParams.Gamma * Mx[xi, xj] * My[yi, yj] * Mz[zi, zj];
                }

                for (var j = 0; j < i; j++)
                {
                    G[i, j] = G[j, i];
                    M[i, j] = M[j, i];
                }
            }

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
            var lambda = SolutionParams.Lambda;

            switch (SolutionParams.FunctionNumber)
            {
                case 1:
                    return 0;
                case 2:
                    return gamma * x * y * z;
                case 3:
                    return gamma * (x + y + z);
                case 4:
                    return -4 * lambda + gamma * (Math.Pow(x, 2) + Math.Pow(y, 2));
                default:
                    throw new ArgumentException("Неверно задан номер выражения");
            }
        }

        private double U(double x, double y, double z)
        {
            switch (SolutionParams.FunctionNumber)
            {
                case 1:
                    return 0;
                case 2:
                    return x * y * z;
                case 3:
                    return x + y + z;
                case 4:
                    return Math.Pow(x, 2) + Math.Pow(y, 2);
                default:
                    throw new ArgumentException("Неверно задан номер выражения");
            }
        }

        private void kuslau1(ISlaeService slae, int node)
        {
            var n = SolutionParams.N;

            for (var i = 0; i < n; i++)
            {
                slae.GetElementOfA(node, i) = i == node ? 1 : 0;
                slae.GetElementOfA(i, node) = i == node ? 1 : 0;
            }

            slae.b[node] = 10;
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
