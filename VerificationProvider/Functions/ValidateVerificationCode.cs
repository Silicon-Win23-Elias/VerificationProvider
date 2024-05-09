using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
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
    }
}
