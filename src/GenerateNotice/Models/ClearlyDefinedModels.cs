// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;

namespace OSS.GenerateNotice.Models
{
    public record NoticeRequest(ImmutableArray<string> Coordinates, NoticeRequestOptions Options, string? Renderer);

    public record NoticeRequestOptions();

    public record NoticeResponse<T>(T Content, NoticeResponseSummary Summary) where T : notnull;

    public record NoticeResponseSummary(int Total, NoticeResponseWarnings Warnings);

    public record NoticeResponseWarnings(ImmutableArray<string> NoDefinition, ImmutableArray<string> NoLicense, ImmutableArray<string> NoCopyright);

    public record NoticeResponseJsonContent(ImmutableArray<NoticeResponseJsonPackage> Packages);

    public record NoticeResponseJsonPackage(string Uuid, string Name, string Version, string Website, string License, string Text, ImmutableArray<string>? Copyrights);
}
