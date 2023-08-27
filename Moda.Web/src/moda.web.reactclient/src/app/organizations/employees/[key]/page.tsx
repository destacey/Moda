'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { EmployeeDetailsDto } from '@/src/services/moda-api'
import { useEffect, useState } from 'react'
import EmployeeDetails from './employee-details'
import { getEmployeesClient } from '@/src/services/clients'
import { Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'

const EmployeeDetailsPage = ({ params }) => {
  useDocumentTitle('Employee Details')
  const [activeTab, setActiveTab] = useState('details')
  const [employee, setEmployee] = useState<EmployeeDetailsDto | null>(null)
  const { key } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [notEmployeeFound, setEmployeeNotFound] = useState<boolean>(false)

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <EmployeeDetails employee={employee} />,
    },
  ]

  useEffect(() => {
    const getEmployee = async () => {
      const employeesClient = await getEmployeesClient()
      const employeeDto = await employeesClient.getById(key)
      setEmployee(employeeDto)
      setBreadcrumbTitle(employeeDto.displayName)
    }

    getEmployee().catch((error) => {
      if (error.status === 404) {
        setEmployeeNotFound(true)
      }
      console.error('getEmployee error', error)
    })
  }, [key, setBreadcrumbTitle])

  if (notEmployeeFound) {
    return notFound()
  }

  return (
    <>
      <PageTitle title={employee?.displayName} subtitle="Employee Details" />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
    </>
  )
}

const EmployeeDetailsPageWithAuthorization = authorizePage(
  EmployeeDetailsPage,
  'Permission',
  'Permissions.Employees.View',
)

export default EmployeeDetailsPageWithAuthorization
