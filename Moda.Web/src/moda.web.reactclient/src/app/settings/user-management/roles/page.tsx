'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '@/src/app/components/common/moda-grid'
import { authorizePage } from '@/src/app/components/hoc'
import Link from 'next/link'
import { Button } from 'antd'
import useAuth from '@/src/app/components/contexts/auth'
import CreateRoleForm from './create-role-form'
import { useRouter } from 'next/navigation'
import { useGetRoles } from '@/src/services/queries/user-management-queries'
import { useDocumentTitle } from '@/src/app/hooks'

const LinkCellRenderer = ({ value, data }) => {
  return <Link href={`roles/${data.id}`}>{value}</Link>
}

const RoleListPage = () => {
  useDocumentTitle('Roles')
  const [openCreateRoleForm, setOpenCreateRoleForm] = useState(false)
  const router = useRouter()
  const { data: roleData, isLoading, refetch } = useGetRoles()

  const { hasClaim } = useAuth()
  const canCreateRole = hasClaim('Permission', 'Permissions.Roles.Create')

  const columnDefs = useMemo(
    () => [
      { field: 'name', cellRenderer: LinkCellRenderer },
      { field: 'description', width: 300 },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

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

      <ModaGrid
        columnDefs={columnDefs}
        rowData={roleData}
        loadData={refresh}
        isDataLoading={isLoading}
      />

      {openCreateRoleForm && (
        <CreateRoleForm
          showForm={openCreateRoleForm}
          roles={roleData}
          onFormCreate={(id: string) => {
            setOpenCreateRoleForm(false)
            router.push(`/settings/roles/${id}`)
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
