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
    /// Constant training speed, no slowdown at high values.
    /// </summary>
    public static TrainingFunction Constant(double value = 1.0) => new ConstantFunction(value);

    /// <summary>
    /// Even slowdown — the higher the level, the slower the training.
    /// Parameters start and end define the factor at the beginning (baseValue=0) and end (baseValue=1).
    /// </summary>
    public static TrainingFunction Linear(double start = 1.0, double end = 0.5)
        => new LinearFunction(start, end);

    /// <summary>
    /// Initially easy training, drastic slowdown at high values.
    /// </summary>
    public static TrainingFunction Quadratic() => new QuadraticFunction();

    /// <summary>
    /// Fast initial slowdown, then stable, slow progress.
    /// </summary>
    public static TrainingFunction SquareRoot() => new SquareRootFunction();

    /// <summary>
    /// Training never stops completely, there's always minimal progress.
    /// </summary>
    public static TrainingFunction Exponential(double k = 3.0) => new ExponentialFunction(k);

    /// <summary>
    /// Like linear, but with guaranteed minimum — even a master can improve.
    /// </summary>
    public static TrainingFunction Capped(double minimum = 0.1) => new CappedFunction(minimum);

    // ========== Implementations ==========

    private sealed class ConstantFunction : TrainingFunction
    {
        private readonly double _value;
        public ConstantFunction(double value) => _value = value;
        public override double Calculate(double baseValue) => _value;
    }

    private sealed class LinearFunction : TrainingFunction
    {
        private readonly double _start;
        private readonly double _end;

        public LinearFunction(double start, double end)
        {
            _start = start;
            _end = end;
        }

        public override double Calculate(double baseValue)
            => _start + (_end - _start) * baseValue;
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
