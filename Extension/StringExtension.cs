using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler.Extension
{
    public static class StringExtension
    {
        private static string[] ReserveWord = new string[]{
        "Namespace","Class","modifiers","Public","Static","Private","Protected","Main","method","Method","Declare","as",
        "string","int","float","double","char","short","long","value","Read","input","bool",
        "If","Else","else","if","Assign","array","of","pass","by","reference","to",
        "Repeat","since","before","increase","decrease","void","size","position",
        "parameters","arguments","Display","with","line","return","type",
        "End","loop","condition","namespace","class","Do"
        };

        public static bool IsReserved(this string source)
        {
            return ReserveWord.Contains(source);
        }
    }
}
