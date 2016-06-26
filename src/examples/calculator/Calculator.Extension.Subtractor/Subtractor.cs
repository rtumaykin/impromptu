using System;

namespace Calculator.Extension
{
    public class Subtractor : ICalculator
    {
        public int Calculate(int a, int b)
        {
            // this line shows that it is referencing shared type from its local directory
            Console.WriteLine($"Additor SharedType Runtime Version: {typeof(SharedType).Assembly.ImageRuntimeVersion}. Codebase: {typeof(SharedType).Assembly.CodeBase}.");
            return a - b;
        }
    }
}
