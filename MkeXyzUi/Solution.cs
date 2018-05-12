namespace MkeXyzUi
{
    using Mke;
    using Mke.Interfaces;
    using Mke.Services;
    using Mke.Extensions;

    using System.Collections.Generic;
    using System;
    using Mke.Helpers;

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
            CalculateKU(EEdge.Bottom, slae);

            //верхняя граница
            CalculateKU(EEdge.Top, slae);

            //левая граница
            CalculateKU(EEdge.Left, slae);

            //правая граница
            CalculateKU(EEdge.Right, slae);

            //задняя граница
            CalculateKU(EEdge.Back, slae);

            //передняя граница
            CalculateKU(EEdge.Front, slae);

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

        private void CalculateKU(EEdge edge, ISlaeService slae)
        {
            var startNode = GetStartNode(edge);
            var endNode = GetEndNode(edge);

            bool isFirst;
            bool isSecond;
            bool isThird;

            switch (edge)
            {
                case EEdge.Bottom:
                    isFirst = SolutionParams.BottomFirst;
                    isSecond = SolutionParams.BottomSecond;
                    isThird = SolutionParams.BottomThird;
                    break;
                case EEdge.Top:
                    isFirst = SolutionParams.TopFirst;
                    isSecond = SolutionParams.TopSecond;
                    isThird = SolutionParams.TopThird;
                    break;
                case EEdge.Left:
                    isFirst = SolutionParams.LeftFirst;
                    isSecond = SolutionParams.LeftSecond;
                    isThird = SolutionParams.LeftThird;
                    break;
                case EEdge.Right:
                    isFirst = SolutionParams.RightFirst;
                    isSecond = SolutionParams.RightSecond;
                    isThird = SolutionParams.RightThird;
                    break;
                case EEdge.Front:
                    isFirst = SolutionParams.FrontFirst;
                    isSecond = SolutionParams.FrontSecond;
                    isThird = SolutionParams.FrontThird;
                    break;
                case EEdge.Back:
                    isFirst = SolutionParams.BackFirst;
                    isSecond = SolutionParams.BackSecond;
                    isThird = SolutionParams.BackThird;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }

            if (!isFirst && !isSecond && !isThird)
            {
                return;
            }

            for (var i = startNode; i <= endNode; i = GetNextNode(i, edge))
            {
                if (IsNodeForNative(i, edge))
                {
                    var nodes = GetElementNodes(i, edge);

                    if (isSecond)
                    {
                        kuslau2(slae, nodes, edge);
                    }

                    if (isThird)
                    {
                        kuslau3(slae, nodes, edge);
                    }
                }

                if (isFirst)
                {
                    kuslau1(slae, i);
                }
            }
        }

        private bool IsNodeForNative(int node, EEdge edge)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;
            var z = SolutionParams.z;

            switch (edge)
            {
                case EEdge.Bottom:
                    if (node >= x.Length * y.Length - x.Length)
                    {
                        return false;
                    }
                    if ((node + 1) % x.Length == 0)
                    {
                        return false;
                    }
                    break;
                case EEdge.Top:
                    if (node >= x.Length * y.Length * z.Length - x.Length)
                    {
                        return false;
                    }
                    if ((node + 1) % x.Length == 0)
                    {
                        return false;
                    }
                    break;
                case EEdge.Left:
                    if (node % x.Length == 0 && node >= x.Length * y.Length * (z.Length - 1))
                    {
                        return false;
                    }
                    if ((node + x.Length) % (x.Length * y.Length) == 0)
                    {
                        return false;
                    }
                    break;
                case EEdge.Right:
                    if ((node + 1) % x.Length == 0 && node >= x.Length * y.Length * (z.Length - 1))
                    {
                        return false;
                    }
                    if ((node + 1) % (x.Length * y.Length) == 0)
                    {
                        return false;
                    }
                    break;
                case EEdge.Front:
                    if (node >= x.Length * y.Length * (z.Length - 1))
                    {
                        return false;
                    }
                    if ((node + x.Length * y.Length - x.Length + 1) % (x.Length * y.Length) == 0)
                    {
                        return false;
                    }
                    break;
                case EEdge.Back:
                    if (node >= x.Length * y.Length * z.Length - x.Length)
                    {
                        return false;
                    }
                    if ((node + 1) % (x.Length * y.Length) == 0)
                    {
                        return false;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
            }

            return true;
        }

        private (int, int, int, int) GetElementNodes(int startNode, EEdge edge)
        {
            var x = SolutionParams.x;
            var y = SolutionParams.y;

            switch (edge)
            {
                case EEdge.Bottom:
                case EEdge.Top:
                    return (startNode, startNode + 1, startNode + x.Length, startNode + x.Length + 1);
                case EEdge.Left:
                case EEdge.Right:
                    return (startNode, startNode + x.Length, startNode + x.Length * y.Length, startNode + x.Length * y.Length + x.Length);
                case EEdge.Front:
                case EEdge.Back:
                    return (startNode, startNode + 1, startNode + x.Length * y.Length, startNode + x.Length * y.Length + 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
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
                    if (++node % (x.Length * y.Length) == 0)
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

            for (var i = 0; i < n; i++)
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
