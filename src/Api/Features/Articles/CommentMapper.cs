using CommentEntity = Realworlddotnet.Core.Entities.Comment;
using CommentModel = Realworlddotnet.Api.Features.Articles.Comment;

namespace Realworlddotnet.Api.Features.Articles;

public static class CommentMapper
{
    public static CommentModel MapFromCommentEntity(CommentEntity commentEntity)
    {
        Thor.Thor.start_rapl("CommentMapper.MapFromCommentEntity");
        var author = new Author(
            commentEntity.Author.Username,
            commentEntity.Author.Image,
            commentEntity.Author.Bio,
            commentEntity.Author.Followers.Any());
        Thor.Thor.stop_rapl("CommentMapper.MapFromCommentEntity");
        return new CommentModel(commentEntity.Id,
            commentEntity.CreatedAt,
            commentEntity.UpdatedAt,
            commentEntity.Body,
            author);
    }
}
