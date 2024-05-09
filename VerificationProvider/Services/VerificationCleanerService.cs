using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Context;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationCleanerService(ILogger<VerificationCleanerService> logger, DataContext context) : IVerificationCleanerService
{
    private readonly ILogger<VerificationCleanerService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task RemoveExipiredRecordsAsync()
    {
        try
        {
            var expired = await _context.VerificationRequests.Where(x => x.ExpiryDate <= DateTime.Now).ToListAsync();
            _context.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCleanerService.RemoveExipiredRecordsAsync() :: {ex.Message}");
        }
    }
}
