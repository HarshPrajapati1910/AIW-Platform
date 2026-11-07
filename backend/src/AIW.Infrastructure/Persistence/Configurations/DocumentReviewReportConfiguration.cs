using AIW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIW.Infrastructure.Persistence.Configurations;

public class DocumentReviewReportConfiguration : IEntityTypeConfiguration<DocumentReviewReport>
{
    public void Configure(EntityTypeBuilder<DocumentReviewReport> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ExecutiveSummary).IsRequired();
        
        builder.HasMany(r => r.Findings)
               .WithOne()
               .HasForeignKey(f => f.ReportId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}