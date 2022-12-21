using GitClient.Operations;
using GitClient.Utilities;
using Spectre.Console;

namespace GitClient
{
    class Program
    {

        static void Main(string[] args)
        {
            var operations = new IOperation[]
            {
                new NewIssue(),
                new CleanGitWorkspace(),
                new CreatePatchBranch(),
                new DeleteMerged(),
                new DeletePatchBranches(),
                new DeleteReleaseBranches(),
                new RebaseOnMaster(),
                new Close()
            };

            while(Prompt.TryGetSelection("Choose a command:", f => f.Name, out var input, operations))
                input.Operation();
        }
    }
}
