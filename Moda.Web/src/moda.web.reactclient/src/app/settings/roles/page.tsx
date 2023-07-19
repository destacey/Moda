'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { RoleListDto } from '@/src/services/moda-api'
import { getRolesClient } from '@/src/services/clients'
import { authorizePage } from '../../components/hoc'
import { useGetRoles } from '@/src/services/query'
import Link from 'next/link'
import { Button } from 'antd'
import useAuth from '../../components/contexts/auth'
import CreateRoleForm from './create-role-form'
import { useRouter } from 'next/navigation'

const LinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`roles/${data.id}`}>{value}</Link>
  )
}
const Page = () => {
  const columnDefs = useMemo(
    () => [
      { field: 'name', cellRenderer: LinkCellRenderer }, 
      { field: 'description' }],
    []
  )

  const getRoles = useCallback(async () => {
    roleData.refetch();
  }, [])

  const router = useRouter();
  const roleData = useGetRoles();
  const [showCreateRoleModal, setShowCreateRoleModal] = useState(false)

  const { hasClaim } = useAuth();
  const canCreateRole = hasClaim('Permission', 'Permissions.Roles.Create');
  
  const Actions = () => {
    return (
      <>
        { canCreateRole && (
          <Button onClick={() => setShowCreateRoleModal(true)}>
            Create Role
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle 
        title="Roles" 
        actions={<Actions />}/>

      <ModaGrid columnDefs={columnDefs} rowData={roleData.data} loadData={getRoles} />

      <CreateRoleForm
        showForm={showCreateRoleModal}
        roles={roleData.data}
        onFormCreate={(id: string) => {
          setShowCreateRoleModal(false);
          router.push(`/settings/roles/${id}`);
        }}
        onFormCancel={() => setShowCreateRoleModal(false)}
      />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  Page,
  'Permission',
  'Permissions.Roles.View'
)

export default PageWithAuthorization
