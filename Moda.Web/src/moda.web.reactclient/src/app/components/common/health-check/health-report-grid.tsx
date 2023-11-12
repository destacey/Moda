'use client'

import { useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import Link from 'next/link'
import dayjs from 'dayjs'
import { useGetHealthReport } from '@/src/services/queries/health-check-queries'

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
  // TODO: redux is returning data for the previous objective
  //   const dispatch = useAppDispatch()
  //   const {
  //     data: healthReport,
  //     isLoading,
  //     error,
  //   } = useAppSelector(selectHealthReportContext)

  const {
    data: healthReport,
    isLoading,
    isFetching,
    refetch,
  } = useGetHealthReport(props?.objectId)

  const columnDefs = useMemo(
    () => [
      { field: 'id', hide: true },
      { field: 'status.name' },
      { field: 'reportedBy.name', cellRenderer: ReportedByLinkCellRenderer },
      {
        field: 'reportedOn',
        valueGetter: (params) =>
          dayjs(params.data.reportedOn).format('M/D/YYYY h:mm A'),
      },
      {
        field: 'Expiration',
        valueGetter: (params) =>
          dayjs(params.data.expiration).format('M/D/YYYY h:mm A'),
      },
      { field: 'note', width: 300 },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  //   const refresh = useCallback(async () => {
  //     dispatch(getHealthReport())
  //   }, [dispatch])

  //   useEffect(() => {
  //     dispatch(setHealthReportId(props.objectId))
  //     dispatch(getHealthReport())
  //   }, [dispatch, props.objectId])

  return (
    <>
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={healthReport}
        loadData={refresh}
        isDataLoading={isLoading}
      />
    </>
  )
}

export default HealthReportGrid
