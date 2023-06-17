'use client'

import PageTitle from "@/src/app/components/common/page-title";
import ModaGrid from "../../components/common/moda-grid";
import { useEffect, useState } from "react";
import { EmployeeListDto } from "@/src/services/moda-api";
import { getEmployeesClient } from "@/src/services/clients";

const columnDefs = [
  { field: 'localId', headerName: 'Id' },
  { field: 'firstName' },
  { field: 'lastName' },
  { field: 'department' },
  { field: 'jobTitle' },
  { field: 'managerName', headerName: 'Manager' },
  { field: 'officeLocation' },
  { field: 'email' },
  { field: 'isActive' } // TODO: convert to yes/no
]

const Page = () => {
  const [employees, setEmployees] = useState<EmployeeListDto[]>([])

  useEffect(() => {
    const getEmployees = async () => {
      const employeesClient = await getEmployeesClient()
      const employeeDtos = await employeesClient.getList(false)
      setEmployees(employeeDtos)
    }

    getEmployees()
  }, [])

  return (
    <>
      <PageTitle title="Employees" />

      <ModaGrid columnDefs={columnDefs}
        rowData={employees} />
    </>
  );
}

export default Page;