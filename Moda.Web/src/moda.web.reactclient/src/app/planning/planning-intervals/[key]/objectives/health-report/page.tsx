'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useEffect, useMemo } from 'react'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalObjectivesHealthReport,
} from '@/src/services/queries/planning-queries'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import {
  MarkdownCellRenderer,

    TeamLinkCellRenderer,
  PlanningIntervalLinkCellRenderer,
  PlanningIntervalObjectiveLinkCellRenderer,
} from '@/src/app/components/common/moda-grid-cell-renderers'
import dayjs from 'dayjs'
import { ModaGrid } from '@/src/app/components/common'

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

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  useEffect(() => {
    if (!piData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/planning-intervals`,
        title: 'Planning Intervals',
      },
    ]

    breadcrumbRoute.push(
      {
        href: `/planning/planning-intervals/${piData.key}`,
        title: piData.name,
      },
      {
        title: 'Health Report',
      },
    )
    // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
    dispatch(
      setBreadcrumbRoute({
        pathname,
        route: breadcrumbRoute,
      }),
    )
  }, [dispatch, pathname, piData])

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
        field: 'planningInterval',
        valueFormatter: (params) => params.value.name,
        useValueFormatterForExport: true,
        cellRenderer: PlanningIntervalLinkCellRenderer,
        hide: true,
      },
      { field: 'status.name', headerName: 'Status', width: 125 },
      {
        field: 'team',
        valueFormatter: (params) => params.value.name,
        cellRenderer: TeamLinkCellRenderer,
        hide: false,
      },
      { field: 'healthCheckId', hide: true },
      {
        field: 'healthStatus.name',
        headerName: 'Health',
        width: 115,
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
      <PageTitle title={piData?.name} subtitle="PI Objectives Health Report" />
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
