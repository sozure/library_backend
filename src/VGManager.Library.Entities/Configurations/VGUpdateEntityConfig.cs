using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGManager.Library.Entities.VGEntities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGManager.Library.Entities.VGEntities;

namespace VGManager.Library.Entities.Configurations;

public class VGUpdateEntityConfig : IEntityTypeConfiguration<VGUpdateEntity>
{
    /// <summary>
    /// Create configurations.
    /// </summary>
    /// <param name="builder">EntityTypeBuilder <see cref="EntityTypeBuilder{EditionEntity}"/>.</param>
    public void Configure(EntityTypeBuilder<VGUpdateEntity> builder)
    {
        builder.HasKey(editon => editon.Id);
        builder.Property(editon => editon.Id).ValueGeneratedOnAdd();
        builder.ToTable("Variable_editions");
    }
}
