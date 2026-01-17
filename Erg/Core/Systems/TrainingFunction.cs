namespace Erg.Core.Systems;

/// <summary>
/// Defines how training speed changes based on current attribute value.
/// Higher return value = faster training.
/// </summary>
public abstract class TrainingFunction
{
    /// <summary>
    /// Calculates the training factor for a given base value.
    /// </summary>
    /// <param name="baseValue">Current base value of the attribute (0-1)</param>
    /// <returns>Training factor (typically 0-1, but can vary)</returns>
    public abstract double Calculate(double baseValue);

    /// <summary>
    /// Stała szybkość treningu, bez spowolnienia przy wysokich wartościach.
    /// </summary>
    public static TrainingFunction Constant(double value = 1.0) => new ConstantFunction(value);

    /// <summary>
    /// Równomierne spowolnienie — im wyższy poziom, tym wolniejszy trening.
    /// </summary>
    public static TrainingFunction Linear() => new LinearFunction();

    /// <summary>
    /// Początkowo łatwy trening, drastyczne spowolnienie przy wysokich wartościach.
    /// </summary>
    public static TrainingFunction Quadratic() => new QuadraticFunction();

    /// <summary>
    /// Szybkie początkowe spowolnienie, potem stabilny, powolny progres.
    /// </summary>
    public static TrainingFunction SquareRoot() => new SquareRootFunction();

    /// <summary>
    /// Trening nigdy nie zatrzymuje się całkowicie, zawsze jest minimalny postęp.
    /// </summary>
    public static TrainingFunction Exponential(double k = 3.0) => new ExponentialFunction(k);

    /// <summary>
    /// Jak liniowy, ale z gwarantowanym minimum — nawet mistrz może się rozwijać.
    /// </summary>
    public static TrainingFunction Capped(double minimum = 0.1) => new CappedFunction(minimum);

    // ========== Implementacje ==========

    private sealed class ConstantFunction : TrainingFunction
    {
        private readonly double _value;
        public ConstantFunction(double value) => _value = value;
        public override double Calculate(double baseValue) => _value;
    }

    private sealed class LinearFunction : TrainingFunction
    {
        public override double Calculate(double baseValue) => 1.0 - baseValue;
    }

    private sealed class QuadraticFunction : TrainingFunction
    {
        public override double Calculate(double baseValue)
        {
            double diff = 1.0 - baseValue;
            return diff * diff;
        }
    }

    private sealed class SquareRootFunction : TrainingFunction
    {
        public override double Calculate(double baseValue) => Math.Sqrt(1.0 - baseValue);
    }

    private sealed class ExponentialFunction : TrainingFunction
    {
        private readonly double _k;
        public ExponentialFunction(double k) => _k = k;
        public override double Calculate(double baseValue) => Math.Exp(-_k * baseValue);
    }

    private sealed class CappedFunction : TrainingFunction
    {
        private readonly double _minimum;
        public CappedFunction(double minimum) => _minimum = minimum;
        public override double Calculate(double baseValue) => Math.Max(_minimum, 1.0 - baseValue);
    }
}
