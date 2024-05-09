using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Context;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class ValidateCodeService(ILogger<ValidateCodeService> logger, DataContext context) : IValidateCodeService
{
    private readonly ILogger<ValidateCodeService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task<ValidateRequest> UnpackRequestAsync(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var request = JsonConvert.DeserializeObject<ValidateRequest>(body);
                if (request != null)
                    return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateVerificationCode.UnpackRequestAsync() :: {ex.Message}");
        }

        return null!;
    }

    public async Task<bool> ValidateCodeAsync(ValidateRequest request)
    {
        try
        {
            var existingRequest = await _context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingRequest != null && existingRequest.Code == request.Code)
            {
                _context.VerificationRequests.Remove(existingRequest);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateVerificationCode.ValidateCodeAsync() :: {ex.Message}");
        }
        return false;
    }
}
