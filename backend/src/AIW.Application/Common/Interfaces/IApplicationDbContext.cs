using AIW.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIW.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<DocumentReviewReport> DocumentReviewReports { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
