using MagellanGPT.Domain.Entities;
using MagellanGPT.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagellanGPT.Infrastructure.Persistence.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToContainer("Chat")
            .HasPartitionKey("PartitionKey")
            .HasNoDiscriminator()
            .Property(o => o.Id).ToJsonProperty("id");
    }
}

