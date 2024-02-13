'use client'

import { ModaGrid, PageTitle } from '@/src/app/components/common'
import { useDocumentTitle } from '@/src/app/hooks'
import { WorkTypeDto } from '@/src/services/moda-api'
import { useGetWorkTypes } from '@/src/services/queries/work-management-queries'
import { ColDef } from 'ag-grid-community'
import { Space, Switch } from 'antd'
import { useCallback, useMemo, useState } from 'react'

const WorkTypesPage: React.FC = () => {
  useDocumentTitle('Work Management - Work Types')
  const [includeDisabled, setIncludeDisabled] = useState(false)
  const { data, isLoading, refetch } = useGetWorkTypes(includeDisabled)

  const columnDefs = useMemo<ColDef<WorkTypeDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'name' },
      { field: 'description', width: 300 },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onIncludeDisabledChange = (checked: boolean) => {
    setIncludeDisabled(checked)
  }

  const controlItems = [
    {
      label: (
        <Space>
          <Switch
            size="small"
            checked={includeDisabled}
            onChange={onIncludeDisabledChange}
          />
          Include Disabled
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
        rowData={data}
        loadData={refresh}
        isDataLoading={isLoading}
      />
    </>
  )
}

export default WorkTypesPage
