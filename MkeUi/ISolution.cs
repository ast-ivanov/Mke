namespace MkeUi
{
    public interface ISolution
    {
        SolutionParams SolutionParams { get; set; }
        (double[] q, double[] U) Calculate();
    }
}
