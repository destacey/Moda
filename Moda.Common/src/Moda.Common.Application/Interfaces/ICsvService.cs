namespace Moda.Common.Application.Interfaces;

public interface ICsvService
{
    public IEnumerable<T> ReadCsv<T>(Stream file);
}
