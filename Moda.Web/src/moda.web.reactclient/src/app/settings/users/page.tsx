'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { authorizePage } from '../../components/hoc'
import Link from 'next/link'
import { useDocumentTitle } from '../../hooks'
import { useGetUsers } from '@/src/services/queries/user-management-queries'

const UserLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/settings/users/${data.id}`}>{value}</Link>
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
        columnDefs={columnDefs}
        rowData={usersData}
        loadData={refresh}
        isDataLoading={isLoading}
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
