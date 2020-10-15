using UnityEngine;

public class Utilities
{
    public static float RoundTo(float number, int digits)
    {
        float exponent = Mathf.Pow(10, digits);
        return Mathf.Round(number * exponent) / exponent;
    }

    
    public static Vector2 RandomPositionInSpawnCircle(float spawnRange)
    {
        Vector2 pos = Random.insideUnitCircle * spawnRange;
        // 50% prob of negative value
        if (Random.value >= 0.5f)
        {
            pos *= -1;
        }

        return pos;
    }

}