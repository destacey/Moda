'use client'

import { ModaGrid, PageTitle } from '@/src/app/components/common'
import { useDocumentTitle } from '@/src/app/hooks'
import { WorkProcessListDto } from '@/src/services/moda-api'
import { useGetWorkProcesses } from '@/src/services/queries/work-management-queries'
import { ColDef } from 'ag-grid-community'
import { Space, Switch } from 'antd'
import { useCallback, useMemo, useState } from 'react'

const WorkTypesPage: React.FC = () => {
  useDocumentTitle('Work Management - Work Processes')
  const [includeDisabled, setIncludeDisabled] = useState(false)
  const { data, isLoading, refetch } = useGetWorkProcesses(includeDisabled)

  const columnDefs = useMemo<ColDef<WorkProcessListDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'key' },
      { field: 'name' },
      { field: 'ownership.name', headerName: 'Ownership' },
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
      <PageTitle title="Work Processes" />

      <ModaGrid
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
