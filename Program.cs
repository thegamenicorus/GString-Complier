using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GStringCompiler
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();

            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: gstrc [optional: -dll] <sourcecode.gstr>\n");
                return;
            }
            try
            {
                string fileName = GetFileName(args);
                string fileExtension = GetFileExtension(args);

                int start = Environment.TickCount;
                ValidateFileExtension(fileName);
                Scanner scanner = CreateScanner(fileName);

                Parser parser = new Parser(scanner.Tokens);
                CodeGen codeGen = new CodeGen(parser.Proj, Path.GetFileNameWithoutExtension(fileName) + fileExtension);
                int end = Environment.TickCount;

                DisplaySuccessMessage(fileName, fileExtension, start, end);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine();
        }

        private static string GetFileName(string[] args)
        {
            return args.Contains("-dll") ? args.FirstOrDefault(txt => txt != "-dll") : args[0];
        }

        private static string GetFileExtension(string[] args)
        {
            return args.Contains("-dll") ? ".dll" : ".exe";
        }

        private static void ValidateFileExtension(string fileName)
        {
            string fileSurname = Path.GetFileName(fileName).Split('.')[1];
            if (!fileSurname.Equals("gstr"))
                throw new Error("GString compiler doesn't support file type: ." + fileSurname);
        }

        private static Scanner CreateScanner(string fileName)
        {
            using (TextReader input = File.OpenText(fileName))
            {
                return new Scanner(input);
            }
        }

        private static void DisplaySuccessMessage(string fileName, string fileExtension, int start, int end)
        {
            Console.Write("Compile result - ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Compile time - " + (end - start).ToString() + " ms");
            Console.WriteLine("Store location - " + Path.GetFullPath(fileName).Replace(".gstr", fileExtension));
        }
    }
}
