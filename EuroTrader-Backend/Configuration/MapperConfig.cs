using AutoMapper;
using Domain;
using EuroTrader_Backend.Models.Account;

namespace EuroTrader_Backend.Configuration;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<ApiUserDto, AppUser>().ReverseMap().ReverseMap();
    }
}