'use client'

import PageTitle from '@/src/components/common/page-title'
import { useEffect, useMemo, useState } from 'react'
import WaydGrid from '@/src/components/common/wayd-grid'
import { authorizePage } from '@/src/components/hoc'
import Link from 'next/link'
import { Button } from 'antd'
import useAuth from '@/src/components/contexts/auth'
import { useRouter } from 'next/navigation'
import { useDocumentTitle } from '@/src/hooks'
import { useGetRolesQuery } from '@/src/store/features/user-management/roles-api'
import { CreateRoleForm } from './_components'
import { RoleListDto } from '@/src/services/wayd-api'
import { ICellRendererParams } from 'ag-grid-community'

const LinkCellRenderer = ({ value, data }: ICellRendererParams<RoleListDto>) => {
  return <Link href={`roles/${data!.id}`}>{value}</Link>
}

const RoleListPage = () => {
  useDocumentTitle('Roles')
  const [openCreateRoleForm, setOpenCreateRoleForm] = useState(false)
  const router = useRouter()

  const { data: roleData, isLoading, error, refetch } = useGetRolesQuery()

  const { hasClaim } = useAuth()
  const canCreateRole = hasClaim('Permission', 'Permissions.Roles.Create')

  const columnDefs = useMemo(() => [
      { field: 'name', cellRenderer: LinkCellRenderer },
      { field: 'description', width: 300 },
    ], [])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const refresh = async () => {
    refetch()
  }

  const actions = () => {
    return (
      <>
        {canCreateRole && (
          <Button onClick={() => setOpenCreateRoleForm(true)}>
            Create Role
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle title="Roles" actions={actions()} />

      <WaydGrid
        height={600}
        columnDefs={columnDefs}
        rowData={roleData}
        loadData={refresh}
        loading={isLoading}
      />

      {openCreateRoleForm && (
        <CreateRoleForm
          roles={roleData ?? []}
          onFormCreate={(id: string) => {
            setOpenCreateRoleForm(false)
            router.push(`/settings/user-management/roles/${id}`)
          }}
          onFormCancel={() => setOpenCreateRoleForm(false)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  RoleListPage,
  'Permission',
  'Permissions.Roles.View',
)

export default PageWithAuthorization
