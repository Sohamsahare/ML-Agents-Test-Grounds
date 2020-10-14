using UnityEngine;

public class Utilities
{
    public static float RoundTo(float number, int digits)
    {
        float exponent = Mathf.Pow(10, digits);
        return Mathf.Round(number * exponent) / exponent;
    }
}