using Realworlddotnet.Core.Dto;
using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Users;

public class UserHandler : IUserHandler
{
    private readonly IConduitRepository _repository;
    private readonly ITokenGenerator _tokenGenerator;

    public UserHandler(IConduitRepository repository, ITokenGenerator tokenGenerator)
    {
        Thor.Thor.start_rapl("UserHandler.UserHandler");
        _repository = repository;
        _tokenGenerator = tokenGenerator;
        Thor.Thor.stop_rapl("UserHandler.UserHandler");
    }

    public async Task<UserDto> CreateAsync(NewUserDto newUser, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("UserHandler.CreateAsync");
        var user = new User(newUser);
        await _repository.AddUserAsync(user);
        await _repository.SaveChangesAsync(cancellationToken);
        var token = _tokenGenerator.CreateToken(user.Username);
        Thor.Thor.stop_rapl("UserHandler.CreateAsync");
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }

    public async Task<UserDto> UpdateAsync(
        string username, UpdatedUserDto updatedUser, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("UserHandler.UpdateAsync");
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        user.UpdateUser(updatedUser);
        await _repository.SaveChangesAsync(cancellationToken);
        var token = _tokenGenerator.CreateToken(user.Username);
        Thor.Thor.stop_rapl("UserHandler.UpdateAsync");
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }

    public async Task<UserDto> LoginAsync(LoginUserDto login, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("UserHandler.LoginAsync");
        var user = await _repository.GetUserByEmailAsync(login.Email);

        if (user == null || user.Password != login.Password)
        {
            Thor.Thor.stop_rapl("UserHandler.LoginAsync");
            throw new ProblemDetailsException(422, "Incorrect Credentials");
        }

        var token = _tokenGenerator.CreateToken(user.Username);
        Thor.Thor.stop_rapl("UserHandler.LoginAsync");
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }

    public async Task<UserDto> GetAsync(string username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("UserHandler.GetAsync");
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        var token = _tokenGenerator.CreateToken(user.Username);
        Thor.Thor.stop_rapl("UserHandler.GetAsync");
        return new UserDto(user.Username, user.Email, token, user.Bio, user.Image);
    }
}
