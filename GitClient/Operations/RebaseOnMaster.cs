using System;

namespace GitClient.Operations
{
    public class RebaseOnMaster : IOperation
    {
        public string Name => "Rebase current branch onto master";
        public void Operation()
        {
            var hasChanges = GitHelpers.HasPendingChanges();
            var current = GitHelpers.GetCurrentBranch();
            if(hasChanges)
                GitHelpers.Stash();

            if (!GitHelpers.GetToCleanMaster())
            {
                Console.WriteLine("Could not checkout a clean master branch. Will not rebase.");
                return;
            }
            GitHelpers.CheckoutBranch(current);
            GitHelpers.RebaseOnMaster();

            if(hasChanges)
                GitHelpers.PopStash();
        }
    }
}