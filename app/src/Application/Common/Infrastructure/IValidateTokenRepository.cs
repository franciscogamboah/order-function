namespace Application.Common.Infrastructure
{
    public interface IValidateTokenRepository
    {
        Task<bool> ValidateTokenWithRemoteAsync(string token);
    }
}
