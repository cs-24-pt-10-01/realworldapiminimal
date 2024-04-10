using Realworlddotnet.Core.Dto;
using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Profiles;

public class ProfilesHandler : IProfilesHandler
{
    private readonly IConduitRepository _repository;

    public ProfilesHandler(IConduitRepository repository)
    {
        Thor.Thor.start_rapl("ProfilesHandler.ProfilesHandler");
        _repository = repository;
        Thor.Thor.stop_rapl("ProfilesHandler.ProfilesHandler");
    }

    public async Task<ProfileDto> GetAsync(string profileUsername, string? username,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ProfilesHandler.GetAsync");
        var profileUser = await _repository.GetUserByUsernameAsync(profileUsername, cancellationToken);

        if (profileUser is null)
        {
            throw new ProblemDetailsException(422, "Profile not found");
        }

        var isFollowing = false;

        if (username is not null)
        {
            isFollowing = await _repository.IsFollowingAsync(profileUsername, username, cancellationToken);
        }

        Thor.Thor.stop_rapl("ProfilesHandler.GetAsync");
        return new ProfileDto(profileUser.Username, profileUser.Bio, profileUser.Image, isFollowing);
    }

    public async Task<ProfileDto> FollowProfileAsync(string profileUsername, string username,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ProfilesHandler.FollowProfileAsync");
        var profileUser = await _repository.GetUserByUsernameAsync(profileUsername, cancellationToken);

        if (profileUser is null)
            throw new ProblemDetailsException(422, "Profile not found");
        

        _repository.Follow(profileUsername, username);
        await _repository.SaveChangesAsync(cancellationToken);

        Thor.Thor.stop_rapl("ProfilesHandler.FollowProfileAsync");
        return new ProfileDto(profileUser.Username, profileUser.Bio, profileUser.Email, true);
    }

    public async Task<ProfileDto> UnFollowProfileAsync(string profileUsername, string username,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ProfilesHandler.UnFollowProfileAsync");
        var profileUser = await _repository.GetUserByUsernameAsync(profileUsername, cancellationToken);

        if (profileUser is null)
        {
            Thor.Thor.stop_rapl("ProfilesHandler.UnFollowProfileAsync");
            throw new ProblemDetailsException(422, "Profile not found");
        }

        _repository.UnFollow(profileUsername, username);
        await _repository.SaveChangesAsync(cancellationToken);

        Thor.Thor.stop_rapl("ProfilesHandler.UnFollowProfileAsync");
        return new ProfileDto(profileUser.Username, profileUser.Bio, profileUser.Email, false);
    }
}
