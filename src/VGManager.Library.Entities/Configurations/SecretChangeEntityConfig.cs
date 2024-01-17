using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGManager.Library.Entities.SecretEntities;

namespace VGManager.Library.Entities.Configurations;

public class SecretChangeEntityConfig : IEntityTypeConfiguration<SecretChangeEntity>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">EntityTypeBuilder <see cref="EntityTypeBuilder{SecretChangeEntity}"/>.</param>
    public void Configure(EntityTypeBuilder<SecretChangeEntity> builder)
    {
        builder.HasKey(secret => secret.Id);
        builder.Property(secret => secret.Id).ValueGeneratedOnAdd();
        builder.ToTable("Secret_changes");
    }
}
