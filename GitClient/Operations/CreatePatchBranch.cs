using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using GitClient.Utilities;
using Spectre.Console;

namespace GitClient.Operations
{
    public class CreatePatchBranch : IOperation
    {
        public string Name => "Create Patch";

        public void Operation()
        {
            var issuePossibilities = GitHelpers.GetBranches().Where(b => b.Contains("issue"))
                .Select(b => b.Split("/").Last()).ToArray();

            if (!Prompt.GetAndConfirmInput("issue", out var selectedIssue, issuePossibilities))
                return;
            if (!Prompt.TryGetMultipleSelection("suite", out var selectedSuites, GetSuiteNames()))
                return;

            var branches = GetBranchPossibilities(selectedSuites);
            if (!Prompt.TryMultiPrompt("cut", out var selectedReleaseBranches, branches))
                return;

            var patchBranchNames = selectedReleaseBranches.Select(rb =>
            {
                var split = rb.Split("/");
                return (rb, $"patch/{selectedIssue}-{split[^2]}-{split[^1]}");
            }).ToArray();

            var patchCommits = GetCherryPickCommitMessages(selectedIssue);

            if (!AnsiConsole.Confirm(
                $"Create {string.Join(", ", patchBranchNames)} and patch {patchCommits.Length} commits?"))
                return;

            foreach (var pb in patchBranchNames)
            {
                if (!GitHelpers.NewBranch(pb.Item2, pb.rb))
                {
                    AnsiConsole.WriteLine($"Branch creation failed. Will not patch commits");
                    continue;
                }

                AnsiConsole.WriteLine($"Created {pb.Item2}");

                foreach (var hash in patchCommits)
                {
                    AnsiConsole.WriteLine($"Cherry picking {hash}");
                    if (!TryCherryPick(hash.Split(' ').First()))
                    {
                        AnsiConsole.WriteLine("Failed patching a commit. Bailing on the rest.");
                        continue;
                    }
                }
                GitHelpers.GitCommand($"push origin {pb.Item2}");
                var url = "https://stash.group1.com/projects/ETDEV/repos/etdev-bare/pull-requests" +
                              "?create" +
                              $"&targetBranch={HttpUtility.UrlEncode(string.Join("/", pb.rb.Split("/").Skip(1)))}" +
                              $"&sourceBranch={HttpUtility.UrlEncode(pb.Item2)}";
                Process.Start(new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }


            //TODO automatically create the pull request on bitbucket
        }

        private bool TryCherryPick(string hash)
        {
            GitHelpers.GitCommand($"cherry-pick {hash}");
            return true;
        }

        private string[] GetSuiteNames()
        {
            var toReturn = new HashSet<string>();
            var branches = GitHelpers.GetBranches("-r --list \"*/release/*\" --contains master@{one.month.ago}");
            foreach (var branch in branches)
            {
                var split = branch.Split("/").ToArray();
                if (split.Length > 2)
                    toReturn.Add(split[2]);
            }

            return toReturn.ToArray();
        }

        private string[] GetCherryPickCommitMessages(string issue)
        {
            return GitHelpers.GitCommand($"rev-list issue/{issue} --oneline --grep={issue} --reverse")
                .Split(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
        }

        private IEnumerable<(string, string[])> GetBranchPossibilities(IEnumerable<string> suites)
        {
            foreach (var selectedSuite in suites)
            {
                yield return (selectedSuite,
                    GitHelpers.GetBranches($"-r --list \"*release*{selectedSuite}*{DateTime.Today.Year}*\"")
                        .TakeLast(3).Reverse().ToArray());
            }
        }
    }
}