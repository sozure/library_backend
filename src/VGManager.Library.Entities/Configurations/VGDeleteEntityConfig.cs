using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGManager.Library.Entities.VGEntities;

namespace VGManager.Library.Entities.Configurations;

public class VGDeleteEntityConfig : IEntityTypeConfiguration<VGDeleteEntity>
{
    /// <summary>
    /// Create configurations.
    /// </summary>
    /// <param name="builder">EntityTypeBuilder <see cref="EntityTypeBuilder{DeletionEntity}"/>.</param>
    public void Configure(EntityTypeBuilder<VGDeleteEntity> builder)
    {
        builder.HasKey(deletion => deletion.Id);
        builder.Property(deletion => deletion.Id).ValueGeneratedOnAdd();
        builder.ToTable("Variable_deletions");
    }
}
