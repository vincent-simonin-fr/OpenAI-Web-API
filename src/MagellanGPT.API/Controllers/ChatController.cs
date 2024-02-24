using MagellanGPT.Application.ChatbotUseCases.Commands.CreateAICompletion;
using MagellanGPT.Application.ChatbotUseCases.Queries;
using MagellanGPT.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace MagellanGPT.API.Controllers;

public class ChatController : ApiControllerBase
{
    // GET: api/values
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationDTO>>> Get()
    {
        return Ok(await Mediator.Send(new GetDataFromCosmosDb()));
    }

    // POST api/values
    [HttpPost]
    public async IAsyncEnumerable<string> Post([FromBody]string answer)
    {
        Response.ContentType = "text/plain";
        var completions = await Mediator.Send(new CreateAICompletion { Demand = answer });

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
}

