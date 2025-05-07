using System;
using System.Collections.Generic;

public static class RPN
{
    public static int ParseInt(string expression, int power, int wave = 1)
    {
        return (int)Parse(expression, power, wave);
    }

    public static float ParseFloat(string expression, int power, int wave = 1)
    {
        return (float)Parse(expression, power, wave);
    }

    private static double Parse(string expression, int power, int wave)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;

        var stack = new Stack<double>();
        var tokens = expression.Split(' ');

        foreach (var token in tokens)
        {
            if (double.TryParse(token, out double number))
            {
                stack.Push(number);
            }
            else if (token == "power")
            {
                stack.Push(power);
            }
            else if (token == "wave")
            {
                stack.Push(wave);
            }
            else
            {
                double b = stack.Pop();
                double a = stack.Count > 0 ? stack.Pop() : 0;

                switch (token)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/": stack.Push(a / b); break;
                    default: throw new Exception($"Unknown token '{token}' in RPN expression.");
                }
            }
        }

        return stack.Pop();
    }
}
