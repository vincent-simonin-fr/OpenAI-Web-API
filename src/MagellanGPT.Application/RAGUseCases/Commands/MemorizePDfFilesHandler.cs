using MagellanGPT.Application.Common.Models;
using MediatR;

namespace MagellanGPT.Application.RAGUseCases.Commands;

public record MemorizePDfFiles : IRequest<ConversationDTO>;

public class MemorizePDfFilesHandler : IRequestHandler<MemorizePDfFiles, ConversationDTO>
{
    public MemorizePDfFilesHandler()
    {
    }

    public Task<ConversationDTO> Handle(MemorizePDfFiles request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

