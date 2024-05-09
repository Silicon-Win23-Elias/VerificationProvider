using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class VerificationCodeGenerator(ILogger<VerificationCodeGenerator> logger, IVerificationCodeService verificationCodeService)
{
    private readonly ILogger<VerificationCodeGenerator> _logger = logger;
    private readonly IVerificationCodeService _verificationCodeService = verificationCodeService;

    [Function(nameof(VerificationCodeGenerator))]
    [ServiceBusOutput("email_request", Connection = "EmailProviderServiceBus")]
    public async Task<string> Run([ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var request = _verificationCodeService.UnpackRequest(message);
            if (request != null)
            {
                var code = _verificationCodeService.GenerateCode();
                if (!string.IsNullOrEmpty(code))
                {
                    var result = await _verificationCodeService.SaveVerificationRequest(request, code);
                    if (result)
                    {
                        var emailRequest = _verificationCodeService.GenerateEmailRequest(request, code);
                        if ( emailRequest != null)
                        {
                            var payload = _verificationCodeService.GenerateServiceBusEmailRequest(emailRequest);
                            if (!string.IsNullOrEmpty(payload))
                            {
                                await messageActions.CompleteMessageAsync(message);
                                return payload;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCodeGenerator.Run() :: {ex.Message}");
        }
        return null!;
    }
}
