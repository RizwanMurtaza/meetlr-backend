using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Homepage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Homepage.Application.Queries.GetHomepageTemplates;

public class GetHomepageTemplatesQueryHandler : IRequestHandler<GetHomepageTemplatesQuery, GetHomepageTemplatesResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetHomepageTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetHomepageTemplatesResponse> Handle(
        GetHomepageTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<HomepageTemplate>().GetQueryable();

        // Apply filters
        if (request.ActiveOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(t => t.Category == request.Category);
        }

        if (request.IsFree.HasValue)
        {
            query = query.Where(t => t.IsFree == request.IsFree.Value);
        }

        // Order by display order
        query = query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.Name);

        var templates = await query.ToListAsync(cancellationToken);

        var templateDtos = templates.Select(t => new HomepageTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            Description = t.Description,
            Category = t.Category,
            PreviewImageUrl = t.PreviewImageUrl,
            PreviewImageUrlMobile = t.PreviewImageUrlMobile,
            ComponentName = t.ComponentName,
            DefaultPrimaryColor = t.DefaultPrimaryColor,
            DefaultSecondaryColor = t.DefaultSecondaryColor,
            DefaultAccentColor = t.DefaultAccentColor,
            DefaultBackgroundColor = t.DefaultBackgroundColor,
            DefaultTextColor = t.DefaultTextColor,
            DefaultFontFamily = t.DefaultFontFamily,
            AvailableSections = ParseSections(t.AvailableSectionsJson),
            DefaultEnabledSections = ParseEnabledSections(t.DefaultEnabledSectionsJson),
            IsActive = t.IsActive,
            IsFree = t.IsFree,
            DisplayOrder = t.DisplayOrder
        }).ToList();

        return new GetHomepageTemplatesResponse
        {
            Templates = templateDtos,
            TotalCount = templateDtos.Count
        };
    }

    private static List<TemplateSectionDto> ParseSections(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]")
            return new List<TemplateSectionDto>();

        try
        {
            return JsonSerializer.Deserialize<List<TemplateSectionDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<TemplateSectionDto>();
        }
        catch
        {
            return new List<TemplateSectionDto>();
        }
    }

    private static List<string> ParseEnabledSections(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]")
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
