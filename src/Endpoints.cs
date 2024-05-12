namespace Demo;

public static class Endpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/send-message", async (ProducerService broker, MessageRequest request) =>
        {
            await broker.SendMessageAsync(request);
            return Results.Ok(new
            {
                Status = "Message Sent",
                request.Body
            });
        });
    }
}
