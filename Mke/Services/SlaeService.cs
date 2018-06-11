namespace Mke.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Helpers;
    using Interfaces;

    /// <inheritdoc />
    public class SlaeService : ISlaeService
    {
        public int N { get; }

        public double[,] A { get; }

        public double[] b { get; }

        public double[] q { get; }

        public SlaeService(int n)
        {
            A = new double[n, n];
            b = new double[n];
            q = new double[n];
            N = n;
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
                    if (Math.Abs(A[m, k]) > Math.Abs(max))
                    {
                        max = A[m, k];
                        imax = m;
                    }
                }
                if (max == 0) throw new DivideByZeroException();

                for (i = 0; i < N; i++)
                {
                    str = A[k, i];
                    A[k, i] = A[imax, i];
                    A[imax, i] = str;
                }
                d = RightPart[k]; RightPart[k] = RightPart[imax]; RightPart[imax] = d;

                for (i = k + 1; i < N; i++)
                {
                    t = A[i, k] / A[k, k];
                    RightPart[i] = RightPart[i] - t * RightPart[k];
                    for (j = k + 1; j < N; j++)
                    {
                        A[i, j] = A[i, j] - t * A[k, j];
                    }
                }

            }

            q[N - 1] = RightPart[N - 1] / A[N - 1, N - 1];
            for (k = N - 2; k >= 0; k--)
            {
                sum = 0;
                for (j = k + 1; j < N; j++) sum += A[k, j] * q[j];
                q[k] = (RightPart[k] - sum) / A[k, k];
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

        public void CalculateMSG(int maxiter, double eps)
        {
            double alpha;
            double beta;
            double discrepancy;
            var iterationCount = 0;

            //начальное решение
            for (var i = 0; i < N; i++)
            {
                q[i] = 0;
            }
            var r = MathOperations.MatrixMult(N, A, q);

            for (var i = 0; i < N; i++)
            {
                r[i] = b[i] - r[i];
            }

            var z = new double[N];
            r.CopyTo(z, 0);

            do
            {
                ++iterationCount;
                var Az = MathOperations.MatrixMult(N, A, z);
                alpha = MathOperations.ScalarMult(r, r) / MathOperations.ScalarMult(Az, z);
                var temp = MathOperations.ScalarMult(r, r);
                Parallel.For(0, N, i =>
                {
                    q[i] = q[i] + alpha * z[i];
                    r[i] = r[i] - alpha * Az[i];
                });
                beta = MathOperations.ScalarMult(r, r) / temp;
                Parallel.For(0, N, i =>
                {
                    z[i] = r[i] + beta * z[i];
                });

                discrepancy = MathOperations.ScalarMult(r, r);
            }
            while (iterationCount < maxiter && discrepancy > eps);

            using (var sw = new StreamWriter("msg_log.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLine($"Погрешность: {discrepancy}\nЧисло итераций: {iterationCount}");
            }
        }

        public void CalculateLOS(int maxiter, double eps)
        {
            double alpha;
            double beta;
            double discrepancy;
            var iterationCount = 0;

            //начальное решение
            for (var i = 0; i < N; i++)
            {
                q[i] = 0;
            }
            var r = MathOperations.MatrixMult(N, A, q);

            for (var i = 0; i < N; i++)
            {
                r[i] = b[i] - r[i];
            }

            var z = new double[N];
            r.CopyTo(z, 0);
            var p = MathOperations.MatrixMult(N, A, z);

            do
            {
                ++iterationCount;
                alpha = MathOperations.ScalarMult(p, r) / MathOperations.ScalarMult(p, p);
                // nev = MathOperations.ScalarMult(r, r) - alpha * alpha * MathOperations.ScalarMult(p, p);
                Parallel.For(0, N, i =>
                {
                    q[i] = q[i] + alpha * z[i];
                    r[i] = r[i] - alpha * p[i];
                });

                var Ar = MathOperations.MatrixMult(N, A, r);
                beta = - MathOperations.ScalarMult(p, Ar) / MathOperations.ScalarMult(p, p);

                Parallel.For(0, N, i =>
                {
                    z[i] = r[i] + beta * z[i];
                    p[i] = Ar[i] + beta * p[i];
                });

                discrepancy = MathOperations.ScalarMult(r, r);
            }
            while (iterationCount < maxiter && discrepancy > eps);

            using (var sw = new StreamWriter("los_log.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLine($"Погрешность: {discrepancy}\nЧисло итераций: {iterationCount}");
            }
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
                    U[i, j] = A[i, j] - sum;
                }

                for (int j = i + 1; j < N; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += L[i, k] * U[k, j];
                    }
                    L[j, i] = (A[j, i] - sum) / U[i, i];
                }

                L[i, i] = 1;
            }
        }
    }
}
