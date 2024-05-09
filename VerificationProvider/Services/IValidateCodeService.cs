using Microsoft.AspNetCore.Http;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface IValidateCodeService
    {
        Task<ValidateRequest> UnpackRequestAsync(HttpRequest req);
        Task<bool> ValidateCodeAsync(ValidateRequest request);
    }
}