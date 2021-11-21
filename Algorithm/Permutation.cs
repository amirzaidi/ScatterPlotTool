using System.Collections.Generic;
using System.Linq;

namespace ScatterPlotTool.Algorithm
{
    internal class Permutation
    {
        public static IEnumerable<T[]> FindAll<T>(T[] input)
        {
            if (input.Length <= 1)
            {
                yield return input;
                yield break;
            }

            for (int i = 0; i < input.Length; i++)
            {
                // Select the first element.
                var firstElement = new T[] { input[i] };

                // Copy the other elements to a second array.
                var remainingElements = input.Take(i).Concat(input.Skip(i + 1)).ToArray();

                // Take all permutations of the other elements and append them.
                foreach (var nextElements in FindAll(remainingElements))
                {
                    yield return firstElement.Concat(nextElements).ToArray();
                }
            }
        }
    }
}
