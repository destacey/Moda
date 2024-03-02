'use client'

import { ModaGrid, PageTitle } from '@/src/app/components/common'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useAppDispatch,
  useAppSelector,
  useDocumentTitle,
} from '@/src/app/hooks'
import { WorkProcessListDto } from '@/src/services/moda-api'
import { useGetWorkProcessesQuery } from '@/src/store/features/work-management/work-process-api'
import { setIncludeInactive } from '@/src/store/features/work-management/work-process-slice'
import { ColDef } from 'ag-grid-community'
import { Space, Switch } from 'antd'
import Link from 'next/link'
import { useCallback, useEffect, useMemo, useState } from 'react'

const WorkProcessLinkCellRenderer = ({ value, data }) => {
  return <Link href={`./work-processes/${data.key}`}>{value}</Link>
}

const WorkProcessesPage: React.FC = () => {
  useDocumentTitle('Work Management - Work Processes')

  const { includeInactive } = useAppSelector((state) => state.workProcess)

  const {
    data: workProcessesData,
    isLoading,
    error,
    refetch,
  } = useGetWorkProcessesQuery(includeInactive)
  const dispatch = useAppDispatch()

  const columnDefs = useMemo<ColDef<WorkProcessListDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'key', width: 80 },
      { field: 'name', width: 300, cellRenderer: WorkProcessLinkCellRenderer },
      { field: 'ownership.name', headerName: 'Ownership' },
      { field: 'isActive', width: 100 }, // TODO: convert to yes/no
    ],
    [],
  )

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onIncludeInactiveChange = (checked: boolean) => {
    dispatch(setIncludeInactive(checked))
  }

  const controlItems = [
    {
      label: (
        <Space>
          <Switch
            size="small"
            checked={includeInactive}
            onChange={onIncludeInactiveChange}
          />
          Include Disabled
        </Space>
      ),
      key: '0',
    },
  ]

  return (
    <>
      <PageTitle title="Work Processes" />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={workProcessesData}
        loadData={refresh}
        isDataLoading={isLoading}
      />
    </>
  )
}

const WorkProcessesPageWithAuthorization = authorizePage(
  WorkProcessesPage,
  'Permission',
  'Permissions.WorkProcesses.View',
)

export default WorkProcessesPageWithAuthorization
