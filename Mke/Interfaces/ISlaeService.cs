namespace Mke.Interfaces
{
    /// <summary>Сервис для решения СЛАУ</summary>
    public interface ISlaeService
    {
        /// <summary>Размерность матрицы</summary>
        int N { get; }
        
        /// <summary>Правая часть системы</summary>
        double[] b { get; }
        
        /// <summary>Веса</summary>
        double[] q { get; }

        /// <summary>Решить систему методом ЛОС</summary>
        /// <param name="maxiter">Максимальное количество итераций</param>
        /// <param name="eps">Невязка</param>
        /// <param name="iterationCount">Количество итераций</param>
        /// <param name="discrepancy">Невязка</param>
        void CalculateLOS(int maxiter, double eps, out int iterationCount, out double discrepancy);

        /// <summary>Решить систему методом Гаусса</summary>
        void CalculateGauss();

        /// <summary>Решить систему методом LU</summary>
        /// <param name="withFactorization">С факторизацией</param>
        void CalculateLU(bool withFactorization);

        /// <summary>Получить элемент матрицы A</summary>
        /// <param name="i">Индекс строки</param>
        /// <param name="j">Индекс столбца</param>
        ref double GetElementOfA(int i, int j);
    }
}