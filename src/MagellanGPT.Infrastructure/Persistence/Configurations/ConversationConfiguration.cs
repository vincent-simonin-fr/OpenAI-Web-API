
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagellanGPT.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToContainer(CosmosDbConst.ConversationContainer)
            .HasNoDiscriminator()
            .Property(o => o.Id).ToJsonProperty("id");
    }
}

