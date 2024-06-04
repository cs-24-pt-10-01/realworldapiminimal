using CommentEntity = Realworlddotnet.Core.Entities.Comment;
using CommentModel = Realworlddotnet.Api.Features.Articles.Comment;

namespace Realworlddotnet.Api.Features.Articles;

public static class CommentMapper
{
    public static CommentModel MapFromCommentEntity(CommentEntity commentEntity)
    {
        var author = new Author(
            "",
            "",
            "",
            false);
        return new CommentModel(0,
            DateTime.Now,
            DateTime.Now,
            "",
            author);
    }
}
