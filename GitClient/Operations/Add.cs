using GitClient.Utilities;
using Spectre.Console;

namespace GitClient.Operations
{
    public class NewIssue : IOperation
    {
        public string Name => "New issue branch";
        public void Operation()
        {
            if(!Prompt.GetAndConfirmInput("new branch name", out var name))
                return;

            var fullName = $"issue/{name.ToUpper()}";
            
            if(!GitHelpers.GetToCleanMaster())
                return;

            var result = GitHelpers.NewBranch(fullName);
            AnsiConsole.WriteLine(!result ? $"Branch creation failed." : $"Created {fullName}");
        }


    }
}