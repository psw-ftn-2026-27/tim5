using Exploration.Domain.Tours;

namespace Exploration.Domain.TourExecutions;

public static class TourProgress
{
    private const double NearbyDistanceInMeters = 20;

    // Only the next key point by order can be reached; being near any other one is not recorded.
    public static void CheckPosition(TourExecution execution, Tour tour, Location position, DateTime checkedAt)
    {
        execution.RecordActivity(checkedAt);

        var completedKeyPointIds = execution.CompletedKeyPoints.Select(completed => completed.KeyPointId).ToList();
        var nextKeyPoint = tour.NextKeyPoint(completedKeyPointIds);
        if (nextKeyPoint is null || position.DistanceTo(nextKeyPoint.Location) > NearbyDistanceInMeters)
        {
            return;
        }

        completedKeyPointIds.Add(nextKeyPoint.Id);
        var isLastKeyPoint = tour.NextKeyPoint(completedKeyPointIds) is null;
        execution.CompleteKeyPoint(nextKeyPoint.Id, checkedAt, isLastKeyPoint);
    }
}
