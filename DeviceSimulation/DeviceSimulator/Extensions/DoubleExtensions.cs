using Bogus;

namespace DeviceSimulator.Extensions
{
    public static class DoubleExtensions
    {
        public static double TweakValue(this double baseValue, double maxRange)
        {
            var randomizer = new Randomizer();
            var oddEven = randomizer.Int() % 2 == 0;
            var delta = randomizer.Double(0, maxRange);
            if (oddEven)
            {
                baseValue += delta;
            }
            else
            {
                baseValue -= delta;
            }

            return baseValue;
        }
    }
}
