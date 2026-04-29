'use client'

import PageTitle from '@/src/components/common/page-title'
import { use, useMemo } from 'react'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import {
  MarkdownCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
  NestedTeamNameLinkCellRenderer,
} from '@/src/components/common/wayd-grid-cell-renderers'
import HealthCheckTag from '@/src/components/common/health-check/health-check-tag'
import dayjs from 'dayjs'
import { WaydGrid } from '@/src/components/common'
import { Progress } from 'antd'
import { useGetPlanningIntervalObjectivesHealthReportQuery } from '@/src/store/features/planning/planning-interval-api'

const LocalHealthCheckCellRenderer = (params) => {
  if (!params.data?.healthCheckId) return null
  return (
    <HealthCheckTag
      healthCheck={{
        id: params.data.healthCheckId,
        status: params.data.healthStatus,
        reportedOn: params.data.reportedOn,
        expiration: params.data.expiration,
        note: params.data.note,
      }}
      planningIntervalId={params.data.planningInterval?.id}
      objectiveId={params.data.id}
    />
  )
}

const ProgressCellRenderer = ({ value, data }) => {
  const progressStatus = ['Canceled', 'Missed'].includes(data.status?.name)
    ? 'exception'
    : undefined
  return <Progress percent={value} size="small" status={progressStatus} />
}

const ObjectiveHealthReportPage = (props: {
  params: Promise<{ key: string }>
}) => {
  const { key } = use(props.params)
  const piKey = Number(key)

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

  const refresh = async () => {
    refetch()
  }

  if (!isLoading && !healthReport) {
    return notFound()
  }

  return (
    <>
      <PageTitle title="PI Objectives Health Report" />
      {/* TODO:  setup dynamic height */}
      <WaydGrid
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
