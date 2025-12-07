using System.Net;
using System.Net.Mail;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.TestSmtpConfiguration;

public class TestSmtpConfigurationCommandHandler : IRequestHandler<TestSmtpConfigurationCommand, TestSmtpConfigurationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TestSmtpConfigurationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TestSmtpConfigurationCommandResponse> Handle(TestSmtpConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (config == null)
        {
            throw NotFoundException.ForEntity("SmtpConfiguration", request.Id);
        }

        try
        {
            // Test SMTP connection
            using var client = new SmtpClient(config.SmtpHost, config.SmtpPort)
            {
                Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword),
                EnableSsl = config.EnableSsl,
                Timeout = 10000 // 10 seconds timeout
            };

            // Try to connect
            await client.SendMailAsync(
                new MailMessage(
                    config.FromEmail,
                    config.FromEmail,
                    "SMTP Test",
                    "This is a test email to verify SMTP configuration."
                ),
                cancellationToken
            );

            // Update test result
            config.LastTestedAt = DateTime.UtcNow;
            config.LastTestSucceeded = true;
            config.LastTestError = null;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TestSmtpConfigurationCommandResponse
            {
                Success = true,
                Message = "SMTP connection test successful!"
            };
        }
        catch (Exception ex)
        {
            // Update test result
            config.LastTestedAt = DateTime.UtcNow;
            config.LastTestSucceeded = false;
            config.LastTestError = ex.Message;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TestSmtpConfigurationCommandResponse
            {
                Success = false,
                Message = $"SMTP connection test failed: {ex.Message}"
            };
        }
    }
}
