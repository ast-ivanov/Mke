namespace MkeXyzUi
{
    public interface ISolution
    {
        /// <summary>Параметры решателя</summary>
        SolutionParams SolutionParams { get; set; }

        /// <summary>Получить решение</summary>
        /// <returns>Веса</returns>
        (double[] q, double[] U) Calculate();
    }
}
