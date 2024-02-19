'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo } from 'react'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalObjectivesHealthReport,
} from '@/src/services/queries/planning-queries'
import { notFound } from 'next/navigation'
import {
  MarkdownCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
  HealthCheckStatusCellRenderer,
  HealthCheckStatusColumn,
  NestedTeamNameLinkCellRenderer,
} from '@/src/app/components/common/moda-grid-cell-renderers'
import dayjs from 'dayjs'
import { ModaGrid } from '@/src/app/components/common'

const LocalHealthCheckCellRenderer = ({ data }) => {
  if (!data.healthCheckId) return null
  const healthCheck: HealthCheckStatusColumn = {
    id: data.healthCheckId,
    status: data.healthStatus,
    expiration: data.expiration,
  }
  return HealthCheckStatusCellRenderer({ data: healthCheck })
}

const ObjectiveHealthReportPage = ({ params }) => {
  useDocumentTitle('PI Objectives Health Report')

  const {
    data: piData,
    isLoading: piIsLoading,
    isFetching: piIsFetching,
    refetch: refetchPlanningInterval,
  } = useGetPlanningIntervalByKey(params.key)

  const {
    data: healthReport,
    isLoading,
    isFetching,
    refetch,
  } = useGetPlanningIntervalObjectivesHealthReport(params.key, null, true)

  const columnDefs = useMemo(
    () => [
      { field: 'id', hide: true },
      { field: 'key', width: 90 },
      {
        field: 'name',
        width: 400,
        cellRenderer: PlanningIntervalObjectiveLinkCellRenderer,
      },
      { field: 'isStretch', width: 100 },
      {
        field: 'status.name',
        headerName: 'Status',
        width: 125,
      },
      {
        field: 'team.name',
        headerName: 'Team',
        cellRenderer: NestedTeamNameLinkCellRenderer,
        hide: false,
      },
      { field: 'healthCheckId', hide: true },
      {
        field: 'healthStatus.name',
        headerName: 'Health',
        width: 115,
        cellRenderer: LocalHealthCheckCellRenderer,
      },
      {
        field: 'note',
        width: 400,
        autoHeight: true,
        cellRenderer: MarkdownCellRenderer,
      },
      {
        field: 'reportedOn',
        valueGetter: (params) =>
          !params.data.reportedOn
            ? null
            : dayjs(params.data.reportedOn).format('M/D/YYYY h:mm A'),
      },
      {
        field: 'Expiration',
        valueGetter: (params) =>
          !params.data.expiration
            ? null
            : dayjs(params.data.expiration).format('M/D/YYYY h:mm A'),
      },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  if (!piIsLoading && !isLoading && !isFetching && !healthReport) {
    notFound()
  }

  return (
    <>
      <PageTitle title="PI Objectives Health Report" />
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        columnDefs={columnDefs}
        rowData={healthReport}
        loadData={refresh}
      />
    </>
  )
}

const ObjectiveHealthReportPageWithAuthorization = authorizePage(
  ObjectiveHealthReportPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default ObjectiveHealthReportPageWithAuthorization
