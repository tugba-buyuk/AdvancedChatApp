using AutoMapper;
using Entities.Dtos;
using Entities.Models;

namespace SignalRApp.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserDTO,User>();
        }
    }
}
