using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGManager.Library.Entities.VGEntities;

namespace VGManager.Library.Entities.Configurations;

public class VGAddEntityConfig : IEntityTypeConfiguration<VGAddEntity>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">EntityTypeBuilder <see cref="EntityTypeBuilder{AdditionEntity}"/>.</param>
    public void Configure(EntityTypeBuilder<VGAddEntity> builder)
    {
        builder.HasKey(addition => addition.Id);
        builder.Property(addition => addition.Id).ValueGeneratedOnAdd();
        builder.ToTable("Variable_additions");
    }
}
