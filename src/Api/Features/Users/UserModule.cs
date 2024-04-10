using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Users;

public class UserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        Thor.Thor.start_rapl("UserModule.AddRoutes");
        app.MapGet("/user",
                [Authorize] async (IUserHandler userHandler, ClaimsPrincipal claimsPrincipal) =>
                {
                    Thor.Thor.start_rapl("UserModule.AddRoutes.GetUser");
                    var username = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await userHandler.GetAsync(username!, new CancellationToken());
                    Thor.Thor.stop_rapl("UserModule.AddRoutes.GetUser");
                    return new UserEnvelope<UserDto>(user);
                })
            .Produces<UserEnvelope<UserDto>>()
            .WithTags("User")
            .WithName("GetUser")
            .IncludeInOpenApi();

        app.MapPut("/user",
                [Authorize] async (
                    IUserHandler userHandler,
                    ClaimsPrincipal claimsPrincipal,
                    UserEnvelope<UpdatedUserDto> request
                ) =>
                {
                    Thor.Thor.start_rapl("UserModule.AddRoutes.UpdateUser");
                    if (!MiniValidator.TryValidate(request, out var errors))
                    {
                        Thor.Thor.stop_rapl("UserModule.AddRoutes.UpdateUser");
                        return Results.ValidationProblem(errors);
                    }
                    
                    var username = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await userHandler.UpdateAsync(username!, request.User, new CancellationToken());
                    Thor.Thor.stop_rapl("UserModule.AddRoutes.UpdateUser");
                    return Results.Ok(new UserEnvelope<UserDto>(user));
                })
            .Produces<UserEnvelope<UserDto>>()
            .WithTags("User")
            .WithName("UpdateUser")
            .IncludeInOpenApi();

        app.MapPost("/users",
                async (IUserHandler userHandler, UserEnvelope<NewUserDto> request) =>
                {
                    Thor.Thor.start_rapl("UserModule.AddRoutes.CreateUser");
                    if (!MiniValidator.TryValidate(request, out var errors))
                    {
                        Thor.Thor.stop_rapl("UserModule.AddRoutes.CreateUser");
                        return Results.ValidationProblem(errors);
                    }

                    var user = await userHandler.CreateAsync(request.User, new CancellationToken());
                    Thor.Thor.stop_rapl("UserModule.AddRoutes.CreateUser");
                    return Results.Ok(new UserEnvelope<UserDto>(user));
                })
            .Produces<UserEnvelope<UserDto>>()
            .WithTags("User")
            .WithName("CreateUser")
            .IncludeInOpenApi();

        app.MapPost("/users/login",
                async Task<Results<ValidationProblem, Ok<UserEnvelope<UserDto>>>> (IUserHandler userHandler,
                    UserEnvelope<LoginUserDto> request) =>
                {
                    Thor.Thor.start_rapl("UserModule.AddRoutes.LoginUser");
                    if (!MiniValidator.TryValidate(request, out var errors))
                    {
                        Thor.Thor.stop_rapl("UserModule.AddRoutes.LoginUser");
                        return TypedResults.ValidationProblem(errors);
                    }

                    var user = await userHandler.LoginAsync(request.User, new CancellationToken());
                    Thor.Thor.stop_rapl("UserModule.AddRoutes.LoginUser");
                    return TypedResults.Ok(new UserEnvelope<UserDto>(user));
                })
            .Produces<UnprocessableEntity<ValidationProblem>>(422)
            .WithTags("User")
            .WithName("LoginUser")
            .IncludeInOpenApi().ProducesValidationProblem();
        Thor.Thor.stop_rapl("UserModule.AddRoutes");
    }
}
