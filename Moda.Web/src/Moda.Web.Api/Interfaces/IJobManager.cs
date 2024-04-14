﻿using Moda.Common.Application.Enums;

namespace Moda.Web.Api.Interfaces;

public interface IJobManager
{
    Task RunSyncExternalEmployees(CancellationToken cancellationToken);
    Task RunSyncAzureDevOpsBoards(SyncType syncType, CancellationToken cancellationToken);
}
