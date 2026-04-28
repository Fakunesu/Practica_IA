using System.Collections.Generic;
using UnityEngine;

public static class MyRandom
{
    public static T RouletteWheelSelection<T>(Dictionary<T, float> elements)
    {
        float totalChance = 0f;

        foreach (float chance in elements.Values)
        {
            totalChance += chance;
        }

        float randomValue = Random.Range(0f, totalChance);

        foreach (var elem in elements)
        {
            randomValue -= elem.Value;

            if (randomValue <= 0f)
            {
                return elem.Key;
            }
        }

        return default;
    }
}