using FluentAssertions;
using FluentAssertions.Execution;
using OSS.GenerateNotice;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace GenerateNotice.IntegrationTests
{
    [TestClass]
    public class RootCommandTests
    {
        [NotNull]
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public async Task VersionArgumentShouldPrintOutTheVersion()
        {
            var result = await GenerateNoticeAsync(this.TestContext, "--version");

            using (new AssertionScope())
            {
                result.ExitCode.Should().Be(0);
                result.StdErr.Should().BeEmpty();
                result.StdOut.Should().SatisfyRespectively(line => Regex.Match(line, @"^\d+\.\d+\.\d+\+[\da-f]{10}$").Success.Should().BeTrue());
            }
        }

        [TestMethod]
        public async Task HelpArgumentShoudlPrintHelp()
        {
            var result = await GenerateNoticeAsync(this.TestContext, "--help");

            using (new AssertionScope())
            {
                result.ExitCode.Should().Be(0);
                result.StdErr.Should().BeEmpty();
                result.StdOutAsString.Should().ContainAll(
                    "--help",
                    "--version",
                    "Third-party notice generator",
                    "--asset-files",
                    "--npm-list-json-files",
                    "--output-file",
                    "--preamble-file");
            }
        }

        [TestMethod]
        public async Task GeneratingForSingleAssetsFileShouldWork()
        {
            var assetFilePath = await ReadFileFromStreamAsync(this.TestContext, "bicep.cli.project.assets.json");
            var outputFilePath = GetResultFilePath(this.TestContext, "notice.txt");

            var result = await GenerateNoticeAsync(
                testContext: this.TestContext,
                assetfiles: new[] { assetFilePath },
                npmListAllFiles: new string[0],
                outputFile: outputFilePath,
                preambleFile: null);

            result.ExitCode.Should().Be(0);
            result.StdErr.Should().BeEmpty();
            result.StdOutAsString.Should().ContainAll(
                "NuGet dependency count = 212",
                "NPM dependency count = 0",
                "OK",
                "NOTICE file saved to '",
                "Starting batch 0 (0..99)",
                "Completed batch 0",
                "Starting batch 1 (100..199)",
                "Completed batch 1",
                "Starting batch 2 (200..211)",
                "Completed batch 2");

            File.Exists(outputFilePath).Should().BeTrue();
            var notice = File.ReadAllText(outputFilePath);

            // we don't need to validate the full generated notice file since we don't own the API that produces
            // just need to confirm the output seems reasonable
            notice.Should().ContainAll(
                "---------------------------------------------------------",
                "WindowsAzure.Storage 9.3.3 - Apache-2.0",
                "Apache License",
                "Microsoft.Extensions.Configuration 6.0.1 - MIT",
                "Copyright (c) .NET Foundation Contributors",
                "Permission is hereby granted, free of charge, to any person obtaining a copy",
                "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
                "Microsoft.Extensions.Configuration.Json 6.0.0 - MIT",
                "System.Security.Permissions 4.6.0 - MIT");

            // this copyright should not appear in the notice file
            notice.Should().NotContain("(c) 2008 VeriSign, Inc.");
        }

        [TestMethod]
        public async Task GeneratingForSingleNpmFileShouldWork()
        {
            var npmFilePath = await ReadFileFromStreamAsync(this.TestContext, "textmate.npm-list-all.json");
            var outputFilePath = GetResultFilePath(this.TestContext, "notice.txt");

            var result = await GenerateNoticeAsync(
                testContext: this.TestContext,
                assetfiles: new string[0],
                npmListAllFiles: new[] { npmFilePath },
                outputFile: outputFilePath,
                preambleFile: null);

            result.ExitCode.Should().Be(0);
            result.StdErr.Should().BeEmpty();
            result.StdOutAsString.Should().ContainAll(
                "NuGet dependency count = 0",
                "NPM dependency count = 393",
                "OK",
                "NOTICE file saved to '",
                "Starting batch 0 (0..99)",
                "Completed batch 0",
                "Starting batch 1 (100..199)",
                "Completed batch 1",
                "Starting batch 2 (200..299)",
                "Completed batch 2",
                "Starting batch 3 (300..392)",
                "Completed batch 3");

            File.Exists(outputFilePath).Should().BeTrue();
            var notice = File.ReadAllText(outputFilePath);

            // we don't need to validate the full generated notice file since we don't own the API that produces
            // just need to confirm the output seems reasonable
            notice.Should().ContainAll(
                "---------------------------------------------------------",
                "tslib 1.14.1 - 0BSD",
                "typescript 4.7.4 - Apache-2.0",
                "\"License\" shall mean the terms and conditions for use, reproduction, and distribution as defined by Sections 1 through 9 of this document.",
                "jest-matcher-utils 28.1.3 - MIT",
                "MIT License",
                "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE");
        }

        /*
         * This test makes an outgoing call to the notices API, which is rather flaky.
         * Disabling for now until we can improve the test or switch to mocked responses.
         */
        //[TestMethod]
        public async Task CombinedMultipleNuGetAndNpmFilesWithPreambleShouldGenerateNoticeFile()
        {
            var nugetFile1 = await ReadFileFromStreamAsync(this.TestContext, "bicep.cli.project.assets.json");
            var nugetFile2 = await ReadFileFromStreamAsync(this.TestContext, "bicep.langserver.project.assets.json");
            var npmFile1 = await ReadFileFromStreamAsync(this.TestContext, "textmate.npm-list-all.json");
            var npmFile2 = await ReadFileFromStreamAsync(this.TestContext, "vscode-bicep.npm-list-all.json");
            var preambleFile = await ReadFileFromStreamAsync(this.TestContext, "sample-preamble.txt");
            var outputFile = GetResultFilePath(this.TestContext, "notice.txt");

            var result = await GenerateNoticeAsync(
                testContext: this.TestContext,
                assetfiles: new[] { nugetFile1, nugetFile2 },
                npmListAllFiles: new[] { npmFile1, npmFile2 },
                outputFile: outputFile,
                preambleFile: preambleFile,
                retryCount: 5,
                timeoutSeconds: 200);

            result.ExitCode.Should().Be(0);
            result.StdErr.Should().BeEmpty();

            File.Exists(outputFile).Should().BeTrue();
            var notice = File.ReadAllText(outputFile);

            // the preamble must be included at the beginning
            notice.Should().StartWith("This is a sample notice file preamble.");

            // we don't need to validate the full generated notice file since we don't own the API that produces
            // just need to confirm the output seems reasonable
            notice.Should().ContainAll(
                "---------------------------------------------------------",
                "WindowsAzure.Storage 9.3.3 - Apache-2.0",
                "Apache License",
                "Microsoft.Extensions.Configuration 6.0.1 - MIT",
                "Copyright (c) .NET Foundation Contributors",
                "Permission is hereby granted, free of charge, to any person obtaining a copy",
                "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
                "Microsoft.Extensions.Configuration.Json 6.0.0 - MIT",
                "System.Security.Permissions 4.6.0 - MIT",
                "tslib 1.14.1 - 0BSD",
                "typescript 4.7.4 - Apache-2.0",
                "\"License\" shall mean the terms and conditions for use, reproduction, and distribution as defined by Sections 1 through 9 of this document.",
                "jest-matcher-utils 28.1.3 - MIT",
                "MIT License",
                "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE");

            // this copyright should not appear in the notice file
            notice.Should().NotContain("(c) 2008 VeriSign, Inc.");
        }

        private static async Task<string> ReadFileFromStreamAsync(TestContext testContext, string streamNameSuffix)
        {
            string streamName = $"GenerateNotice.IntegrationTests.Files.{streamNameSuffix}";
            using var stream = typeof(RootCommandTests).Assembly.GetManifestResourceStream(streamName)
                ?? throw new AssertFailedException($"Could not find resource stream '{streamName}'.");

            var filePath = GetResultFilePath(testContext, streamNameSuffix);

            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            return filePath;
        }

        private static string GetResultFilePath(TestContext testContext, string fileName)
        {
            var fileDirectory = Path.Combine(testContext.ResultsDirectory!, testContext.TestName!);
            Directory.CreateDirectory(fileDirectory);

            return Path.Combine(fileDirectory, fileName);
        }

        private static Task<ToolResult> GenerateNoticeAsync(TestContext testContext,IList<string> assetfiles, IList<string> npmListAllFiles, string outputFile, string? preambleFile, int? batchSize = null, int? retryCount = null, int? timeoutSeconds = null)
        {
            return GenerateNoticeAsync(testContext, CreateArguments(assetfiles, npmListAllFiles, outputFile, preambleFile, batchSize, retryCount, timeoutSeconds));
        }

        private static string CreateArguments(IList<string> assetfiles, IList<string> npmListAllFiles, string outputFile, string? preambleFile, int? batchSize, int? retryCount, int? timeoutSeconds)
        {
            var buffer = new StringBuilder();

            void AppendFile(string fileName)
            {
                buffer.Append('"');
                buffer.Append(fileName);
                buffer.Append("\" ");
            }

            if (assetfiles.Any())
            {
                buffer.Append("--asset-files ");
            }

            foreach (var assetFile in assetfiles)
            {
                AppendFile(assetFile);
            }

            if (npmListAllFiles.Any())
            {
                buffer.Append("--npm-list-json-files ");
            }

            foreach (var npmListFile in npmListAllFiles)
            {
                AppendFile(npmListFile);
            }

            buffer.Append("--output-file ");
            AppendFile(outputFile);

            if (preambleFile is not null)
            {
                buffer.Append("--preamble-file ");
                AppendFile(preambleFile);
            }

            if (batchSize.HasValue)
            {
                buffer.Append("--batch-size ");
                buffer.Append(batchSize.Value);
                buffer.Append(' ');
            }

            if (retryCount.HasValue)
            {
                buffer.Append("--retry-count ");
                buffer.Append(retryCount.Value);
                buffer.Append(' ');
            }

            if(timeoutSeconds.HasValue)
            {
                buffer.Append("--timeout-seconds ");
                buffer.Append(timeoutSeconds.Value);
                buffer.Append(' ');
            }

            return buffer.ToString();
        }

        private static async Task<ToolResult> GenerateNoticeAsync(TestContext testContext, string arguments)
        {
            var toolAssemblyFilePath = typeof(Program).Assembly.Location;
            var toolDirectoryPath = Path.GetDirectoryName(toolAssemblyFilePath) ?? throw new Exception($"Unable to determine directory path for assembly file '{toolAssemblyFilePath}'.");

            var result = await ToolRunner.Run(
                workingDirectory: toolDirectoryPath,
                fileName: "dotnet",
                arguments: $"\"{toolAssemblyFilePath}\" {arguments}");

            testContext.WriteLine("--- stderr ---");
            foreach (var line in result.StdErr)
            {
                testContext.WriteLine(line);
            }

            testContext.WriteLine("--- stdout ---");
            foreach (var line in result.StdOut)
            {
                testContext.WriteLine(line);
            }

            return result;
        }
    }
}