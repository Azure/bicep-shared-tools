// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;

namespace OSS.NoticeGenerator.Models
{
    public record NpmListJsonFile(string Name, string Version, ImmutableDictionary<string, NpmListJsonFileDependency>? Dependencies) : NpmListJsonFileDependency(Version, null, Dependencies);

    public record NpmListJsonFileDependency(string Version, string? Resolved, ImmutableDictionary<string, NpmListJsonFileDependency>? Dependencies);
}
