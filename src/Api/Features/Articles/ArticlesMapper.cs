using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Articles;

public static class ArticlesMapper
{
    public static ArticleResponse MapFromArticleEntity(Article article)
    {
        Thor.Thor.start_rapl("ArticlesMapper.MapFromArticleEntity");
        var tags = article.Tags.Select(tag => tag.Id);
        var author = article.Author;
        var result = new ArticleResponse(
            article.Slug,
            article.Title,
            article.Description,
            article.Body,
            article.CreatedAt,
            article.UpdatedAt,
            tags,
            new Author(
                author.Username,
                author.Image,
                author.Bio,
                author.Followers.Any()),
            article.Favorited,
            article.FavoritesCount);
        Thor.Thor.stop_rapl("ArticlesMapper.MapFromArticleEntity");
        return result;
    }

    public static ArticlesResponse MapFromArticles(ArticlesResponseDto articlesResponseDto)
    {
        Thor.Thor.start_rapl("ArticlesMapper.MapFromArticles");
        var articles = articlesResponseDto.Articles
            .Select(articleEntity => MapFromArticleEntity(articleEntity))
            .ToList();
        Thor.Thor.stop_rapl("ArticlesMapper.MapFromArticles");
        return new ArticlesResponse(articles, articlesResponseDto.ArticlesCount);
    }
}
