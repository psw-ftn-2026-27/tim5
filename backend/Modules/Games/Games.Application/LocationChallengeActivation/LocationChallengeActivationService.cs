using Exploration.Contracts;
using Games.Application.LocationChallengeActivations;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallengeActivations;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeActivation;

public sealed class LocationChallengeActivationService
{
    private const int MaxExtraMeters = 100;

    private readonly ILocationChallengeActivationRepository _activationRepository;
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IExplorationApi _explorationApi;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeActivationService(
        ILocationChallengeActivationRepository activationRepository,
        ILocationChallengeRepository challengeRepository,
        IExplorationApi explorationApi,
        IUnitOfWork unitOfWork)
    {
        _activationRepository = activationRepository;
        _challengeRepository = challengeRepository;
        _explorationApi = explorationApi;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid userId, Guid challengeId)
    {
        var position = await _explorationApi.GetCurrentPositionAsync(userId);
        if (position is null)
        {
            throw new NotFoundException("Set your position in the position simulator before activating a challenge.");
        }

        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException("The location challenge no longer exists.");
        }

        var existingActivation = await _activationRepository.GetByChallengeAndUserAsync(challengeId, userId);
        var userPosition = new Location(position.Latitude, position.Longitude);
        var extraMeters = Random.Shared.NextDouble() * MaxExtraMeters;

        var activation = LocationChallengeActivator.Activate(challenge, existingActivation, userId, userPosition, extraMeters);
        _activationRepository.Add(activation);
        await _unitOfWork.SaveChangesAsync();
    }
}
