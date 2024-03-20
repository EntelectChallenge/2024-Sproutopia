using System.Diagnostics;

namespace Sproutopia.Utilities
{
    public static class Helpers
    {
        public static bool JaggedArraysEqual<T>(T[][] array1, T[][] array2)
        {
            // Check if arrays have the same length
            if (array1.Length != array2.Length)
                return false;

            // Iterate through each array in the jagged arrays
            for (int i = 0; i < array1.Length; i++)
            {
                // Check if subarrays have the same length
                if (array1[i].Length != array2[i].Length)
                    return false;

                // Iterate through each element in the subarrays
                for (int j = 0; j < array1[i].Length; j++)
                {
                    // Compare corresponding elements
                    if (!array1[i][j].Equals(array2[i][j]))
                        return false;
                }
            }

            // If all elements are equal, arrays are equal
            return true;
        }

        public static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        public static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
            Type elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(
                        InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }

        public static (T, TimeSpan) MeasureExecutionTime<T>(Func<T> function)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            T result = function();
            stopwatch.Stop();

            return (result, stopwatch.Elapsed);
        }

        public static T RandomEnumValue<T>(Random rng) where T : struct, Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            return values[1+rng.Next(values.Length-1)];
        }
    }
}
