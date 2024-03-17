using System.Collections.Generic;
using System.ComponentModel;
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

    //// POST api/chat
    //[HttpPost]
    // [Route("stream")]
    //public async IAsyncEnumerable<string> Post([FromBody]string question)
    //{
    //    Response.ContentType = "text/plain";
    //    var completions = await Mediator.Send(new CreateAICompletion { Demand = question });

    //    //using (StreamReader reader = new StreamReader(choices))
    //    //{
    //    //    yield return  reader.ReadLineAsync().Result;
    //    //}

    //    // TODO : Refactor loop for reusable function
    //    await foreach (var completion in completions)
    //    {
    //        if (completion.ContentUpdate is not null)
    //        {
    //            await Task.Delay(20);
    //            yield return completion.ContentUpdate;
    //        }
    //    }
    //}

    // POST api/chat
    [HttpPost]
    [EndpointDescription("Traitement des demandes utilisateur")]
    [ProducesResponseType(typeof(ActionResult<ResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Description("Traitement des demandes utilisateur")]
    public async Task<ActionResult<ResponseDto>> PostSynchrone([FromBody] CreateAICompletionSynchronously request)
    {
        var completions = await Mediator.Send(request);

        return completions;
    }

    /// <summary>
    /// You can search for Historic here.
    /// </summary>
    /// <remarks>
    ///
    ///  Header of request must contain the field Authorization
    ///  
    /// Sample request:
    /// 
    ///     Get api/Chat
    ///     
    /// </remarks>
    /// <returns> This endpoint returns a list of Accounts.</returns>
    [HttpGet]
    [EndpointDescription("Historique des conversations de l'utilisateur exécutant la requête")]
    [ProducesResponseType(typeof(ActionResult<List<ConversationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ConversationDto>>> GetHistoricByUser()
    {
        return Ok(await Mediator.Send(new GetHistoricOfConversationByCurrentUser()));
    }
}

