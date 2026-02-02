using AutoMapper;

namespace AP.Common.Services.Contracts;

public interface IMapFrom<T>
{
    void Mapping(Profile mapper) => mapper.CreateMap(typeof(T), GetType());
}