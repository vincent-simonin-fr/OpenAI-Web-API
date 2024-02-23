using MagellanGPT.Application.ChatbotUseCases.Queries;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagellanGPT.API.Controllers;

[Route("api/[controller]")]
public class ChatbotController : ApiControllerBase
{
    // GET: api/values
    [HttpGet]
    public async Task<ActionResult<Conversation>> Get()
    {
        return Ok(await Mediator.Send(new GetDataFromCosmosDb()));
    }

    // POST api/values
    [HttpPost]
    public async Task<ActionResult<string>> Post([FromBody]string answer)
    {
        var response = await Mediator.Send(new GetAIResponse { Answer = answer });
        return Ok(response);
    }
}

