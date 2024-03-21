using MagellanGPT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagellanGPT.Infrastructure.Persistence.Configurations;

public class USerConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToContainer("Users")
        .HasPartitionKey(u => u.PartitionKey)
        .HasNoDiscriminator()
        .Property(o => o.Id).ToJsonProperty("id");
    }
}

