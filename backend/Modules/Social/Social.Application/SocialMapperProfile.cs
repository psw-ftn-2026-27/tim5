using AutoMapper;
using Social.Application.Blogs;
using Social.Application.UploadedImages;
using Social.Domain.Blogs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        CreateMap<Blog, BlogDto>()
            .ForCtorParam(nameof(BlogDto.Images),
                options => options.MapFrom(blog => blog.ImageIds.Select(id => ImageAddress.For(id)).ToList()));
        CreateMap<Comment, CommentDto>();
    }
}
