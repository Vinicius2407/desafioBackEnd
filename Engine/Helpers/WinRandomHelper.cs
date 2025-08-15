namespace Engine.Helpers;
public class WinRandomHelper
{
    private static readonly Random _random = new Random();

    public static bool WinOrLose()
    {
        int[] fibs = { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };
        double winProbability = Math.Min(fibs[_random.Next(1, 12)] / 100, 0.99);
        double randomValue = _random.NextDouble();
        return randomValue < winProbability;
    }
}
