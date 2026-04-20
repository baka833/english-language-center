using EnglishCenter.Web.Models.Auth;

namespace EnglishCenter.Web.Services;

public interface IAuthApiClient
{
    Task<AuthResponseModel> LoginAsync(LoginForm form, CancellationToken cancellationToken = default);
}
