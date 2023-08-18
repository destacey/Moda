'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { authorizePage } from '../../components/hoc'
import Link from 'next/link'
import { Button } from 'antd'
import useAuth from '../../components/contexts/auth'
import CreateRoleForm from './create-role-form'
import { useRouter } from 'next/navigation'
import { useGetRoles } from '@/src/services/queries/user-management-queries'
import { RoleListDto } from '@/src/services/moda-api'

const LinkCellRenderer = ({ value, data }) => {
  return <Link href={`roles/${data.id}`}>{value}</Link>
}

const sortRoles = (data: RoleListDto[]) => {
  return data?.sort((a, b) => {
    return a.name.localeCompare(b.name)
  })
}

const RoleListPage = () => {
  const [showCreateRoleModal, setShowCreateRoleModal] = useState(false)
  const router = useRouter()
  const { data, refetch } = useGetRoles()

  const { hasClaim } = useAuth()
  const canCreateRole = hasClaim('Permission', 'Permissions.Roles.Create')

  const columnDefs = useMemo(
    () => [
      { field: 'name', cellRenderer: LinkCellRenderer },
      { field: 'description' },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

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
        rowData={sortRoles(data)}
        loadData={refresh}
      />

      <CreateRoleForm
        showForm={showCreateRoleModal}
        roles={data}
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
  'Permissions.Roles.View',
)

export default PageWithAuthorization
