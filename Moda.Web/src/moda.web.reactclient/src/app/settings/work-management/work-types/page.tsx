'use client'

import { ModaGrid, PageTitle } from '@/src/app/components/common'
import {
  useAppDispatch,
  useAppSelector,
  useDocumentTitle,
} from '@/src/app/hooks'
import { WorkTypeDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { Space, Switch } from 'antd'
import { useCallback, useEffect, useMemo } from 'react'
import { setIncludeInactive } from '../../../../store/features/work-management/work-type-slice'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetWorkTypesQuery } from '@/src/store/features/work-management/work-type-api'

const WorkTypesPage = () => {
  useDocumentTitle('Work Management - Work Types')

  const { includeInactive } = useAppSelector((state) => state.workType)

  const {
    data: workTypes,
    isLoading,
    error,
    refetch,
  } = useGetWorkTypesQuery(includeInactive)
  const dispatch = useAppDispatch()

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const columnDefs = useMemo<ColDef<WorkTypeDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'name' },
      { field: 'description', width: 300 },
      { field: 'level' },
      { field: 'isActive', width: 100 }, // TODO: convert to yes/no
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onIncludeInactiveChange = (checked: boolean) => {
    dispatch(setIncludeInactive(checked))
    refresh()
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
      <PageTitle title="Work Types" />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={workTypes}
        loadData={refresh}
        isDataLoading={isLoading}
      />
    </>
  )
}

const WorkTypesPageWithAuthorization = authorizePage(
  WorkTypesPage,
  'Permission',
  'Permissions.WorkTypes.View',
)

export default WorkTypesPageWithAuthorization
