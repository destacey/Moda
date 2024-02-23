'use client'

import { ModaGrid, PageTitle } from '@/src/app/components/common'
import { authorizePage } from '@/src/app/components/hoc'
import { useDocumentTitle } from '@/src/app/hooks'
import { WorkProcessListDto } from '@/src/services/moda-api'
import { useGetWorkProcesses } from '@/src/services/queries/work-management-queries'
import { ColDef } from 'ag-grid-community'
import { Space, Switch } from 'antd'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'

const WorkProcessLinkCellRenderer = ({ value, data }) => {
  return <Link href={`./work-processes/${data.key}`}>{value}</Link>
}

const WorkProcessesPage: React.FC = () => {
  useDocumentTitle('Work Management - Work Processes')
  const [includeDisabled, setIncludeDisabled] = useState(true)
  const { data, isLoading, refetch } = useGetWorkProcesses(includeDisabled)

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

const WorkProcessesPageWithAuthorization = authorizePage(
  WorkProcessesPage,
  'Permission',
  'Permissions.WorkProcesses.View',
)

export default WorkProcessesPageWithAuthorization
