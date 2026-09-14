using NotifyHub.Api.Infrastructure.Mongo;

namespace NotifyHub.Api.Features.Notifications.Create;

public static class Endpoint
{
    public static void MapCreateNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/notifications", async (Command command, MongoContext mongoContext) =>
        {
            var errors = Handler.Validate(command);

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var document = await Handler.Handle(command, mongoContext);

            return Results.Created($"/notifications/{document.Id}", document);
        });
    }
}
