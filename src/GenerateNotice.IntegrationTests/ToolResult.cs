using System.Collections.Immutable;

namespace GenerateNotice.IntegrationTests
{
    public record ToolResult(int ExitCode, ImmutableArray<string> StdOut, ImmutableArray<string> StdErr)
    {
        public string StdOutAsString => string.Join(Environment.NewLine, StdOut);

        public string StdErrAsString => string.Join(Environment.NewLine, StdErr);
    }
}