using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Ardalis.GuardClauses;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;
using Moda.Common.Domain.Enums.Work;
using Moda.Work.Domain.Models;

namespace Moda.Analytics.Application.AnalyticsViews.Queries;

public sealed record GetWorkItemAnalyticsQuery(
    Guid AnalyticsViewId,
    int PageNumber = 1,
    int PageSize = 50) : IQuery<Result<AnalyticsViewDataResultDto>>;

public sealed class GetWorkItemAnalyticsQueryValidator : CustomValidator<GetWorkItemAnalyticsQuery>
{
    public GetWorkItemAnalyticsQueryValidator()
    {
        RuleFor(v => v.AnalyticsViewId)
            .NotEmpty();

        RuleFor(v => v.PageNumber)
            .GreaterThan(0);

        RuleFor(v => v.PageSize)
            .InclusiveBetween(1, 500);
    }
}

internal sealed class GetWorkItemAnalyticsQueryHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser) : IQueryHandler<GetWorkItemAnalyticsQuery, Result<AnalyticsViewDataResultDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Maps definition field names to accessor functions on the materialized projection.
    /// </summary>
    private static readonly Dictionary<string, Func<WorkItemProjection, object?>> FieldAccessors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = p => p.Id,
        ["key"] = p => p.Key,
        ["title"] = p => p.Title,
        ["workspace.key"] = p => p.WorkspaceKey,
        ["workspace.name"] = p => p.WorkspaceName,
        ["type.name"] = p => p.TypeName,
        ["status.name"] = p => p.StatusName,
        ["statusCategory"] = p => p.StatusCategory.ToString(),
        ["createdDate"] = p => p.CreatedDate,
        ["changedDate"] = p => p.ChangedDate,
        ["doneDate"] = p => p.DoneDate,
        ["priority"] = p => p.Priority,
        ["stackRank"] = p => p.StackRank,
        ["storyPoints"] = p => p.StoryPoints,
    };

    private static readonly List<AnalyticsViewColumnDefinition> DefaultColumns =
    [
        new() { Field = "key", Alias = "Key" },
        new() { Field = "title", Alias = "Title" },
        new() { Field = "statusCategory", Alias = "Status Category" },
    ];

    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<Result<AnalyticsViewDataResultDto>> Handle(GetWorkItemAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var view = await _analyticsDbContext.AnalyticsViews
            .AsNoTracking()
            .Include(v => v.AnalyticsViewManagers)
            .FirstOrDefaultAsync(v => v.Id == request.AnalyticsViewId, cancellationToken);

        if (view is null)
            return Result.Failure<AnalyticsViewDataResultDto>("Analytics view not found.");

        if (view.Visibility != Visibility.Public && !view.AnalyticsViewManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            return Result.Failure<AnalyticsViewDataResultDto>("You do not have permission to run this analytics view.");

        if (view.Dataset != AnalyticsDataset.WorkItems)
            return Result.Failure<AnalyticsViewDataResultDto>($"Dataset {view.Dataset} is not supported yet.");

        // Parse definition
        AnalyticsViewDefinition? definition = null;
        if (!string.IsNullOrWhiteSpace(view.DefinitionJson))
        {
            try
            {
                definition = JsonSerializer.Deserialize<AnalyticsViewDefinition>(view.DefinitionJson, JsonOptions);
            }
            catch (JsonException)
            {
                // Invalid definition JSON, use defaults
            }
        }

        var selectedColumns = definition?.Columns is { Count: > 0 }
            ? definition.Columns
            : DefaultColumns;

        // Build entity query with filters and sort applied before projection
        IQueryable<WorkItem> entityQuery = _analyticsDbContext.WorkItems.AsNoTracking();

        if (definition?.Filters is { Count: > 0 })
        {
            entityQuery = ApplyFilters(entityQuery, definition.Filters);
        }

        if (definition?.Sort is { Count: > 0 })
        {
            entityQuery = ApplySort(entityQuery, definition.Sort);
        }
        else
        {
            entityQuery = entityQuery.OrderBy(w => w.Id);
        }

        // Get total count before paging (single COUNT query in SQL)
        var totalCount = await entityQuery.CountAsync(cancellationToken);

        // Project to flat record, page, and materialize
        var projectedRows = await entityQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WorkItemProjection(
                w.Id,
                (string)w.Key,
                w.Title,
                (string)w.Workspace.Key,
                w.Workspace.Name,
                w.Type.Name,
                w.Status.Name,
                w.StatusCategory,
                w.Created.ToDateTimeOffset(),
                w.LastModified.ToDateTimeOffset(),
                w.DoneTimestamp != null ? w.DoneTimestamp.Value.ToDateTimeOffset() : null,
                w.Priority,
                w.StackRank,
                w.StoryPoints))
            .ToListAsync(cancellationToken);

        // Build column metadata
        var columns = selectedColumns
            .Where(c => FieldAccessors.ContainsKey(c.Field))
            .Select(c => new AnalyticsViewColumnDto(
                c.Field,
                string.IsNullOrWhiteSpace(c.Alias) ? c.Field : c.Alias!))
            .ToList();

        // Project only selected columns into dictionaries keyed by display name
        var rows = projectedRows
            .Select(row =>
            {
                var dict = new Dictionary<string, object?>();
                foreach (var col in columns)
                {
                    if (FieldAccessors.TryGetValue(col.Field, out var accessor))
                    {
                        dict[col.DisplayName] = accessor(row);
                    }
                }
                return dict;
            })
            .ToList();

        return Result.Success(new AnalyticsViewDataResultDto(columns, rows, totalCount));
    }

    #region Filters

    private static IQueryable<WorkItem> ApplyFilters(
        IQueryable<WorkItem> query,
        List<AnalyticsViewFilterDefinition> filters)
    {
        foreach (var filter in filters)
        {
            query = ApplyFilter(query, filter);
        }
        return query;
    }

    private static IQueryable<WorkItem> ApplyFilter(
        IQueryable<WorkItem> query,
        AnalyticsViewFilterDefinition filter)
    {
        var op = filter.Operator?.Trim().ToLowerInvariant();
        var values = filter.Values ?? [];

        return filter.Field.ToLowerInvariant() switch
        {
            "key" => ApplyStringFilter(query, w => (string)w.Key, op, values),
            "title" => ApplyStringFilter(query, w => w.Title, op, values),
            "workspace.key" => ApplyStringFilter(query, w => (string)w.Workspace.Key, op, values),
            "workspace.name" => ApplyStringFilter(query, w => w.Workspace.Name, op, values),
            "type.name" => ApplyStringFilter(query, w => w.Type.Name, op, values),
            "status.name" => ApplyStringFilter(query, w => w.Status.Name, op, values),
            "statuscategory" => ApplyStatusCategoryFilter(query, op, values),
            "createddate" => ApplyInstantFilter(query, w => w.Created, op, values),
            "changeddate" => ApplyInstantFilter(query, w => w.LastModified, op, values),
            "donedate" => ApplyNullableInstantFilter(query, w => w.DoneTimestamp, op, values),
            "priority" => ApplyNullableIntFilter(query, w => w.Priority, op, values),
            "stackrank" => ApplyDoubleFilter(query, w => w.StackRank, op, values),
            "storypoints" => ApplyNullableDoubleFilter(query, w => w.StoryPoints, op, values),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyStringFilter(
        IQueryable<WorkItem> query,
        Expression<Func<WorkItem, string?>> property,
        string? op,
        List<object?> values)
    {
        var stringValues = values.Select(v => v?.ToString()).Where(v => v != null).ToList();

        return op switch
        {
            "equals" when stringValues.Count > 0
                => query.Where(Combine(property, val => val == stringValues[0])),
            "notequals" when stringValues.Count > 0
                => query.Where(Combine(property, val => val != stringValues[0])),
            "in" when stringValues.Count > 0
                => query.Where(Combine(property, val => stringValues.Contains(val))),
            "notin" when stringValues.Count > 0
                => query.Where(Combine(property, val => !stringValues.Contains(val))),
            "contains" when stringValues.Count > 0
                => query.Where(Combine(property, val => val != null && val.Contains(stringValues[0]!))),
            "startswith" when stringValues.Count > 0
                => query.Where(Combine(property, val => val != null && val.StartsWith(stringValues[0]!))),
            "endswith" when stringValues.Count > 0
                => query.Where(Combine(property, val => val != null && val.EndsWith(stringValues[0]!))),
            "isnull" => query.Where(Combine(property, val => val == null)),
            "isnotnull" => query.Where(Combine(property, val => val != null)),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyInstantFilter(
        IQueryable<WorkItem> query,
        Expression<Func<WorkItem, Instant>> property,
        string? op,
        List<object?> values)
    {
        if (op is "isnull" or "isnotnull") return query;
        if (values.Count == 0) return query;
        if (!TryParseInstant(values[0], out var val1)) return query;

        var param = property.Parameters[0];
        var propBody = property.Body;
        var valConst = Expression.Constant(val1);

        return op switch
        {
            "equals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, valConst), param)),
            "notequals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, valConst), param)),
            "greaterthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThan(propBody, valConst), param)),
            "greaterthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThanOrEqual(propBody, valConst), param)),
            "lessthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThan(propBody, valConst), param)),
            "lessthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThanOrEqual(propBody, valConst), param)),
            "between" when values.Count > 1 && TryParseInstant(values[1], out var val2) =>
                query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(propBody, valConst),
                        Expression.LessThanOrEqual(propBody, Expression.Constant(val2))),
                    param)),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyNullableInstantFilter(
        IQueryable<WorkItem> query,
        Expression<Func<WorkItem, Instant?>> property,
        string? op,
        List<object?> values)
    {
        var param = property.Parameters[0];
        var propBody = property.Body;

        if (op is "isnull")
            return query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, Expression.Constant(null, typeof(Instant?))), param));
        if (op is "isnotnull")
            return query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, Expression.Constant(null, typeof(Instant?))), param));

        if (values.Count == 0) return query;
        if (!TryParseInstant(values[0], out var val1)) return query;

        var valConst = Expression.Constant((Instant?)val1, typeof(Instant?));

        return op switch
        {
            "equals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, valConst), param)),
            "notequals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, valConst), param)),
            "greaterthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThan(propBody, valConst), param)),
            "greaterthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThanOrEqual(propBody, valConst), param)),
            "lessthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThan(propBody, valConst), param)),
            "lessthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThanOrEqual(propBody, valConst), param)),
            "between" when values.Count > 1 && TryParseInstant(values[1], out var val2) =>
                query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(propBody, valConst),
                        Expression.LessThanOrEqual(propBody, Expression.Constant((Instant?)val2, typeof(Instant?)))),
                    param)),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyNullableIntFilter(
        IQueryable<WorkItem> query,
        Expression<Func<WorkItem, int?>> property,
        string? op,
        List<object?> values)
    {
        var param = property.Parameters[0];
        var propBody = property.Body;

        if (op is "isnull")
            return query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, Expression.Constant(null, typeof(int?))), param));
        if (op is "isnotnull")
            return query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, Expression.Constant(null, typeof(int?))), param));

        if (values.Count == 0) return query;
        if (!TryParseInt(values[0], out var val1)) return query;

        var valConst = Expression.Constant((int?)val1, typeof(int?));

        return op switch
        {
            "equals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, valConst), param)),
            "notequals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, valConst), param)),
            "greaterthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThan(propBody, valConst), param)),
            "greaterthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThanOrEqual(propBody, valConst), param)),
            "lessthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThan(propBody, valConst), param)),
            "lessthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThanOrEqual(propBody, valConst), param)),
            "between" when values.Count > 1 && TryParseInt(values[1], out var val2) =>
                query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(propBody, valConst),
                        Expression.LessThanOrEqual(propBody, Expression.Constant((int?)val2, typeof(int?)))),
                    param)),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyDoubleFilter(
        IQueryable<WorkItem> query,
        Expression<Func<WorkItem, double>> property,
        string? op,
        List<object?> values)
    {
        if (values.Count == 0 && op is not "isnull" and not "isnotnull") return query;
        if (op is "isnull" or "isnotnull") return query;

        if (!TryParseDouble(values[0], out var val1)) return query;

        var param = property.Parameters[0];
        var propBody = property.Body;
        var valConst = Expression.Constant(val1);

        return op switch
        {
            "equals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, valConst), param)),
            "notequals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, valConst), param)),
            "greaterthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThan(propBody, valConst), param)),
            "greaterthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThanOrEqual(propBody, valConst), param)),
            "lessthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThan(propBody, valConst), param)),
            "lessthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThanOrEqual(propBody, valConst), param)),
            "between" when values.Count > 1 && TryParseDouble(values[1], out var val2) =>
                query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(propBody, valConst),
                        Expression.LessThanOrEqual(propBody, Expression.Constant(val2))),
                    param)),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyNullableDoubleFilter(
        IQueryable<WorkItem> query,
        Expression<Func<WorkItem, double?>> property,
        string? op,
        List<object?> values)
    {
        var param = property.Parameters[0];
        var propBody = property.Body;

        if (op is "isnull")
            return query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, Expression.Constant(null, typeof(double?))), param));
        if (op is "isnotnull")
            return query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, Expression.Constant(null, typeof(double?))), param));

        if (values.Count == 0) return query;
        if (!TryParseDouble(values[0], out var val1)) return query;

        var valConst = Expression.Constant((double?)val1, typeof(double?));

        return op switch
        {
            "equals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.Equal(propBody, valConst), param)),
            "notequals" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.NotEqual(propBody, valConst), param)),
            "greaterthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThan(propBody, valConst), param)),
            "greaterthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.GreaterThanOrEqual(propBody, valConst), param)),
            "lessthan" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThan(propBody, valConst), param)),
            "lessthanorequal" => query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                Expression.LessThanOrEqual(propBody, valConst), param)),
            "between" when values.Count > 1 && TryParseDouble(values[1], out var val2) =>
                query.Where(Expression.Lambda<Func<WorkItem, bool>>(
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(propBody, valConst),
                        Expression.LessThanOrEqual(propBody, Expression.Constant((double?)val2, typeof(double?)))),
                    param)),
            _ => query
        };
    }

    private static IQueryable<WorkItem> ApplyStatusCategoryFilter(
        IQueryable<WorkItem> query,
        string? op,
        List<object?> values)
    {
        var parsed = values
            .Select(v => Enum.TryParse<WorkStatusCategory>(v?.ToString(), true, out var e) ? (WorkStatusCategory?)e : null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return op switch
        {
            "equals" when parsed.Count > 0 => query.Where(w => w.StatusCategory == parsed[0]),
            "notequals" when parsed.Count > 0 => query.Where(w => w.StatusCategory != parsed[0]),
            "in" when parsed.Count > 0 => query.Where(w => parsed.Contains(w.StatusCategory)),
            "notin" when parsed.Count > 0 => query.Where(w => !parsed.Contains(w.StatusCategory)),
            _ => query
        };
    }

    #endregion

    #region Sort

    private static IQueryable<WorkItem> ApplySort(
        IQueryable<WorkItem> query,
        List<AnalyticsViewSortDefinition> sortDefinitions)
    {
        IOrderedQueryable<WorkItem>? ordered = null;

        foreach (var sort in sortDefinitions)
        {
            var descending = string.Equals(sort.Direction, "Desc", StringComparison.OrdinalIgnoreCase);
            ordered = ApplySortField(query, ordered, sort.Field, descending);
        }

        return ordered ?? query;
    }

    private static IOrderedQueryable<WorkItem> ApplySortField(
        IQueryable<WorkItem> query,
        IOrderedQueryable<WorkItem>? ordered,
        string field,
        bool descending)
    {
        return field.ToLowerInvariant() switch
        {
            "id" => OrderBy(query, ordered, w => w.Id, descending),
            "key" => OrderBy(query, ordered, w => (string)w.Key, descending),
            "title" => OrderBy(query, ordered, w => w.Title, descending),
            "workspace.key" => OrderBy(query, ordered, w => (string)w.Workspace.Key, descending),
            "workspace.name" => OrderBy(query, ordered, w => w.Workspace.Name, descending),
            "type.name" => OrderBy(query, ordered, w => w.Type.Name, descending),
            "status.name" => OrderBy(query, ordered, w => w.Status.Name, descending),
            "statuscategory" => OrderBy(query, ordered, w => w.StatusCategory, descending),
            "createddate" => OrderBy(query, ordered, w => w.Created, descending),
            "changeddate" => OrderBy(query, ordered, w => w.LastModified, descending),
            "donedate" => OrderBy(query, ordered, w => w.DoneTimestamp, descending),
            "priority" => OrderBy(query, ordered, w => w.Priority, descending),
            "stackrank" => OrderBy(query, ordered, w => w.StackRank, descending),
            "storypoints" => OrderBy(query, ordered, w => w.StoryPoints, descending),
            _ => ordered ?? query.OrderBy(w => w.Id)
        };
    }

    private static IOrderedQueryable<WorkItem> OrderBy<TKey>(
        IQueryable<WorkItem> query,
        IOrderedQueryable<WorkItem>? ordered,
        Expression<Func<WorkItem, TKey>> keySelector,
        bool descending)
    {
        if (ordered is not null)
            return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);

        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

    #endregion

    #region Helpers

    private static Expression<Func<WorkItem, bool>> Combine(
        Expression<Func<WorkItem, string?>> propertySelector,
        Expression<Func<string?, bool>> predicate)
    {
        var param = propertySelector.Parameters[0];
        var propertyBody = propertySelector.Body;
        var predicateBody = new ParameterReplacer(predicate.Parameters[0], propertyBody).Visit(predicate.Body);
        return Expression.Lambda<Func<WorkItem, bool>>(predicateBody, param);
    }

    private static bool TryParseInstant(object? value, out Instant result)
    {
        result = default;
        if (value is null) return false;
        if (DateTimeOffset.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            result = Instant.FromDateTimeOffset(dto);
            return true;
        }
        return false;
    }

    private static bool TryParseInt(object? value, out int result)
    {
        result = default;
        if (value is null) return false;
        if (value is JsonElement je && je.ValueKind == JsonValueKind.Number)
            return je.TryGetInt32(out result);
        return int.TryParse(value.ToString(), CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDouble(object? value, out double result)
    {
        result = default;
        if (value is null) return false;
        if (value is JsonElement je && je.ValueKind == JsonValueKind.Number)
            return je.TryGetDouble(out result);
        return double.TryParse(value.ToString(), CultureInfo.InvariantCulture, out result);
    }

    private sealed class ParameterReplacer(Expression oldParam, Expression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }

    #endregion
}

internal sealed record WorkItemProjection(
    Guid Id,
    string Key,
    string Title,
    string WorkspaceKey,
    string WorkspaceName,
    string TypeName,
    string StatusName,
    WorkStatusCategory StatusCategory,
    DateTimeOffset CreatedDate,
    DateTimeOffset ChangedDate,
    DateTimeOffset? DoneDate,
    int? Priority,
    double StackRank,
    double? StoryPoints);

#region Definition DTOs

internal sealed class AnalyticsViewDefinition
{
    public int Version { get; set; }
    public List<AnalyticsViewColumnDefinition>? Columns { get; set; }
    public List<AnalyticsViewFilterDefinition>? Filters { get; set; }
    public List<string>? GroupBy { get; set; }
    public List<AnalyticsViewMeasureDefinition>? Measures { get; set; }
    public List<AnalyticsViewSortDefinition>? Sort { get; set; }
}

internal sealed class AnalyticsViewColumnDefinition
{
    public string Field { get; set; } = default!;
    public string? Alias { get; set; }
}

internal sealed class AnalyticsViewFilterDefinition
{
    public string Field { get; set; } = default!;
    public string? Operator { get; set; }
    public List<object?>? Values { get; set; }
}

internal sealed class AnalyticsViewMeasureDefinition
{
    public string Type { get; set; } = default!;
    public string? Field { get; set; }
    public string? Alias { get; set; }
    public double? Percentile { get; set; }
}

internal sealed class AnalyticsViewSortDefinition
{
    public string Field { get; set; } = default!;
    public string Direction { get; set; } = "Asc";
}

#endregion
