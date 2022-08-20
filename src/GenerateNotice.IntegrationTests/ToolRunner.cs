using System.Collections.Immutable;
using System.Diagnostics;

namespace GenerateNotice.IntegrationTests
{
    public class ToolRunner
    {
        public static async Task<ToolResult> Run(string workingDirectory, string fileName, string arguments, CancellationToken cancellationToken = default)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingDirectory
                }
            };

            var outputBuilder = ImmutableArray.CreateBuilder<string>();
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                {
                    outputBuilder.Add(e.Data);
                }
            };

            var errorBuilder = ImmutableArray.CreateBuilder<string>();
            process.ErrorDataReceived += (sender, e) =>
            {
                if(e.Data is not null)
                {
                    errorBuilder.Add(e.Data);
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            return new ToolResult(process.ExitCode, outputBuilder.ToImmutable(), errorBuilder.ToImmutable());
        }
    }
}