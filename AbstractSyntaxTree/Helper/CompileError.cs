using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    class CompileError
    {
        public static void ShowError(string message)
        {
            try
            {
                throw new Error(message);
            }
            catch { throw new Error(); }
        }
    }

    class Error : Exception
    {
        public Error() { }
        public Error(string message)
        {
            Console.Write("Compile result - ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Fail");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Error detected - ");
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }
    }
}
