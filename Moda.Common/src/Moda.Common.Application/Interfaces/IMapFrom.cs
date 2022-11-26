using Mapster;

namespace Moda.Common.Application.Interfaces;
public interface IMapFrom<T> : IRegister
{
    void IRegister.Register(TypeAdapterConfig config) => config.NewConfig(typeof(T), GetType());
}
