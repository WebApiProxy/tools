﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RestCode.Tools.TTGenerator
{
    internal class Generator
    {
        private readonly string generatorClassName;
        private readonly string defaultNamespace;
        private string sourceContent;

        internal Generator(string sourceContent, string defaultNamespace, string generatorClassName)
        {
            this.sourceContent = sourceContent;
            this.defaultNamespace = defaultNamespace;
            this.generatorClassName = generatorClassName;
        }

        private string getUsings(string source, List<string> usings)
        {
            var directiveStartIndex = 0;
            while ((directiveStartIndex = source.IndexOf("<#@")) >= 0)
            {
                var match = Regex.Match(source.Substring(0, directiveStartIndex), @"^\s*$");
                if (!match.Success)
                {
                    throw new Exception("Text is not allowed in the <#@ ... #> section!");
                }
                var directiveEndIndex = source.IndexOf("#>", directiveStartIndex);
                var directive = source.Substring(directiveStartIndex, directiveEndIndex + 2);
                source = source.Substring(directiveEndIndex + 2);
                var directiveMatch = Regex.Match(directive, @"\s*import\s+namespace\s*=\s*""([^""]+)""\s*");
                if (directiveMatch.Success)
                {
                    usings.Add(directiveMatch.Groups[1].Value);
                }
            }
            return source;
        }

        private string generateTransformCode(string source, List<string> declarations)
        {
            var sectionStartIndex = 0;
            var output = "";
            while ((sectionStartIndex = source.IndexOf("<#")) >= 0)
            {
                var sectionEndIndex = source.IndexOf("#>", sectionStartIndex);

                if (sectionStartIndex > 0)
                {
                    output += @"__sb.Append(@""" + Regex.Replace(source.Substring(0, sectionStartIndex), @"""", @"""""") + @""");" + "\r\n";
                }

                if (source.IndexOf("<#+") == sectionStartIndex)
                {
                    declarations.Add(source.Substring(sectionStartIndex + 3, sectionEndIndex - sectionStartIndex - 3));
                }
                else if (source.IndexOf("<#=") == sectionStartIndex)
                {
                    var exp = source.Substring(sectionStartIndex + 3, sectionEndIndex - sectionStartIndex - 3);
                    output += "__sb.Append(" + exp + ");\r\n";
                }
                else
                {
                    output += source.Substring(sectionStartIndex + 2, sectionEndIndex - sectionStartIndex - 2);
                }
                source = source.Substring(sectionEndIndex + 2);
            }
            if (source.Length > 0)
            {
                output += "__sb.Append(@\"" + Regex.Replace(source, @"""", @"""""") + @""");" + "\r\n";
            }
            return output;
        }

        public string Generate()
        {
            var usings = new List<string>();
            var declarations = new List<string>();

            sourceContent = getUsings(sourceContent, usings);
            var transformCode = generateTransformCode(sourceContent, declarations);

            var result = "";
            result += "/* This file is automatically generated by RestCode Parser Generator */\r\n\r\n";
            result += "namespace " + defaultNamespace + "\r\n";
            result += "{\r\n";
            for (var i = 0; i < usings.Count; i++)
            {
                result += "\tusing " + usings[i] + ";\r\n";
            }
            result += "\r\n";
            result += "    public partial class " + generatorClassName + "\r\n";
            result += "    {\r\n";
            for (var i = 0; i < declarations.Count; i++)
            {
                result += declarations[i] + "\r\n";
            }
            result += "        private System.Text.StringBuilder __sb;\r\n\r\n";
            result += "        private void Write(string text) {\r\n";
            result += "            __sb.Append(text);\r\n";
            result += "        }\r\n\r\n";
            result += "        private void WriteLine(string text) {\r\n";
            result += "            __sb.AppendLine(text);\r\n";
            result += "        }\r\n\r\n";
            result += "        private string transformText()\r\n";
            result += "        {\r\n";
            result += "            __sb = new System.Text.StringBuilder();\r\n";
            result += transformCode + "\r\n";
            result += "            return __sb.ToString();\r\n";
            result += "        }\r\n";
            result += "    }\r\n";
            result += "}\r\n";

            return result;
        }
    }
}
