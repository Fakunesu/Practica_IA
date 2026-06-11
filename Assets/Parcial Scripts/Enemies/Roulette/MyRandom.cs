using System.Collections.Generic;
using UnityEngine;

public static class MyRandom
{
    // Elige un elemento según el peso/probabilidad que tenga en el diccionario.
    public static T RouletteWheelSelection<T>(Dictionary<T, float> elements)
    {
        float totalChance = 0f;

        // Sumo todos los pesos para saber el rango total de la ruleta.
        foreach (float chance in elements.Values)
        {
            totalChance += chance;
        }

        float randomValue = Random.Range(0f, totalChance);

        // Voy restando pesos hasta encontrar en qué opción cayó el random.
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