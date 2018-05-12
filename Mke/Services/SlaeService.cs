namespace Mke.Services
{
    using System;
    using Helpers;
    using Interfaces;

    /// <inheritdoc />
    public class SlaeService : ISlaeService
    {
        private double[] ggl, ggu, di;
        private int[] ig, jg;
        private double zero;

        public int N { get; }
        public double[] b { get; }
        public double[] q { get; }

        public SlaeService(int n, double[] ggl, int[] ig, int[] jg)
        {
            this.ggl = ggl;
            this.ggu = new double[ggl.Length];
            this.di = new double[n];
            this.b = new double[n];
            this.q = new double[n];
            this.ig = ig;
            this.jg = jg;
            this.N = n;
        }

        public void CalculateGauss()
        {
            int i, j, k;
            double sum;
            // int N = matrix.GetLength(0);
            var RightPart = new double[N];
            for (int ii = 0; ii < N; ii++)
                RightPart[ii] = b[ii];

            double t;
            // double[] q = new double[n];

            double max, d;
            double str;
            int m, imax = 0;
            for (k = 0; k < N - 1; k++)
            {
                max = 0;
                for (m = k; m < N; m++)
                {
                    if (Math.Abs(GetElementOfA(m, k)) > Math.Abs(max))
                    {
                        max = GetElementOfA(m, k);
                        imax = m;
                    }
                }
                if (max == 0) throw new DivideByZeroException();

                for (i = 0; i < N; i++)
                {
                    str = GetElementOfA(k, i);
                    GetElementOfA(k, i) = GetElementOfA(imax, i);
                    GetElementOfA(imax, i) = str;
                }
                d = RightPart[k]; RightPart[k] = RightPart[imax]; RightPart[imax] = d;

                for (i = k + 1; i < N; i++)
                {
                    t = GetElementOfA(i, k) / GetElementOfA(k, k);
                    RightPart[i] = RightPart[i] - t * RightPart[k];
                    for (j = k + 1; j < N; j++)
                    {
                        GetElementOfA(i, j) = GetElementOfA(i, j) - t * GetElementOfA(k, j);
                    }
                }

            }

            q[N - 1] = RightPart[N - 1] / GetElementOfA(N - 1, N - 1);
            for (k = N - 2; k >= 0; k--)
            {
                sum = 0;
                for (j = k + 1; j < N; j++) sum += GetElementOfA(k, j) * q[j];
                q[k] = (RightPart[k] - sum) / GetElementOfA(k, k);
            }
        }

        public void CalculateLU(bool withFactorization)
        {
            var L = new double[N, N];
            var U = new double[N, N];

            if (withFactorization)
            {
                LU_Factorization(L, U);
            }

            var y = new double[N];

            //прямой ход
            for (var i = 0; i < N; i++)
            {
                double sum = 0;
                for (var k = 0; k < i; k++)
                {
                    sum += L[i, k] * y[k];
                }
                y[i] = b[i] - sum;
            }

            //обратный ход
            for (var i = N - 1; i >= 0; i--)
            {
                double sum = 0;
                for (var k = i + 1; k < N; k++)
                {
                    sum += U[i, k] * q[k];
                }
                q[i] = (y[i] - sum) / U[i, i];
            }
        }

        public void CalculateLOS(int maxiter, double eps, out int iterationCount, out double discrepancy)
        {
            double alpha;
            double beta;
            iterationCount = 0;

            //начальное решение
            for (var i = 0; i < N; i++)
            {
                q[i] = 0;
            }
            var r = MathOperations.MatrixMult(GetElementOfA, N, q);

            for (var i = 0; i < N; i++)
            {
                r[i] = b[i] - r[i];
            }

            var z = new double[N];
            r.CopyTo(z, 0);
            var p = MathOperations.MatrixMult(GetElementOfA, N, z);

            do
            {
                ++iterationCount;
                alpha = MathOperations.ScalarMult(p, r) / MathOperations.ScalarMult(p, p);
                // nev = MathOperations.ScalarMult(r, r) - alpha * alpha * MathOperations.ScalarMult(p, p);
                for (var i = 0; i < N; i++)
                {
                    q[i] = q[i] + alpha * z[i];
                    r[i] = r[i] - alpha * p[i];
                }
                var Ar = MathOperations.MatrixMult(GetElementOfA, N, r);
                beta = - MathOperations.ScalarMult(p, Ar) / MathOperations.ScalarMult(p, p);

                for (var i = 0; i < N; i++)
                {
                    z[i] = r[i] + beta * z[i];
                    p[i] = Ar[i] + beta * p[i];
                }

                discrepancy = MathOperations.ScalarMult(r, r);
                Console.WriteLine(discrepancy);
            }
            while (iterationCount < maxiter && discrepancy > eps);
        }

        public ref double GetElementOfA(int i, int j)
        {
            if (i == j)
                return ref di[i];
            var gguflag = false;
            if (i < j)
            {
                MathOperations.Swap(ref i, ref j);
                gguflag = true;
            }
            j--;

            for (var k = ig[i]; k < ig[i + 1]; k++)
            {
                if (jg[k] == j + 1)
                {
                    if (gguflag)
                        return ref ggu[k];
                    return ref ggl[k];
                }
            }

            return ref zero;
        }

        public double[] test_vector(double[] vector)
        {
            return MathOperations.MatrixMult(GetElementOfA, N, vector);
        }

        private void LU_Factorization(double[,] L, double[,] U)
        {
            for (int i = 0; i < N; ++i)
            {
                for (int j = i; j < N; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += L[i, k] * U[k, j];
                    }
                    U[i, j] = GetElementOfA(i, j) - sum;
                }

                for (int j = i + 1; j < N; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += L[i, k] * U[k, j];
                    }
                    L[j, i] = (GetElementOfA(j, i) - sum) / U[i, i];
                }

                L[i, i] = 1;
            }
        }
    }
}
