using Exploration.Application.Tours;

namespace Exploration.Application.TourExecution;

public sealed record ActiveTourExecutionDto(
    Guid TourId,
    string TourName,
    LocationDto CurrentPosition,
    NextKeyPointDto NextKeyPoint);
