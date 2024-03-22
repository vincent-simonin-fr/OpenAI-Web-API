using System;
using MagellanGPT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagellanGPT.Infrastructure.Persistence.Configurations;

public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToContainer(nameof(Organisation))
        .HasPartitionKey(u => u.PartitionKey)
        .HasNoDiscriminator()
        .Property(o => o.Id).ToJsonProperty("id");
    }
}