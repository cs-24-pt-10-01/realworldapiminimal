using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Tags;

public class TagsHandler : ITagsHandler
{
    private readonly IConduitRepository _repository;

    public TagsHandler(IConduitRepository repository)
    {
        Thor.Thor.start_rapl("TagsHandler.TagsHandler");
        _repository = repository;
        Thor.Thor.stop_rapl("TagsHandler.TagsHandler");
    }


    public async Task<string[]> GetTagsAsync(CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("TagsHandler.GetTagsAsync");
        var tags = await _repository.GetTagsAsync(cancellationToken);
        Thor.Thor.stop_rapl("TagsHandler.GetTagsAsync");
        return tags.Select(x => x.Id).ToArray();
    }
}
