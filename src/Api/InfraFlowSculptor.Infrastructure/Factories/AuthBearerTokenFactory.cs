namespace InfraFlowSculptor.Infrastructure.Factories;

/// <summary>
///  Factory to provide a Bearer Token for authorization purposes.
/// </summary>
public static class AuthBearerTokenFactory
{
    private static Func<CancellationToken, Task<string>>? _getBearerTokenAsyncFunc;
  
    /// <summary>
    /// Provide a delegate that returns a bearer token to use for authorization
    /// </summary>
    public static void SetBearerTokenGetterFunc(Func<CancellationToken, Task<string>> getBearerTokenAsyncFunc)
        => _getBearerTokenAsyncFunc = getBearerTokenAsyncFunc;

    /// <summary>
    ///  Get the Bearer Token asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Task<string> GetBearerTokenAsync(CancellationToken cancellationToken)
    {
        return _getBearerTokenAsyncFunc is null 
            ? throw new InvalidOperationException("Must set Bearer Token Func before using it!") 
            : _getBearerTokenAsyncFunc!(cancellationToken);
    }
}