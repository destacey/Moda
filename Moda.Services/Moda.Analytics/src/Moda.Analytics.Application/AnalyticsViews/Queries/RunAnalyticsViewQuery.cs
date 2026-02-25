using System.Globalization;
using System.Text.Json;
using Ardalis.GuardClauses;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.Persistence;

namespace Moda.Analytics.Application.AnalyticsViews.Queries;

public sealed record RunAnalyticsViewQuery(Guid Id, int PageNumber = 1, int PageSize = 50) : IQuery<Result<AnalyticsViewResultDto>>;

public sealed class RunAnalyticsViewQueryValidator : CustomValidator<RunAnalyticsViewQuery>
{
    public RunAnalyticsViewQueryValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.PageNumber)
            .GreaterThan(0);

        RuleFor(v => v.PageSize)
            .InclusiveBetween(1, 500);
    }
}

internal sealed class RunAnalyticsViewQueryHandler(
    IAnalyticsDbContext analyticsDbContext,
    ICurrentUser currentUser) : IQueryHandler<RunAnalyticsViewQuery, Result<AnalyticsViewResultDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, string> FieldTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = "guid",
        ["key"] = "string",
        ["title"] = "string",
        ["workspace.key"] = "string",
        ["workspace.name"] = "string",
        ["type.name"] = "string",
        ["status.name"] = "string",
        ["statusCategory"] = "string",
        ["createdDate"] = "datetime",
        ["changedDate"] = "datetime",
        ["doneDate"] = "datetime",
        ["priority"] = "number",
        ["stackRank"] = "number",
        ["storyPoints"] = "number"
    };

    private static readonly HashSet<string> NumericFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "priority",
        "stackRank",
        "storyPoints"
    };

    private static readonly HashSet<string> DateFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdDate",
        "changedDate",
        "doneDate"
    };

    private readonly IAnalyticsDbContext _analyticsDbContext = analyticsDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<Result<AnalyticsViewResultDto>> Handle(RunAnalyticsViewQuery request, CancellationToken cancellationToken)
    {
        var view = await _analyticsDbContext.AnalyticsViews
            .AsNoTracking()
            .Include(v => v.AnalyticsViewManagers)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (view is null)
            return Result.Failure<AnalyticsViewResultDto>("Analytics view not found.");

        if (view.Visibility != Visibility.Public && !view.AnalyticsViewManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            return Result.Failure<AnalyticsViewResultDto>("You do not have permission to run this analytics view.");

        if (view.Dataset != AnalyticsDataset.WorkItems)
            return Result.Failure<AnalyticsViewResultDto>($"Dataset {view.Dataset} is not supported yet.");

        AnalyticsViewDefinition definition;
        try
        {
            definition = JsonSerializer.Deserialize<AnalyticsViewDefinition>(view.DefinitionJson, JsonOptions) ?? new AnalyticsViewDefinition();
        }
        catch (Exception)
        {
            return Result.Failure<AnalyticsViewResultDto>("DefinitionJson is invalid JSON.");
        }

        var selectedColumns = (definition.Columns?.Count > 0
            ? definition.Columns
            : [new AnalyticsViewColumnDefinition("id", "Id"), new AnalyticsViewColumnDefinition("key", "Key"), new AnalyticsViewColumnDefinition("title", "Title")])
            .ToList();

        var groupBy = definition.GroupBy ?? [];
        var measures = definition.Measures ?? [];
        var hasGrouping = groupBy.Count > 0 || measures.Count > 0;
        var effectiveMeasures = measures.Count > 0
            ? measures
            : [new AnalyticsViewMeasureDefinition { Type = "Count", Field = "id", Alias = "Count" }];

        var invalidField = selectedColumns.Select(c => c.Field)
            .Concat(definition.Filters?.Select(f => f.Field) ?? [])
            .Concat(groupBy)
            .Concat(measures.Where(m => !string.IsNullOrWhiteSpace(m.Field)).Select(m => m.Field!))
            .FirstOrDefault(f => !IsValidFieldToken(f));

        if (invalidField is not null)
            return Result.Failure<AnalyticsViewResultDto>($"Field '{invalidField}' is not supported for WorkItems analytics.");

        var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in selectedColumns.Select(c => c.Field))
        {
            validSortFields.Add(field);
        }
        foreach (var field in groupBy)
        {
            validSortFields.Add(field);
        }
        foreach (var field in FieldTypes.Keys)
        {
            validSortFields.Add(field);
        }
        foreach (var col in selectedColumns.Where(c => !string.IsNullOrWhiteSpace(c.Alias)))
        {
            validSortFields.Add(col.Alias!);
        }
        foreach (var measure in effectiveMeasures)
        {
            validSortFields.Add(GetMeasureDisplayName(measure));
        }

        var invalidSort = definition.Sort?.Select(s => s.Field).FirstOrDefault(f => !validSortFields.Contains(f));
        if (invalidSort is not null)
            return Result.Failure<AnalyticsViewResultDto>($"Sort field '{invalidSort}' is not supported for WorkItems analytics.");

        var invalidMeasure = measures
            .FirstOrDefault(m => !IsSupportedMeasureType(m.Type));
        if (invalidMeasure is not null)
            return Result.Failure<AnalyticsViewResultDto>($"Measure type '{invalidMeasure.Type}' is not supported. Supported types: Count, Sum, Avg.");

        var invalidMeasureField = effectiveMeasures.FirstOrDefault(m =>
            IsNumericMeasureType(m.Type)
            && (string.IsNullOrWhiteSpace(m.Field) || !NumericFields.Contains(GetBaseFieldName(m.Field!))));
        if (invalidMeasureField is not null)
            return Result.Failure<AnalyticsViewResultDto>("Numeric measures require a numeric field. Supported numeric fields: priority, stackRank, storyPoints.");

        var invalidPercentile = effectiveMeasures.FirstOrDefault(m =>
            IsPercentileMeasureType(m.Type) && (!m.Percentile.HasValue || m.Percentile.Value < 0 || m.Percentile.Value > 100));
        if (invalidPercentile is not null)
            return Result.Failure<AnalyticsViewResultDto>("Percentile measures require a percentile value between 0 and 100.");

        var projectedRows = await _analyticsDbContext.WorkItems
            .AsNoTracking()
            .Select(w => new WorkItemAnalyticsProjection(
                w.Id,
                w.Key.Value,
                w.Title,
                w.Workspace.Key.Value,
                w.Workspace.Name,
                w.Type.Name,
                w.Status.Name,
                w.StatusCategory.ToString(),
                w.Created,
                w.LastModified,
                w.DoneTimestamp,
                w.Priority,
                w.StackRank,
                w.StoryPoints))
            .ToListAsync(cancellationToken);

        IEnumerable<WorkItemAnalyticsProjection> filteredRows = projectedRows;

        foreach (var filter in definition.Filters ?? [])
        {
            filteredRows = filteredRows.Where(r => EvaluateFilter(r, filter));
        }

        if (hasGrouping)
        {
            return RunGroupedResult(filteredRows, selectedColumns, groupBy, effectiveMeasures, definition.Sort, request);
        }

        IOrderedEnumerable<WorkItemAnalyticsProjection>? orderedRows = null;
        foreach (var sort in definition.Sort ?? [])
        {
            var sortField = ResolveSortField(sort.Field, selectedColumns);
            var descending = string.Equals(sort.Direction, "Desc", StringComparison.OrdinalIgnoreCase);
            if (orderedRows is null)
            {
                orderedRows = descending
                    ? filteredRows.OrderByDescending(r => GetComparableValue(r, sortField), AnalyticsValueComparer.Instance)
                    : filteredRows.OrderBy(r => GetComparableValue(r, sortField), AnalyticsValueComparer.Instance);
            }
            else
            {
                orderedRows = descending
                    ? orderedRows.ThenByDescending(r => GetComparableValue(r, sortField), AnalyticsValueComparer.Instance)
                    : orderedRows.ThenBy(r => GetComparableValue(r, sortField), AnalyticsValueComparer.Instance);
            }
        }

        var sortedRows = orderedRows ?? filteredRows.OrderBy(r => r.Id);
        var totalRows = sortedRows.Count();

        var pagedRows = sortedRows
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var columns = selectedColumns
            .Select(c => new AnalyticsViewResultColumnDto(
                c.Field,
                string.IsNullOrWhiteSpace(c.Alias) ? c.Field : c.Alias!,
                GetFieldType(c.Field)))
            .ToList();

        var rows = pagedRows
            .Select(r =>
            {
                var dict = new Dictionary<string, object?>();
                foreach (var col in selectedColumns)
                {
                    var key = string.IsNullOrWhiteSpace(col.Alias) ? col.Field : col.Alias!;
                    dict[key] = FormatValue(GetFieldValue(r, col.Field));
                }

                return dict;
            })
            .ToList();

        return Result.Success(new AnalyticsViewResultDto(columns, rows, totalRows));
    }

    private static Result<AnalyticsViewResultDto> RunGroupedResult(
        IEnumerable<WorkItemAnalyticsProjection> filteredRows,
        List<AnalyticsViewColumnDefinition> selectedColumns,
        List<string> groupBy,
        List<AnalyticsViewMeasureDefinition> effectiveMeasures,
        List<AnalyticsViewSortDefinition>? sortDefinitions,
        RunAnalyticsViewQuery request)
    {
        var rows = new List<Dictionary<string, object?>>();
        IEnumerable<IGrouping<string, WorkItemAnalyticsProjection>> groups;
        if (groupBy.Count > 0)
        {
            groups = filteredRows.GroupBy(r => BuildGroupKey(r, groupBy));
        }
        else
        {
            var materialized = filteredRows.ToList();
            groups = materialized.Count > 0
                ? [materialized.GroupBy(_ => string.Empty).First()]
                : [];
        }

        foreach (var group in groups)
        {
            var row = new Dictionary<string, object?>();
            var first = group.FirstOrDefault();
            if (first is null)
                continue;

            foreach (var field in groupBy)
            {
                var displayName = GetDisplayName(field, selectedColumns);
                row[displayName] = FormatValue(GetFieldValue(first, field));
            }

            foreach (var measure in effectiveMeasures)
            {
                var measureName = GetMeasureDisplayName(measure);
                row[measureName] = CalculateMeasure(group, measure);
            }

            rows.Add(row);
        }

        var orderedRows = ApplyRowSort(rows, sortDefinitions, selectedColumns);
        var totalRows = orderedRows.Count;
        var pagedRows = orderedRows
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var columns = new List<AnalyticsViewResultColumnDto>();
        foreach (var field in groupBy)
        {
            columns.Add(new AnalyticsViewResultColumnDto(
                field,
                GetDisplayName(field, selectedColumns),
                GetFieldType(field)));
        }

        foreach (var measure in effectiveMeasures)
        {
            columns.Add(new AnalyticsViewResultColumnDto(
                measure.Field ?? "id",
                GetMeasureDisplayName(measure),
                "number"));
        }

        return Result.Success(new AnalyticsViewResultDto(columns, pagedRows, totalRows));
    }

    private static bool EvaluateFilter(WorkItemAnalyticsProjection row, AnalyticsViewFilterDefinition filter)
    {
        var actual = GetFieldValue(row, filter.Field);
        var values = filter.Values ?? [];

        return filter.Operator?.Trim().ToLowerInvariant() switch
        {
            "equals" => values.Count > 0 && Compare(actual, values[0]) == 0,
            "notequals" => values.Count > 0 && Compare(actual, values[0]) != 0,
            "in" => values.Any(v => Compare(actual, v) == 0),
            "notin" => values.All(v => Compare(actual, v) != 0),
            "contains" => actual?.ToString()?.Contains(values.FirstOrDefault()?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) ?? false,
            "startswith" => actual?.ToString()?.StartsWith(values.FirstOrDefault()?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) ?? false,
            "endswith" => actual?.ToString()?.EndsWith(values.FirstOrDefault()?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) ?? false,
            "greaterthan" => values.Count > 0 && Compare(actual, values[0]) > 0,
            "greaterthanorequal" => values.Count > 0 && Compare(actual, values[0]) >= 0,
            "lessthan" => values.Count > 0 && Compare(actual, values[0]) < 0,
            "lessthanorequal" => values.Count > 0 && Compare(actual, values[0]) <= 0,
            "between" => values.Count > 1 && Compare(actual, values[0]) >= 0 && Compare(actual, values[1]) <= 0,
            "isnull" => actual is null,
            "isnotnull" => actual is not null,
            _ => false
        };
    }

    private static int Compare(object? left, object? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;

        if (left is DateOnly ld)
        {
            var rd = ParseDateOnly(right);
            return ld.CompareTo(rd);
        }

        if (left is Instant li)
        {
            var ri = ParseInstant(right);
            return li.CompareTo(ri);
        }

        if (left is Guid lg)
        {
            if (Guid.TryParse(right.ToString(), out var rg))
                return lg.CompareTo(rg);
            return string.Compare(lg.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        if (left is string ls)
            return string.Compare(ls, right.ToString(), StringComparison.OrdinalIgnoreCase);

        if (left is IComparable comparable)
        {
            try
            {
                var converted = Convert.ChangeType(right, left.GetType(), CultureInfo.InvariantCulture);
                return comparable.CompareTo(converted);
            }
            catch
            {
                return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static Instant ParseInstant(object value)
    {
        if (value is Instant instant) return instant;
        if (value is DateTime dt) return Instant.FromDateTimeUtc(dt.ToUniversalTime());
        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
            return Instant.FromDateTimeUtc(parsed.ToUniversalTime());

        throw new ArgumentException("Invalid date value for analytics filter.");
    }

    private static DateOnly ParseDateOnly(object value)
    {
        if (value is DateOnly d)
            return d;
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);
        if (DateOnly.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;
        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedDateTime))
            return DateOnly.FromDateTime(parsedDateTime);

        throw new ArgumentException("Invalid date value for analytics filter.");
    }

    private static object? GetFieldValue(WorkItemAnalyticsProjection row, string field)
    {
        var (baseField, timeGrain) = ParseFieldToken(field);
        object? value = baseField switch
        {
            "id" => row.Id,
            "key" => row.Key,
            "title" => row.Title,
            "workspace.key" => row.WorkspaceKey,
            "workspace.name" => row.WorkspaceName,
            "type.name" => row.TypeName,
            "status.name" => row.StatusName,
            "statusCategory" => row.StatusCategory,
            "createdDate" => row.CreatedDate,
            "changedDate" => row.ChangedDate,
            "doneDate" => row.DoneDate,
            "priority" => row.Priority,
            "stackRank" => row.StackRank,
            "storyPoints" => row.StoryPoints,
            _ => null
        };

        if (timeGrain is not null)
        {
            return ApplyTimeGrain(value, timeGrain);
        }

        return value;
    }

    private static object? GetComparableValue(WorkItemAnalyticsProjection row, string field)
        => GetFieldValue(row, field);

    private static string ResolveSortField(string sortField, List<AnalyticsViewColumnDefinition> selectedColumns)
    {
        var column = selectedColumns.FirstOrDefault(c => string.Equals(c.Alias, sortField, StringComparison.OrdinalIgnoreCase));
        return column?.Field ?? sortField;
    }

    private static (string BaseField, string? TimeGrain) ParseFieldToken(string token)
    {
        var value = token.Trim();
        var idx = value.LastIndexOf(':');
        if (idx <= 0 || idx == value.Length - 1)
            return (value, null);

        var baseField = value[..idx];
        var grain = value[(idx + 1)..];
        return (baseField, grain);
    }

    private static string GetBaseFieldName(string token)
        => ParseFieldToken(token).BaseField;

    private static bool IsValidFieldToken(string token)
    {
        var (baseField, grain) = ParseFieldToken(token);
        if (!FieldTypes.ContainsKey(baseField))
            return false;

        if (string.IsNullOrWhiteSpace(grain))
            return true;

        if (!DateFields.Contains(baseField))
            return false;

        return grain.Equals("Day", StringComparison.OrdinalIgnoreCase)
            || grain.Equals("Week", StringComparison.OrdinalIgnoreCase)
            || grain.Equals("Month", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFieldType(string token)
    {
        var (baseField, grain) = ParseFieldToken(token);
        if (!string.IsNullOrWhiteSpace(grain))
            return "date";

        return FieldTypes.TryGetValue(baseField, out var type)
            ? type
            : "string";
    }

    private static object? ApplyTimeGrain(object? value, string timeGrain)
    {
        DateOnly? date = value switch
        {
            Instant i => DateOnly.FromDateTime(i.ToDateTimeUtc()),
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            _ => (DateOnly?)null
        };

        if (!date.HasValue)
            return null;

        return timeGrain.Trim().ToLowerInvariant() switch
        {
            "day" => date.Value,
            "week" => StartOfWeek(date.Value),
            "month" => new DateOnly(date.Value.Year, date.Value.Month, 1),
            _ => date.Value
        };
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var offset = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Monday start
        return date.AddDays(-offset);
    }

    private static string BuildGroupKey(WorkItemAnalyticsProjection row, List<string> fields)
    {
        return string.Join(
            '\u001f',
            fields.Select(f => NormalizeGroupKeyValue(GetFieldValue(row, f))));
    }

    private static string NormalizeGroupKeyValue(object? value)
        => value switch
        {
            null => "null:",
            Instant instant => $"instant:{instant.ToUnixTimeTicks()}",
            DateOnly date => $"date:{date:yyyy-MM-dd}",
            Guid guid => $"guid:{guid}",
            _ => $"text:{value}"
        };

    private static string GetDisplayName(string field, List<AnalyticsViewColumnDefinition> selectedColumns)
    {
        var match = selectedColumns.FirstOrDefault(c => string.Equals(c.Field, field, StringComparison.OrdinalIgnoreCase));
        if (match is null || string.IsNullOrWhiteSpace(match.Alias))
            return field;

        return match.Alias!;
    }

    private static string GetMeasureDisplayName(AnalyticsViewMeasureDefinition measure)
    {
        if (!string.IsNullOrWhiteSpace(measure.Alias))
            return measure.Alias!;

        if (string.IsNullOrWhiteSpace(measure.Field))
            return measure.Type;

        return $"{measure.Type}_{measure.Field}";
    }

    private static object? CalculateMeasure(IGrouping<string, WorkItemAnalyticsProjection> group, AnalyticsViewMeasureDefinition measure)
    {
        var normalizedType = (measure.Type ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedType is "count")
        {
            if (string.IsNullOrWhiteSpace(measure.Field))
                return group.Count();

            return group.Count(item => GetFieldValue(item, measure.Field!) is not null);
        }

        if (!IsNumericMeasureType(measure.Type) || string.IsNullOrWhiteSpace(measure.Field))
            return null;

        var numericValues = group
            .Select(item => ToDouble(GetFieldValue(item, measure.Field!)))
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (numericValues.Count == 0)
            return null;

        return normalizedType switch
        {
            "sum" => numericValues.Sum(),
            "avg" or "average" => numericValues.Average(),
            "median" => CalculatePercentile(numericValues, 50),
            "percentile" => CalculatePercentile(numericValues, measure.Percentile ?? 50),
            _ => null
        };
    }

    private static List<Dictionary<string, object?>> ApplyRowSort(
        List<Dictionary<string, object?>> rows,
        List<AnalyticsViewSortDefinition>? sortDefinitions,
        List<AnalyticsViewColumnDefinition> selectedColumns)
    {
        IOrderedEnumerable<Dictionary<string, object?>>? orderedRows = null;
        foreach (var sort in sortDefinitions ?? [])
        {
            var descending = string.Equals(sort.Direction, "Desc", StringComparison.OrdinalIgnoreCase);
            var sortKey = sort.Field;
            if (!rows.Any(r => r.ContainsKey(sortKey)))
            {
                sortKey = GetDisplayName(sort.Field, selectedColumns);
            }

            if (orderedRows is null)
            {
                orderedRows = descending
                    ? rows.OrderByDescending(r => r.TryGetValue(sortKey, out var v) ? v : null, AnalyticsValueComparer.Instance)
                    : rows.OrderBy(r => r.TryGetValue(sortKey, out var v) ? v : null, AnalyticsValueComparer.Instance);
            }
            else
            {
                orderedRows = descending
                    ? orderedRows.ThenByDescending(r => r.TryGetValue(sortKey, out var v) ? v : null, AnalyticsValueComparer.Instance)
                    : orderedRows.ThenBy(r => r.TryGetValue(sortKey, out var v) ? v : null, AnalyticsValueComparer.Instance);
            }
        }

        return orderedRows?.ToList() ?? rows;
    }

    private static object? FormatValue(object? value)
        => value switch
        {
            null => null,
            Instant instant => instant.ToDateTimeUtc().ToString("O", CultureInfo.InvariantCulture),
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => value
        };

    private static bool IsSupportedMeasureType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        var normalized = type.Trim().ToLowerInvariant();
        return normalized is "count" or "sum" or "avg" or "average" or "median" or "percentile";
    }

    private static bool IsNumericMeasureType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        var normalized = type.Trim().ToLowerInvariant();
        return normalized is "sum" or "avg" or "average" or "median" or "percentile";
    }

    private static bool IsPercentileMeasureType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        return type.Trim().Equals("percentile", StringComparison.OrdinalIgnoreCase);
    }

    private static double CalculatePercentile(List<double> values, double percentile)
    {
        if (values.Count == 0)
            return 0;

        var sorted = values.OrderBy(v => v).ToList();
        var p = Math.Clamp(percentile, 0, 100) / 100d;
        if (sorted.Count == 1)
            return sorted[0];

        var rank = p * (sorted.Count - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);
        if (lower == upper)
            return sorted[lower];

        var weight = rank - lower;
        return sorted[lower] + ((sorted[upper] - sorted[lower]) * weight);
    }

    private static double? ToDouble(object? value)
    {
        if (value is null)
            return null;

        if (value is int i)
            return i;
        if (value is long l)
            return l;
        if (value is float f)
            return f;
        if (value is double d)
            return d;
        if (value is decimal m)
            return (double)m;

        return double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private sealed record WorkItemAnalyticsProjection(
        Guid Id,
        string Key,
        string Title,
        string WorkspaceKey,
        string WorkspaceName,
        string TypeName,
        string StatusName,
        string StatusCategory,
        Instant CreatedDate,
        Instant ChangedDate,
        Instant? DoneDate,
        int? Priority,
        double StackRank,
        double? StoryPoints);

    private sealed class AnalyticsValueComparer : IComparer<object?>
    {
        public static readonly AnalyticsValueComparer Instance = new();

        public int Compare(object? x, object? y) => RunAnalyticsViewQueryHandler.Compare(x, y);
    }

    private sealed record AnalyticsViewDefinition
    {
        public int Version { get; init; }
        public string? Dataset { get; init; }
        public List<AnalyticsViewColumnDefinition>? Columns { get; init; }
        public List<AnalyticsViewFilterDefinition>? Filters { get; init; }
        public List<string>? GroupBy { get; init; }
        public List<AnalyticsViewMeasureDefinition>? Measures { get; init; }
        public List<AnalyticsViewSortDefinition>? Sort { get; init; }
    }

    private sealed record AnalyticsViewColumnDefinition(string Field, string? Alias = null);

    private sealed record AnalyticsViewFilterDefinition
    {
        public string Field { get; init; } = default!;
        public string Operator { get; init; } = default!;
        public List<object?>? Values { get; init; }
    }

    private sealed record AnalyticsViewSortDefinition
    {
        public string Field { get; init; } = default!;
        public string Direction { get; init; } = "Asc";
    }

    private sealed record AnalyticsViewMeasureDefinition
    {
        public string Type { get; init; } = "Count";
        public string? Field { get; init; }
        public string? Alias { get; init; }
        public double? Percentile { get; init; }
    }
}
