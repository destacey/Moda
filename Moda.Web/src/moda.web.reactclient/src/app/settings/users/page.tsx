'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { useEffect, useState } from "react";
import ModaGrid from "../../components/common/moda-grid";
import { UserDetailsDto } from "@/src/services/moda-api";
import { getUsersClient } from "@/src/services/clients";
import { withAuthorization } from "../../components/hoc";

// TODO: check permissions

const columnDefs = [
  { field: 'userName' },
  { field: 'firstName' },
  { field: 'lastName' },
  { field: 'email' },
  { field: 'employee.name', headerName: 'Employee' },
  { field: 'isActive' } // TODO: convert to yes/no
]

const Page = () => {
  const [users, setUsers] = useState<UserDetailsDto[]>([])

  useEffect(() => {
    const getUsers = async () => {
      const usersClient = await getUsersClient()
      const userDtos = await usersClient.getList()
      setUsers(userDtos)
    }

    getUsers()
  }, [])

  return (
    <>
      <PageTitle title="Users" />

      <ModaGrid columnDefs={columnDefs}
        rowData={users} />
    </>
  );
}

const PageWithAuthorization = withAuthorization(Page, "Permission", "Permissions.Users.View")

export default PageWithAuthorization;