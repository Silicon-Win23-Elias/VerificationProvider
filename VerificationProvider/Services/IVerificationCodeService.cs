using Azure.Messaging.ServiceBus;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface IVerificationCodeService
    {
        string GenerateCode();
        EmailRequest GenerateEmailRequest(VerificationRequest request, string code);
        string GenerateServiceBusEmailRequest(EmailRequest emailRequest);
        Task<bool> SaveVerificationRequest(VerificationRequest request, string code);
        VerificationRequest UnpackRequest(ServiceBusReceivedMessage message);
    }
}