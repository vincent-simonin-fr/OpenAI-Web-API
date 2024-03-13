using MagellanGPT.Application.ChatbotUseCases.Commands.CreateAICompletion;
using MagellanGPT.Application.ChatbotUseCases.Queries;
using MagellanGPT.Application.ChatbotUseCasesCommands;
using MagellanGPT.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace MagellanGPT.API.Controllers;

public class ChatController : ApiControllerBase
{
    private readonly ILogger<ChatController> _logger;

    public ChatController(ILogger<ChatController> logger)
    {
        _logger = logger;
    }

    // GET: api/chat
    // TODO : POC connexion to CosmosDb => To Remove
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> Get()
    {
        return Ok(await Mediator.Send(new GetDataFromCosmosDb()));
    }

    // POST api/chat
    [HttpPost]
    public async IAsyncEnumerable<string> Post([FromBody]string question)
    {
        Response.ContentType = "text/plain";
        var completions = await Mediator.Send(new CreateAICompletion { Demand = question });

        //using (StreamReader reader = new StreamReader(choices))
        //{
        //    yield return  reader.ReadLineAsync().Result;
        //}

        // TODO : Refactor loop for reusable function
        await foreach (var completion in completions)
        {
            if (completion.ContentUpdate is not null)
            {
                await Task.Delay(20);
                yield return completion.ContentUpdate;
            }
        }
    }

    // POST api/chat/synchro
    [HttpPost]
    [Route("synchro")]
    public async Task<ActionResult<string>> PostSynchrone([FromBody] string question)
    {
        Response.ContentType = "text/plain";
        var completions = await Mediator.Send(new CreateAICompletionSynchronously { Demand = question });

        return completions;
    }
}

