using MediatR;

namespace Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

public class GetAvailabilitySchedulesQuery : IRequest<GetAvailabilitySchedulesQueryResponse>
{
    public Guid UserId { get; set; }
}
