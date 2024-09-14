'use client'

import React, { useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import Link from 'next/link'
import dayjs from 'dayjs'
import { useGetHealthReport } from '@/src/services/queries/health-check-queries'
import {
  HealthCheckStatusCellRenderer,
  MarkdownCellRenderer,
} from '../moda-grid-cell-renderers'
import { HealthCheckDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'

interface HealthReportGridProps {
  objectId: string
}

const ReportedByLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.reportedBy?.key}`}>
      {value}
    </Link>
  )
}

const HealthReportGrid = (props: HealthReportGridProps) => {
  const {
    data: healthReport,
    isLoading,
    isFetching,
    refetch,
  } = useGetHealthReport(props?.objectId)

  const columnDefs = useMemo<ColDef<HealthCheckDto>[]>(
    () => [
      { field: 'id', hide: true },
      {
        field: 'status.name',
        headerName: 'Health',
        width: 115,
        cellRenderer: HealthCheckStatusCellRenderer,
      },
      {
        field: 'note',
        width: 400,
        autoHeight: true,
        cellRenderer: MarkdownCellRenderer,
      },
      {
        field: 'reportedBy.name',
        headerName: 'Reported By',
        cellRenderer: ReportedByLinkCellRenderer,
      },
      {
        field: 'reportedOn',
        valueGetter: (params) =>
          dayjs(params.data.reportedOn).format('M/D/YYYY h:mm A'),
      },
      {
        field: 'expiration',
        valueGetter: (params) =>
          dayjs(params.data.expiration).format('M/D/YYYY h:mm A'),
      },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={healthReport}
        loadData={refresh}
        loading={isLoading}
      />
    </>
  )
}

export default HealthReportGrid
