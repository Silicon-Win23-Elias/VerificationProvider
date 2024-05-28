using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VerificationProvider.Services;

namespace VerificationProvider.Functions
{
    public class ValidateVerificationCode(ILogger<ValidateVerificationCode> logger, IValidateCodeService validateCodeService)
    {
        private readonly ILogger<ValidateVerificationCode> _logger = logger;
        private readonly IValidateCodeService _validateCodeService = validateCodeService;

        [Function("ValidateVerificationCode")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "validate")] HttpRequest req)
        {
            if (!req.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return new UnauthorizedResult();
            }


            var token = authHeader.FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                return new UnauthorizedResult();
            }

            if (!ValidateToken(token, out ClaimsPrincipal claimsPrincipal))
            {
                return new UnauthorizedResult();
            }
        

            try
            {
                var request = await _validateCodeService.UnpackRequestAsync(req);
                if (request != null) 
                {
                    var validationResult = await _validateCodeService.ValidateCodeAsync(request);
                    if (validationResult)
                    {
                        return new OkResult();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : ValidateVerificationCode.Run() :: {ex.Message}");
            }

            return new UnauthorizedResult();
        }

        private static bool ValidateToken(string token, out ClaimsPrincipal claimsPrincipal)
        {
            claimsPrincipal = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("3e0f2db8-3ce3-4e22-b53a-7a609b4b7048");

            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "TokenProvider",
                    ValidateAudience = true,
                    ValidAudience = "Silicon",
                    ValidateLifetime = true,
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
