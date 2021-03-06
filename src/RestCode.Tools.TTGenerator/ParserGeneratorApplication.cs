﻿using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace RestCode.Tools.TTGenerator
{
    internal class ParserGeneratorApplication: CommandLineApplication
    {
        internal ParserGeneratorApplication()
        {
            this.Name = "ttgen";
            this.Description = "This tool is used to transform text templates for creating code generators.";
            this.FullName = "RestCode Template Transformation Generator";

            this.Command("create", c =>
            {
                c.Description = "Creates a code generator from the given template.";

                var sourceFileOption = c.Option("-s|--source", "Source template file names", CommandOptionType.MultipleValue);
                var classNameOption = c.Option("-c|--class", "Class name", CommandOptionType.SingleValue);
                var namespaceOption = c.Option("-ns|--namespace", "Namespace", CommandOptionType.SingleValue);
                
                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    var sources = sourceFileOption.Value();
                    if (!sourceFileOption.HasValue())
                    {
                        var dir = Directory.GetCurrentDirectory();
                        Console.WriteLine($"No input file(s) specified. Using *.template in {dir}");
                        sources = string.Join(" ", Directory.GetFiles(dir, "*.template"));
                    }
                    
                    var className = classNameOption.Value();
                    if (!classNameOption.HasValue())
                    {
                        className = "TextTemplateParser";
                    }
                    var ns = namespaceOption.Value();
                    if (!namespaceOption.HasValue())
                    {
                        ns = "RestCode.Tools";
                    }

                    var inputFiles = sources.Split(' ');
                    
                    foreach (var file in inputFiles)
                    {
                        if (!String.IsNullOrEmpty(file))
                        {
                            var content = File.ReadAllText(file);
                            var generator = new Generator(content, ns, className);
                            var result = generator.Generate();
                            var fileName = $"{className}.generated.cs";
                            File.WriteAllText(fileName, result);
                        }
                    }
                    

                    return 0;
                });
            });
            this.OnExecute(() =>
            {
                this.ShowHelp();
                return 2;
            });
        }
    }
}
