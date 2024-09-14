'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo } from 'react'
import ModaGrid from '@/src/app/components/common/moda-grid'
import { authorizePage } from '@/src/app/components/hoc'
import Link from 'next/link'
import { useDocumentTitle } from '@/src/app/hooks'
import { useGetUsers } from '@/src/services/queries/user-management-queries'

const UserLinkCellRenderer = ({ value, data }) => {
  return <Link href={`users/${data.id}`}>{value}</Link>
}

const EmployeeLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.employee?.key}`}>{value}</Link>
  )
}

const UsersListPage = () => {
  useDocumentTitle('Users')

  const { data: usersData, isLoading, refetch } = useGetUsers()

  const columnDefs = useMemo(
    () => [
      { field: 'id', hide: true },
      { field: 'userName', cellRenderer: UserLinkCellRenderer },
      { field: 'firstName' },
      { field: 'lastName' },
      { field: 'email' },
      {
        field: 'employee.name',
        headerName: 'Employee',
        cellRenderer: EmployeeLinkCellRenderer,
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <PageTitle title="Users" />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={usersData}
        loadData={refresh}
        loading={isLoading}
      />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  UsersListPage,
  'Permission',
  'Permissions.Users.View',
)

export default PageWithAuthorization
