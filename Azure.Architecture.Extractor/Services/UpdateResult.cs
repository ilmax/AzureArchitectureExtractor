namespace Azure.Architecture.Extractor.Services;

public abstract record UpdateResult
{
    public sealed record Success(int NewServices, int NewDependencies) : UpdateResult;

    public sealed record NoChanges : UpdateResult;

    public sealed record Failure(Exception Exception) : UpdateResult;
}