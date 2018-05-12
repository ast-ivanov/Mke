namespace Mke.Helpers
{
    using System;

    public static class MathOperations
    {
        /// <summary>Поменять местами значения двух целых чисел</summary>
        public static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        /// <summary>Поменять местами значения двух чисел</summary>
        public static void Swap(ref double a, ref double b)
        {
            double temp = a;
            a = b;
            b = temp;
        }

        /// <summary>Скалярное произведние двух векторов</summary>
        /// <returns>Результат произведения</returns>
        /// <exception cref="ArgumentException">Исключение при разных размерах векторов</exception>
        public static double ScalarMult(double[] a, double[] b)
        {
            double result = 0;
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Arrays sizes not equal");
            }

            for (int i = 0; i < a.Length; i++)
            {
                result += a[i] * b[i];
            }

            return result;
        }

        /// <summary>Произведение матрицы на вектор</summary>
        /// <param name="matrix">Матрица</param>
        /// <param name="N">Размерность матрицы</param>
        /// <param name="vector">Вектор</param>
        /// <returns>Вектор результат</returns>
        public static double[] MatrixMult(ReferenceElement matrix, int N, double[] vector)
        {
            double[] result = new double[vector.Length];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    result[i] += matrix(i, j) * vector[j];
                }
            }
            return result;
        }
    }
}