using Realworlddotnet.Core.Dto;

namespace Realworlddotnet.Api.Features.Articles;

public static class ArticlesMapper
{
    public static ArticleResponse MapFromArticleEntity(Article article)
    {
        var result = new ArticleResponse(
            "",
            "",
            "",
            "",
            DateTime.Now,
            DateTime.Now,
            new List<string>(),
            new Author(
                "",
                "",
                "",
                false),
            false,
            0);
        return result;
    }

    public static ArticlesResponse MapFromArticles(ArticlesResponseDto articlesResponseDto)
    {
        var articles = articlesResponseDto.Articles
            .Select(articleEntity => MapFromArticleEntity(articleEntity))
            .ToList();
        return new ArticlesResponse(articles, articlesResponseDto.ArticlesCount);
    }
}
