using MagellanGPT.Application.RAGUseCases.Commands;
using Microsoft.AspNetCore.Mvc;

namespace MagellanGPT.API.Controllers;

public class RagController : ApiControllerBase
{

    // POST api/rag
    [HttpPost]
    public async Task<ActionResult<string>> OnPostUploadAsync(string question, List<IFormFile> files)
    {
        List<string> filePathList = new();
        long size = files.Sum(f => f.Length);

        foreach (var formFile in files)
        {
            if (formFile.Length > 0)
            {
                var filePath = $"./Files/{Path.GetRandomFileName()}-user.pdf";

                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
                filePathList.Add(filePath);
            }
        }

        var completions = await Mediator.Send(new CreateAICompletionWithMemorizePDfFiles { FilePathList = filePathList, Demand = question });

        // TODO : Refactor loop for reusable function
        //await foreach (var completion in completions)
        //{
        //    if (completion.ContentUpdate is not null)
        //    {
        //        await Task.Delay(20);
        //        yield return completion.ContentUpdate;
        //    }
        //}

        return Ok(completions);
    }
}

