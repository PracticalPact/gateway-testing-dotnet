using Microsoft.AspNetCore.Http;
using PracticalPact.Gateway.ContractData;

namespace PracticalPact.Gateway.Execution;

public sealed class CatchInitialRequestMiddleware(RequestDelegate next, ContractBuilderManager contractBuilderManager)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path == PactRunner.PROVIDER_STATE_ENDPOINT)
        {
            
        }
        else
        {
            ReadHttpRequest readRequest = await ReadHttpRequest.Create(context.Request);
            contractBuilderManager.Builder.MarkOriginalRequest(readRequest);
        }

        await next(context);
    }
}