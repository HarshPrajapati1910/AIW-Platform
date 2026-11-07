using System;
using System.Collections.Generic;
using System.Threading;

namespace AIW.Application.Common.Interfaces;

public interface IAgentReviewClient
{
    IAsyncEnumerable<string> StreamReviewAsync(
        Guid documentId,
        string filename,
        string sourceCode,
        CancellationToken cancellationToken = default
    );
}