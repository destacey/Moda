'use client'

import { ModaGrid, PageTitle } from '@/src/app/components/common'
import {
  useAppDispatch,
  useAppSelector,
  useDocumentTitle,
} from '@/src/app/hooks'
import { WorkStatusDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { Space, Switch } from 'antd'
import { useCallback, useEffect, useMemo } from 'react'
import { fetchWorkStatuses, setIncludeInactive } from '../work-status-slice'
import { authorizePage } from '@/src/app/components/hoc'

const WorkStatusesPage = () => {
  useDocumentTitle('Work Management - Work Statuses')

  const { workStatuses, isLoading, error, includeInactive } = useAppSelector(
    (state) => state.workStatus,
  )
  const dispatch = useAppDispatch()

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const columnDefs = useMemo<ColDef<WorkStatusDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'name' },
      { field: 'description', width: 300 },
      { field: 'isActive', width: 100 }, // TODO: convert to yes/no
    ],
    [],
  )

  const refresh = useCallback(async () => {
    dispatch(fetchWorkStatuses(includeInactive))
  }, [dispatch, includeInactive])

  const onIncludeInactiveChange = (checked: boolean) => {
    dispatch(setIncludeInactive(checked))
    dispatch(fetchWorkStatuses(checked))
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
          Include Inactive
        </Space>
      ),
      key: '0',
    },
  ]

  return (
    <>
      <PageTitle title="Work Statuses" />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={workStatuses}
        loadData={refresh}
        isDataLoading={isLoading}
      />
    </>
  )
}

const WorkStatusesPageWithAuthorization = authorizePage(
  WorkStatusesPage,
  'Permission',
  'Permissions.WorkStatuses.View',
)

export default WorkStatusesPageWithAuthorization
