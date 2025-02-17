'use client'

import { ModaGrid, PageTitle } from '@/src/components/common'
import { useAppDispatch, useAppSelector, useDocumentTitle } from '@/src/hooks'
import { WorkTypeDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { Button } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { setIncludeInactive } from '../../../../store/features/work-management/work-type-slice'
import { authorizePage } from '@/src/components/hoc'
import { useGetWorkTypesQuery } from '@/src/store/features/work-management/work-type-api'
import useAuth from '@/src/components/contexts/auth'
import { EditOutlined } from '@ant-design/icons'
import EditWorkTypeForm from './_components/edit-work-type-form'
import Link from 'next/link'
import { ItemType } from 'antd/es/menu/interface'
import { ControlItemSwitch } from '@/src/components/common/control-items-menu'

const WorkTypesPage = () => {
  useDocumentTitle('Work Management - Work Types')
  const [openUpdateWorkTypeForm, setOpenUpdateWorkTypeForm] =
    useState<boolean>(false)
  const [editWorkTypeId, setEditWorkTypeId] = useState<number | null>(null)

  const { includeInactive } = useAppSelector((state) => state.workType)

  const {
    data: workTypes,
    isLoading,
    error,
    refetch,
  } = useGetWorkTypesQuery(includeInactive)
  const dispatch = useAppDispatch()

  const { hasClaim } = useAuth()
  const canUpdateWorkTypes = hasClaim(
    'Permission',
    'Permissions.WorkTypes.Update',
  )
  const canViewWorkTypeHierarchy = hasClaim(
    'Permission',
    'Permissions.WorkTypeLevels.View',
  )

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const editWorkTypeButtonClicked = (id: number) => {
    setEditWorkTypeId(id)
    setOpenUpdateWorkTypeForm(true)
  }

  const columnDefs = useMemo<ColDef<WorkTypeDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !canUpdateWorkTypes,
        cellRenderer: (params) => {
          return (
            canUpdateWorkTypes && (
              <Button
                type="text"
                size="small"
                icon={<EditOutlined />}
                onClick={() => editWorkTypeButtonClicked(params.data.id)}
              />
            )
          )
        },
      },
      { field: 'id', hide: true },
      { field: 'name' },
      { field: 'description', width: 300 },
      { field: 'level.name', headerName: 'Level' },
      { field: 'isActive', width: 100 }, // TODO: convert to yes/no
    ],
    [canUpdateWorkTypes],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const actions = () => {
    return (
      <>
        {canViewWorkTypeHierarchy && (
          <Link href="/settings/work-management/work-types/hierarchy">
            Work Type Hierarchy
          </Link>
        )}
      </>
    )
  }

  const onIncludeInactiveChange = (checked: boolean) => {
    dispatch(setIncludeInactive(checked))
    refresh()
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <ControlItemSwitch
          label="Include Inactive"
          checked={includeInactive}
          onChange={onIncludeInactiveChange}
        />
      ),
      key: 'include-inactive',
      onClick: () => onIncludeInactiveChange(!includeInactive),
    },
  ]

  const onEditWorkTypeFormClosed = (wasSaved: boolean) => {
    setOpenUpdateWorkTypeForm(false)
    setEditWorkTypeId(null)
    if (wasSaved) {
      refresh()
    }
  }

  return (
    <>
      <PageTitle title="Work Types" actions={actions()} />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={workTypes}
        loadData={refresh}
        loading={isLoading}
      />
      {openUpdateWorkTypeForm && (
        <EditWorkTypeForm
          showForm={openUpdateWorkTypeForm}
          workTypeId={editWorkTypeId}
          onFormSave={() => onEditWorkTypeFormClosed(true)}
          onFormCancel={() => onEditWorkTypeFormClosed(false)}
        />
      )}
    </>
  )
}

const WorkTypesPageWithAuthorization = authorizePage(
  WorkTypesPage,
  'Permission',
  'Permissions.WorkTypes.View',
)

export default WorkTypesPageWithAuthorization
