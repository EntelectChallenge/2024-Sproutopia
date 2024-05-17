using Newtonsoft.Json;
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
        
        public static IEnumerable<T> LoadJson<T>(string filePath, string[] delimeters)
        {
            var jsonObjects = new List<T>();
            var fileContent = "";
            try
            {
                fileContent = File.ReadAllText(filePath);
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            var objectStrings = fileContent.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string objectString in objectStrings)
            {
                try
                {
                    var jsonObject = JsonConvert.DeserializeObject<T>(objectString);

                    jsonObjects.Add(jsonObject);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing JSON object: {ex.Message}");
                }
            }

            return jsonObjects;
        }
        
        public static void WriteJsonToFile(string filePath, string json)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            File.AppendAllText(filePath, json);
            File.AppendAllText(filePath, ";");
        }

        public static T[] DeepCopyArray<T>(T[] source) where T : ICloneable
        {
            if (source == null)
                return null;

            return source.Select(a => (T)a.Clone()).ToArray();
        }

        public static T[][] DeepCopy2DArray<T>(T[][] source)
        {
            if (source == null)
                return null;

            T[][] result = new T[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != null)
                {
                    result[i] = new T[source[i].Length];
                    Array.Copy(source[i], result[i], source[i].Length);
                }
            }
            return result;
        }

        public static Dictionary<TKey, TValue> DeepCopyDictionary<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            if (source == null)
                return null;

            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            foreach (var keyValuePair in source)
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }
            return result;
        }

        public static void SetAllValues<T>(T[][] jaggedArray, T value)
        {
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                for (int j = 0; j < jaggedArray[i].Length; j++)
                {
                    jaggedArray[i][j] = value;
                }
            }
        }
    }
}
