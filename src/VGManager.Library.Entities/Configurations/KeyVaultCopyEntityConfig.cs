using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VGManager.Library.Entities.SecretEntities;

namespace VGManager.Library.Entities.Configurations;

public class KeyVaultCopyEntityConfig : IEntityTypeConfiguration<KeyVaultCopyEntity>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">EntityTypeBuilder <see cref="EntityTypeBuilder{KeyVaultCopyEntity}"/>.</param>
    public void Configure(EntityTypeBuilder<KeyVaultCopyEntity> builder)
    {
        builder.HasKey(kvCopy => kvCopy.Id);
        builder.Property(kvCopy => kvCopy.Id).ValueGeneratedOnAdd();
        builder.ToTable("KeyVault_copy");
    }
}
