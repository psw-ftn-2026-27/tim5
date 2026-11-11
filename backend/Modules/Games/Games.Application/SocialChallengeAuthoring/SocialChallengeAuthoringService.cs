using AutoMapper;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SocialChallengeAuthoringService(ISocialChallengeRepository challengeRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SocialChallengeDto> CreateAsync(SocialChallengeInputDto dto)
    {
        var window = new TimeWindow(dto.Window.Start, dto.Window.End);
        var challenge = new SocialChallenge(dto.Name, dto.ExitMessage, dto.RequiredPeople, dto.RangeInMeters, dto.Xp, window);

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SocialChallengeDto>(challenge);
    }

    public async Task UpdateAsync(Guid challengeId, SocialChallengeInputDto dto)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Challenge {challengeId} does not exist.");
        }

        var window = new TimeWindow(dto.Window.Start, dto.Window.End);
        challenge.Update(dto.Name, dto.ExitMessage, dto.RequiredPeople, dto.RangeInMeters, dto.Xp, window);
        await _unitOfWork.SaveChangesAsync();
    }
}
