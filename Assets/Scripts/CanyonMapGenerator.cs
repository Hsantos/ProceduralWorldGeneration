using UnityEngine;

public static class CanyonMapGenerator
{
    public static float[,] GenerateCanyonMap(int size)
    {
        float[,] map = new float[size, size];
        for (int i = 0; i < size; i++)

        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float) size * 2 - 1;
                float y = 0;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    private static float Evaluate(float value)
    {
        float a = 5f;
        float b = 3f;
        
        // Applying to the formula:
        // f(x) Xª / xª + (b - bx)ª
        // https://www.desmos.com/calculator/tlaxwfvujs
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}