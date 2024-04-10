using Realworlddotnet.Core.Dto;
using Realworlddotnet.Core.Repositories;

namespace Realworlddotnet.Api.Features.Articles;

public class ArticlesHandler : IArticlesHandler
{
    private readonly IConduitRepository _repository;

    public ArticlesHandler(IConduitRepository repository)
    {
        Thor.Thor.start_rapl("ArticlesHandler.ArticlesHandler");
        _repository = repository;
        Thor.Thor.stop_rapl("ArticlesHandler.ArticlesHandler");
    }

    public async Task<Article> CreateArticleAsync(
        NewArticleDto newArticle, string username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.CreateArticleAsync");
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        var tags = await _repository.UpsertTagsAsync(newArticle.TagList, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var article = new Article(
                newArticle.Title,
                newArticle.Description,
                newArticle.Body
            ) { Author = user, Tags = tags.ToList() }
            ;

        _repository.AddArticle(article);
        await _repository.SaveChangesAsync(cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.CreateArticleAsync");
        return article;
    }

    public async Task<Article> UpdateArticleAsync(
        ArticleUpdateDto update, string slug, string username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.UpdateArticleAsync");
        var article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken);

        if (article == null)
        {
            throw new ProblemDetailsException(422, "ArticleNotFound");
        }

        if (username != article.Author.Username)
        {
            throw new ProblemDetailsException(403, $"{username} is not the author");
        }

        article.UpdateArticle(update);
        await _repository.SaveChangesAsync(cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.UpdateArticleAsync");
        return article;
    }

    public async Task DeleteArticleAsync(string slug, string username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.DeleteArticleAsync");
        var article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new ProblemDetailsException(new HttpValidationProblemDetails
                      {
                          Status = 422, Title = "Article not found", Detail = $"Slug: {slug}"
                      });

        if (username != article.Author.Username)
        {
            throw new ProblemDetailsException(403, $"{username} is not the author");
        }

        _repository.DeleteArticle(article);
        await _repository.SaveChangesAsync(cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.DeleteArticleAsync");
    }

    public Task<ArticlesResponseDto> GetArticlesAsync(ArticlesQuery query, string? username, bool isFeed,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.GetArticlesAsync");
        Thor.Thor.stop_rapl("ArticlesHandler.GetArticlesAsync");
        return _repository.GetArticlesAsync(query, username, false, cancellationToken);
    }


    public async Task<Article> GetArticleBySlugAsync(string slug, string? username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.GetArticleBySlugAsync");
        var article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new ProblemDetailsException(new HttpValidationProblemDetails
                      {
                          Status = 422, Title = "Article not found", Detail = $"Slug: {slug}"
                      });

        var comments = await _repository.GetCommentsBySlugAsync(slug, username, cancellationToken);
        article.Comments = comments;

        Thor.Thor.stop_rapl("ArticlesHandler.GetArticleBySlugAsync");
        return article;
    }

    public async Task<Core.Entities.Comment> AddCommentAsync(string slug, string username, CommentDto commentDto,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.AddCommentAsync");
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        var article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new ProblemDetailsException(new HttpValidationProblemDetails
                      {
                          Status = 422, Title = "Article not found", Detail = $"Slug: {slug}"
                      });

        var comment = new Core.Entities.Comment(commentDto.body, user.Username, article.Id);
        _repository.AddArticleComment(comment);

        await _repository.SaveChangesAsync(cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.AddCommentAsync");
        return comment;
    }

    public async Task RemoveCommentAsync(string slug, int commentId, string username,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.RemoveCommentAsync");
        _ = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
            throw new ProblemDetailsException(new HttpValidationProblemDetails
            {
                Status = 422, Title = "Article not found", Detail = $"Slug: {slug}"
            });

        var comments = await _repository.GetCommentsBySlugAsync(slug, username, cancellationToken);
        var comment = comments.FirstOrDefault(x => x.Id == commentId) ??
                      throw new ProblemDetailsException(new HttpValidationProblemDetails
                      {
                          Status = 422, Title = "Comment not found", Detail = $"CommentId {commentId}",
                      });


        if (comment.Author.Username != username)
            throw new ProblemDetailsException(new HttpValidationProblemDetails
            {
                Status = 422, Title = "User does not own Article", Detail = $"User: {username},  Slug: {slug}"
            });

        comments.Remove(comment);
        await _repository.SaveChangesAsync(cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.RemoveCommentAsync");
    }

    public async Task<List<Core.Entities.Comment>> GetCommentsAsync(string slug, string? username,
        CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.GetCommentsAsync");
        var comments = await _repository.GetCommentsBySlugAsync(slug, username, cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.GetCommentsAsync");
        return comments;
    }

    public async Task<Article> AddFavoriteAsync(string slug, string username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.AddFavoriteAsync");
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        var article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new ProblemDetailsException(new HttpValidationProblemDetails
                      {
                          Status = 422, Title = "Article not found", Detail = $"Slug: {slug}"
                      });

        var articleFavorite = await _repository.GetArticleFavoriteAsync(user.Username, article.Id);

        if (articleFavorite is null)
        {
            _repository.AddArticleFavorite(new ArticleFavorite(user.Username, article.Id));
            await _repository.SaveChangesAsync(cancellationToken);
        }

        article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.AddFavoriteAsync");
        return article!;
    }

    public async Task<Article> DeleteFavorite(string slug, string username, CancellationToken cancellationToken)
    {
        Thor.Thor.start_rapl("ArticlesHandler.DeleteFavorite");
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        var article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken) ??
                      throw new ProblemDetailsException(new HttpValidationProblemDetails
                      {
                          Status = 422, Title = "Article not found", Detail = $"Slug: {slug}"
                      });

        var articleFavorite = await _repository.GetArticleFavoriteAsync(user.Username, article.Id);

        if (articleFavorite is not null)
        {
            _repository.RemoveArticleFavorite(articleFavorite);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        article = await _repository.GetArticleBySlugAsync(slug, false, cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.DeleteFavorite");
        return article!;
    }

    public async Task<string[]> GetTags(CancellationToken cancellationToken = default)
    {
        Thor.Thor.start_rapl("ArticlesHandler.GetTags");
        var tags = await _repository.GetTagsAsync(cancellationToken);
        Thor.Thor.stop_rapl("ArticlesHandler.GetTags");
        return tags.Select(x => x.Id).ToArray();
    }
}
