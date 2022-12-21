using System.Linq;

namespace GitClient.Operations
{
    internal class DeleteMerged : IOperation
    {
        public string Name => "Delete Merged Branches";
        public void Operation()
        {
            if (!GitHelpers.GetToCleanMaster())
                return;

            foreach (var branch in GitHelpers.GetBranches("--merged").Where(s => !s.Contains("master")))
            {
                GitHelpers.DeleteBranch(branch);
            }
        }
    }
    internal class DeletePatchBranches : IOperation
    {
        public string Name => "Delete Patch Branches";
        public void Operation()
        {
            if (!GitHelpers.GetToCleanMaster())
                return;

            foreach (var branch in GitHelpers.GetBranches().Where(s => s.Contains("patch")))
            {
                GitHelpers.DeleteBranch(branch, true);
            }
        }
    }

    internal class DeleteReleaseBranches : IOperation
    {
        public string Name => "Delete Release Branches";
        public void Operation()
        {
            if (!GitHelpers.GetToCleanMaster())
                return;

            foreach (var branch in GitHelpers.GetBranches().Where(s => s.Contains("release")))
            {
                GitHelpers.DeleteBranch(branch);
            }
        }
    }

    internal class CleanGitWorkspace : IOperation
    {
        public string Name => "Clean workspace";
        public void Operation()
        {
            new DeleteMerged().Operation();
            new DeletePatchBranches().Operation();
            new DeleteReleaseBranches().Operation();
            GitHelpers.GitCommand("fetch --prune");
        }
    }
}