// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;

namespace OSS.GenerateNotice.Models
{
    public record ProjectAssetsFile(int Version, ImmutableDictionary<string, ProjectAssetsLibrary> Libraries);

    public record ProjectAssetsLibrary(string Type, string Path);
}
