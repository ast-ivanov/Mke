namespace Mke.Interfaces
{
    /// <summary>Решатель</summary>
    public interface ISolution<TParams>
    {
        /// <summary>Параметры решателя</summary>
        TParams SolutionParams { get; set; }

        /// <summary>Получить решение</summary>
        /// <returns>Веса, точное решение в узлах</returns>
        (double[] q, double[] U) Calculate();
    }
}
