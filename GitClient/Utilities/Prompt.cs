using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Spectre.Console;

namespace GitClient.Utilities
{
    public static class Prompt
    {
        public static bool TryGetSelection(string title, out string selection, params string[] choices)
        {
            return TryGetSelection(title, f => f, out selection, choices);
        }
        

        public static bool TryGetSelection<T>(string title, Func<T, string> converter, out T selection, params T[] choices)
        {
            var converted = choices.Select(converter).ToArray();
            AnsiConsole.WriteLine(string.Join(", ", converted));
            var textInput = AnsiConsole.Prompt(new TextPrompt<string>(title).AllowEmpty());
            if (string.IsNullOrEmpty(textInput))
            {
                selection = default;
                return false;
            }
            
            var possibilities = choices
                .Where(c => converter(c).Contains(textInput, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (possibilities.Length != 1)
                return possibilities.Length > 1
                    ? GetReturn(out selection, possibilities)
                    : GetReturn(out selection, choices);

            selection = possibilities[0];
            return true;

            bool GetReturn(out T toReturn, T[] choices)
            {
                toReturn = AnsiConsole.Prompt(new SelectionPrompt<T>()
                    .Title(title)
                    .UseConverter(converter)
                    .AddChoices(choices));
                return true;
            }
        }

        public static bool TryGetMultipleSelection(string prompt, out List<string> selection, params string[] choices)
        {
            AnsiConsole.WriteLine(string.Join(", ", choices));
            var textInput = AnsiConsole.Prompt(new TextPrompt<string>($"Input {prompt}:").AllowEmpty());
            if (string.IsNullOrEmpty(textInput))
            {
                selection = default;
                return false;
            }

            if (textInput.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                return GetReturn(out selection, new Dictionary<string, string[]>()
                {
                    {"Choices", choices}
                });
            }
            

            var possibilities = textInput.Split(",").ToDictionary(s => s, s => GetPossibleChoices(s.Trim(), choices));
            if (possibilities.Values.Any(a => a.Length != 1)) 
                return GetReturn(out selection, possibilities);
            selection = possibilities.Values.Select(v => v[0]).ToList();
            return true;

            bool GetReturn(out List<string> toReturn, Dictionary<string, string[]> choices)
            {
                var p = new MultiSelectionPrompt<string>()
                    .Title(prompt)
                    .Mode(SelectionMode.Leaf)
                    .NotRequired();
                foreach (var choice in choices)
                {
                    p.AddChoiceGroup(choice.Key, choice.Value);
                }
                toReturn = AnsiConsole.Prompt(p);
                return true;
            }

            string[] GetPossibleChoices(string input, string[] options)
            {
                return options.Where(o => o.Contains(input, StringComparison.OrdinalIgnoreCase)).ToArray();
            }
        }

        public static bool GetAndConfirmInput(string prompt, out string input)
        {
            do
            {
                input = AnsiConsole.Prompt(new TextPrompt<string>($"Input {prompt}:").AllowEmpty());
                if (string.IsNullOrWhiteSpace(input))
                    if(AnsiConsole.Confirm("Exit?"))
                        return false;
                    else
                        continue;
                if (AnsiConsole.Confirm($"Are you sure you want [green]{input}[/] for {prompt}"))
                    return true;
            } while (true);
        }

        public static bool GetAndConfirmInput(string prompt, out string selection, params string[] choices)
        {
            selection = null;
            do
            {
                if (TryGetSelection($"Select a {prompt}", out selection, choices))
                {
                    if (AnsiConsole.Confirm($"Are you sure you want [green]{selection}[/] for {prompt}"))
                        return true;
                    continue;
                }

                if (AnsiConsole.Confirm("Exit?"))
                    return false;
            } while (true);
        }

        public static bool TryMultiPrompt(string prompt, out List<string> selection, IEnumerable<(string, string[])> choices)
        {
            var basePrompt = new MultiSelectionPrompt<string>()
                .Title($"Select {prompt}:");
            foreach (var valueTuple in choices)
            {
                basePrompt.AddChoiceGroup(valueTuple.Item1, valueTuple.Item2);
            }

            do
            {
                selection = AnsiConsole.Prompt(basePrompt);
                if (selection.Count > 0)
                {
                    if (AnsiConsole.Confirm($"Select {string.Join(", ", selection)}?"))
                        return true;
                    continue;
                }

                if (AnsiConsole.Confirm("Exit?"))
                    return false;
            } while (true);
        }
    }
}