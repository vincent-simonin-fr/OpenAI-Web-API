using MagellanGPT.Domain.Entities;
using MagellanGPT.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagellanGPT.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToContainer(CosmosDbConst.ConversationContainer)
            .HasPartitionKey("ConversationId")
            .HasNoDiscriminator()
            .Property(o => o.Id).ToJsonProperty("id");
    }
}

