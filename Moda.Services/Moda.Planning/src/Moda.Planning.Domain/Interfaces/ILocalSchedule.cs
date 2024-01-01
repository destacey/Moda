namespace Moda.Planning.Domain.Interfaces;
public interface ILocalSchedule
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    LocalDateRange DateRange { get; }
}
