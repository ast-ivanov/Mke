namespace Mke.Interfaces
{
    /// <summary>Сервис для решения СЛАУ</summary>
    public interface ISlaeService
    {
        /// <summary>Размерность матрицы</summary>
        int N { get; }

        /// <summary>Матрица</summary>
        double[,] A { get; }

        /// <summary>Правая часть системы</summary>
        double[] b { get; }

        /// <summary>Веса</summary>
        double[] q { get; }

        /// <summary>Решить систему методом ЛОС</summary>
        /// <param name="maxiter">Максимальное количество итераций</param>
        /// <param name="eps">Точность</param>
        void CalculateLOS(int maxiter, double eps);

        /// <summary>Решить систему методом сопряжённых градиентов</summary>
        /// <param name="maxiter">Максимальное количество итераций</param>
        /// <param name="eps">Точность</param>
        void CalculateMSG(int maxiter, double eps);

        /// <summary>Решить систему методом Гаусса</summary>
        void CalculateGauss();

        /// <summary>Решить систему методом LU</summary>
        /// <param name="withFactorization">С факторизацией</param>
        void CalculateLU(bool withFactorization);
    }
}