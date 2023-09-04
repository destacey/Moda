namespace Moda.Common.Application.Interfaces.Work;
public interface IExternalWorkItem
{
    int? Id { get; }
    int? Rev { get; }

    // TODO: flatten this out to the fields we want to use
    IDictionary<string, object> Fields { get; }
}
