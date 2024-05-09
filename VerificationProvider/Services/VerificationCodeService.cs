using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Context;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationCodeService(IServiceProvider serviceProvider, ILogger<VerificationCodeService> logger) : IVerificationCodeService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<VerificationCodeService> _logger = logger;

    public VerificationRequest UnpackRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (request != null && !string.IsNullOrEmpty(request.Email))
                return request;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCodeGenerator.UnpackRequest() :: {ex.Message}");
        }

        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var random = new Random();
            var code = random.Next(100000, 999999);

            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCodeGenerator.GenerateCode() :: {ex.Message}");
        }

        return null!;
    }

    public EmailRequest GenerateEmailRequest(VerificationRequest request, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest()
                {
                    To = request.Email,
                    Subject = "Verification Code",
                    HtmlBody = $@"
                    <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Verification Code</title>
                        </head>
                        <body>
                            <div style='color: #191919; max-width: 500px'>
                                <h1 style='font-weight: 600; font-size: 48px'>Verification Code </h1>
                                <p style='font-weight: 600; font-size:32px;'>Your requested verification code:</p>
                                <p style='color: #454545; font-size:32px'>{code}</p>
                            </div>
                        </body>
                        </html>
                    ",
                    PlainText = $"Your requested verification code: {code}",
                };
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCodeGenerator.GenerateEmailRequest() :: {ex.Message}");
        }

        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequest request, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();
            var existing = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existing != null)
            {
                existing.Code = code;
                existing.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new Data.Entities.VerificationRequestEntity
                {
                    Code = code,
                    Email = request.Email
                });
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCodeGenerator.SaveVerificationRequest() :: {ex.Message}");
        }

        return false;
    }

    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCodeGenerator.GenerateServiceBusEmailRequest() :: {ex.Message}");
        }

        return null!;
    }
}
