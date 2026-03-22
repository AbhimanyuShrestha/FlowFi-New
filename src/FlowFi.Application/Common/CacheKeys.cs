namespace FlowFi.Application.Common;

public static class CacheKeys
{
    public static string Dashboard(Guid userId)          => $"dashboard:{userId}";
    public static string Transactions(Guid userId)       => $"transactions:{userId}";
    public static string UserCategories(Guid userId)     => $"categories:{userId}";
    public static string DefaultCategories()             => "categories:defaults";
    public static string Prediction(Guid userId)         => $"prediction:{userId}";
}
