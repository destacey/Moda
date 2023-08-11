'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { authorizePage } from '../../components/hoc'
import { useGetRoles } from '@/src/services/query'
import Link from 'next/link'
import { Button } from 'antd'
import useAuth from '../../components/contexts/auth'
import CreateRoleForm from './create-role-form'
import { useRouter } from 'next/navigation'

const LinkCellRenderer = ({ value, data }) => {
  return <Link href={`roles/${data.id}`}>{value}</Link>
}

const RoleListPage = () => {
  const [showCreateRoleModal, setShowCreateRoleModal] = useState(false)
  const router = useRouter()
  const roleData = useGetRoles()

  const { hasClaim } = useAuth()
  const canCreateRole = hasClaim('Permission', 'Permissions.Roles.Create')

  const columnDefs = useMemo(
    () => [
      { field: 'name', cellRenderer: LinkCellRenderer },
      { field: 'description' },
    ],
    []
  )

  const getRoles = useCallback(async () => {
    roleData.refetch()
  }, [roleData])

  const Actions = () => {
    return (
      <>
        {canCreateRole && (
          <Button onClick={() => setShowCreateRoleModal(true)}>
            Create Role
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle title="Roles" actions={<Actions />} />

      <ModaGrid
        columnDefs={columnDefs}
        rowData={roleData.data}
        loadData={getRoles}
      />

      <CreateRoleForm
        showForm={showCreateRoleModal}
        roles={roleData.data}
        onFormCreate={(id: string) => {
          setShowCreateRoleModal(false)
          router.push(`/settings/roles/${id}`)
        }}
        onFormCancel={() => setShowCreateRoleModal(false)}
      />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  RoleListPage,
  'Permission',
  'Permissions.Roles.View'
)

export default PageWithAuthorization
