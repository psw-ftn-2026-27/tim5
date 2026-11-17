using AutoMapper;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LocationChallengeAuthoringService(ILocationChallengeRepository challengeRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<LocationChallengeDto> CreateAsync(LocationChallengeInputDto dto)
    {
        var startLocation = new Location(dto.StartLocation.Latitude, dto.StartLocation.Longitude);
        var challenge = new LocationChallenge(dto.Name, dto.Xp, startLocation);

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LocationChallengeDto>(challenge);
    }

    public async Task UpdateAsync(Guid challengeId, LocationChallengeInputDto dto)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException("The location challenge no longer exists.");
        }

        var startLocation = new Location(dto.StartLocation.Latitude, dto.StartLocation.Longitude);
        challenge.Update(dto.Name, dto.Xp, startLocation);
        await _unitOfWork.SaveChangesAsync();
    }
}
