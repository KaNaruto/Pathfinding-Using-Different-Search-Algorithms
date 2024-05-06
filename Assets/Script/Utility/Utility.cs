using Random = System.Random;

namespace Script.Utility
{
    public static class Utility
    {
        // Fisher-Yates shuffle algorithm
        public static T[] ShuffleArray<T>(T[] array, int seed)
        {
            Random prng = new Random(seed);

            for (int i = 0; i < array.Length; i++)
            {
                int randomIndex = prng.Next(i, array.Length);
                // Swap
                (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
            }

            return array;
        }
    
    }
}