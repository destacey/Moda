'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { UserDetailsDto } from '@/src/services/moda-api'
import { getUsersClient } from '@/src/services/clients'
import { authorizePage } from '../../components/hoc'
import Link from 'next/link'

const EmployeeLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.employee?.localId}`}>
      {value}
    </Link>
  )
}

const Page = () => {
  const [users, setUsers] = useState<UserDetailsDto[]>([])

  const columnDefs = useMemo(
    () => [
      { field: 'userName' },
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
    []
  )

  const getUsers = useCallback(async () => {
    const usersClient = await getUsersClient()
    const userDtos = await usersClient.getList()
    setUsers(userDtos)
  }, [])

  return (
    <>
      <PageTitle title="Users" />

      <ModaGrid columnDefs={columnDefs} rowData={users} loadData={getUsers} />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  Page,
  'Permission',
  'Permissions.Users.View'
)

export default PageWithAuthorization
