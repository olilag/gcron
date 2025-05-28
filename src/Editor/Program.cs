using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Editor;

class Program
{
    static string OneOfRequiredText(params Option[] options)
    {
        Debug.Assert(options.Length >= 2);

        var names = options.Select(o => $"'-{o.Name}'").ToArray();
        var list = names.Length == 2
            ? $"{names[0]} or {names[1]}"
            : string.Join(", ", names[0..(names.Length - 1)]) + ", or " + names[^1];
        return $"Exactly one of the options {list} is required.";
    }

    static void ValidateOneOf(CommandResult commandResult, params Option[] options)
    {
        Debug.Assert(options.Length >= 2);

        if (options.Count(option => commandResult.FindResultFor(option) is not null) != 1)
        {
            commandResult.ErrorMessage = OneOfRequiredText(options);
        }
    }

    static int Main(string[] args)
    {
        var editOption = new Option<bool?>(["-e"], "edit configuration")
        {
            Arity = ArgumentArity.Zero
        };
        var validateOption = new Option<FileInfo>(["-T"], "verify file syntax");
        var listOption = new Option<bool?>(["-l"], "list current jobs")
        {
            Arity = ArgumentArity.Zero
        };
        var removeOption = new Option<bool?>(["-r"], "remove current jobs")
        {
            Arity = ArgumentArity.Zero
        };
        var rootCommand = new RootCommand("Crontab");
        rootCommand.AddOption(editOption);
        rootCommand.AddOption(validateOption);
        rootCommand.AddOption(listOption);
        rootCommand.AddOption(removeOption);
        rootCommand.AddValidator(commandResult => { ValidateOneOf(commandResult, editOption, validateOption, listOption, removeOption); });
        rootCommand.SetHandler((edit, file, list, remove) =>
        {
            var rv = (edit, file, list, remove) switch
            {
                (true, _, _, _) => Editor.EditJobs(),
                var (_, f, _, _) when f is not null => Editor.CheckSyntax(f),
                (_, _, true, _) => Editor.ListJobs(),
                (_, _, _, true) => Editor.ClearJobs(),
                _ => 0,
            };
            return Task.Run(() => rv);
        }, editOption, validateOption, listOption, removeOption);
        return rootCommand.Invoke(args);
        // TODO: add option -e -> open editor (save file to /var/spool/gcron/{user} on Linux), edit it in some temp file
    }
}
