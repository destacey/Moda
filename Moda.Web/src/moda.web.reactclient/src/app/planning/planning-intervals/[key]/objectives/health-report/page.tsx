'use client'

import PageTitle from '@/src/components/common/page-title'
import { use, useCallback, useMemo } from 'react'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import {
  MarkdownCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
  HealthCheckStatusCellRenderer,
  HealthCheckStatusColumn,
  NestedTeamNameLinkCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'
import dayjs from 'dayjs'
import { ModaGrid } from '@/src/components/common'
import { Progress } from 'antd'
import { useGetPlanningIntervalObjectivesHealthReportQuery } from '@/src/store/features/planning/planning-interval-api'

const LocalHealthCheckCellRenderer = ({ data }) => {
  if (!data.healthCheckId) return null
  const healthCheck: HealthCheckStatusColumn = {
    id: data.healthCheckId,
    status: data.healthStatus,
    expiration: data.expiration,
  }
  return HealthCheckStatusCellRenderer({ data: healthCheck })
}

const ProgressCellRenderer = ({ value, data }) => {
  const progressStatus = ['Canceled', 'Missed'].includes(data.status?.name)
    ? 'exception'
    : undefined
  return <Progress percent={value} size="small" status={progressStatus} />
}

const ObjectiveHealthReportPage = (props: {
  params: Promise<{ key: number }>
}) => {
  const { key: piKey } = use(props.params)

  useDocumentTitle('PI Objectives Health Report')

  const {
    data: healthReport,
    isLoading,
    refetch,
  } = useGetPlanningIntervalObjectivesHealthReportQuery({
    planningIntervalKey: piKey,
  })

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
      { field: 'progress', width: 250, cellRenderer: ProgressCellRenderer },
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

  if (!isLoading && !healthReport) {
    return notFound()
  }

  return (
    <>
      <PageTitle title="PI Objectives Health Report" />
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        columnDefs={columnDefs}
        rowData={healthReport}
        loading={isLoading}
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
