using Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

namespace Meetlr.Api.Endpoints.EmailTemplates.GetAll;

public record GetAllEmailTemplatesResponse
{
    public List<EmailTemplateDto> Templates { get; init; } = new();

    public static GetAllEmailTemplatesResponse FromQueryResponse(GetEmailTemplatesQueryResponse response)
    {
        return new GetAllEmailTemplatesResponse
        {
            Templates = response.Templates
        };
    }
}
