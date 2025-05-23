'use client'

import PageTitle from '@/src/components/common/page-title'
import { useCallback, useEffect, useMemo } from 'react'
import ModaGrid from '@/src/components/common/moda-grid'
import { authorizePage } from '@/src/components/hoc'
import Link from 'next/link'
import { useDocumentTitle } from '@/src/hooks'
import { useGetUsersQuery } from '@/src/store/features/user-management/users-api'

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

  const { data: usersData, isLoading, error, refetch } = useGetUsersQuery()

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

  useEffect(() => {
    error && console.error(error)
  }, [error])

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
