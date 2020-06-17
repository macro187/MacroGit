using System;
using MacroDiagnostics;

namespace MacroGit
{

    /// <summary>
    /// A Git operation failed
    /// </summary>
    ///
    public class GitException : Exception
    {

        const string DefaultMessage = "The Git operation failed";


        public GitException(string message)
            : this(message, null)
        {
        }


        public GitException(ProcessExecuteResult processExecuteResult)
            : this(null, processExecuteResult)
        {
        }


        public GitException(string message, ProcessExecuteResult processExecuteResult)
            : this(
                message,
                processExecuteResult?.CommandLine,
                processExecuteResult?.CombinedOutput,
                processExecuteResult?.ExitCode)
        {
        }


        public GitException(string message, string commandLine, string output, int? exitCode)
            : base(message ?? DefaultMessage)
        {
            CommandLine = commandLine ?? "";
            if (CommandLine != "")
                Data.Add("CommandLine", commandLine);

            Output = output ?? "";
            if (Output != "")
                Data.Add("Output", output);

            ExitCode = exitCode;
            if (ExitCode != null)
                Data.Add("ExitCode", exitCode);
        }


        /// <summary>
        /// The full command line that was executed, or an empty string if unavailable
        /// </summary>
        ///
        public string CommandLine { get; }


        /// <summary>
        /// The output of the Git command that was executed, or an empty string if unavailable
        /// </summary>
        ///
        public string Output { get; }


        /// <summary>
        /// The exit code returned by Git, or <c>null</c> if unavailable
        /// </summary>
        ///
        public int? ExitCode { get; }

    }
}
