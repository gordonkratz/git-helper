using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GitClient
{
    public static class GitHelpers
    {
        private const string repo = "C:\\etdev";

        internal static void DeleteBranch(string branch, bool force = false)
        {
            var command = $"branch -d {branch}";
            if (force)
                command += " -f";
            GitCommand(command);
        }

        internal static bool GetToCleanMaster()
        {
            if (HasPendingChanges())
            {
                Console.WriteLine("Repo has pending changes.");
                return false;
            }

            GitCommand("checkout master");
            GitCommand("pull origin");
            return true;
        }

        internal static bool HasPendingChanges()
        {
            var output =  GitCommand("status -s");
            return !string.IsNullOrWhiteSpace(output);
        }

        internal static void Stash()
        {
            GitCommand("stash push");
        }

        internal static void PopStash()
        {
            GitCommand("stash pop");
        }

        internal static string GetCurrentBranch()
        {
            return GetBranches().First(b => b.Contains("*")).Trim('*', ' ');
        }

        internal static IEnumerable<string> GetBranches(string additionalArgs = "")
        {
            return GitCommand($"branch {additionalArgs}")
                .Split(Environment.NewLine)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(b => b.Trim());
        }

        internal static void RebaseOnMaster()
        {
            GitCommand("rebase master");
        }

        internal static void CheckoutBranch(string branch)
        {
            GitCommand($"checkout {branch}");
        }

        internal static bool NewBranch(string branchName, string startingPoint = null)
        {
            var output = GitCommand($"checkout -b {branchName} {startingPoint}");
            return !output.Contains("fatal");
        }

        internal static string GitCommand(string args)
        {
            var command = $"git {args}";
            var output = CommandOutput(command, repo);
            #if DEBUG
            Console.WriteLine(command);
            Console.WriteLine(output);
            #endif
            return output;
        }

        public static string CommandOutput(string command, string workingDirectory = null)
        {
            try
            {
                var args = "/c " + command;
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", args);

                procStartInfo.RedirectStandardError = procStartInfo.RedirectStandardInput = procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                if (null != workingDirectory)
                {
                    procStartInfo.WorkingDirectory = workingDirectory;
                }

                Process proc = new Process();
                proc.StartInfo = procStartInfo;

                StringBuilder sb = new StringBuilder();
                proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    sb.AppendLine(e.Data);
                };
                proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    sb.AppendLine(e.Data);
                };
                proc.Start();


                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                return sb.ToString();
            }
            catch (Exception objException)
            {
                return $"Error in command: {command}, {objException.Message}";
            }
        }
    }
}