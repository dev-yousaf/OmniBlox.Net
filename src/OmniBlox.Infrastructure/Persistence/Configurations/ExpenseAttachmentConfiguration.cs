using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Infrastructure.Persistence.Configurations;

public class ExpenseAttachmentConfiguration : IEntityTypeConfiguration<ExpenseAttachment>
{
    public void Configure(EntityTypeBuilder<ExpenseAttachment> builder)
    {
        builder.ToTable("ExpenseAttachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.FileType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        builder.HasOne(x => x.Expense).WithMany(x => x.Attachments).HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Cascade);
    }
}
