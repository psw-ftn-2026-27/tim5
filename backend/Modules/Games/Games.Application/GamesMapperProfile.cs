using AutoMapper;
using Games.Application.QuizAuthoring;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;

namespace Games.Application;

public sealed class GamesMapperProfile : Profile
{
    public GamesMapperProfile()
    {
        CreateMap<SocialChallenge, SocialChallengeDto>();
        CreateMap<TimeWindow, TimeWindowDto>();
        CreateMap<Quiz, QuizDto>();
        CreateMap<Location, LocationDto>();
        CreateMap<Question, QuestionDto>();
        CreateMap<Answer, AnswerDto>();
    }
}
