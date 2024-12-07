'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { EmployeeDetailsDto } from '@/src/services/moda-api'
import { useEffect, useState } from 'react'
import EmployeeDetails from './employee-details'
import { getEmployeesClient } from '@/src/services/clients'
import { Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { InactiveTag } from '@/src/app/components/common'

const EmployeeDetailsPage = ({ params }) => {
  useDocumentTitle('Employee Details')
  const [activeTab, setActiveTab] = useState('details')
  const [employee, setEmployee] = useState<EmployeeDetailsDto | null>(null)
  const { key } = params
  const [employeeNotFound, setEmployeeNotFound] = useState<boolean>(false)
  const pathname = usePathname()
  const dispatch = useAppDispatch()

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
      dispatch(setBreadcrumbTitle({ title: employeeDto.displayName, pathname }))
    }

    getEmployee().catch((error) => {
      if (error.status === 404) {
        setEmployeeNotFound(true)
      }
      console.error('getEmployee error', error)
    })
  }, [key, dispatch, pathname])

  if (employeeNotFound) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={employee?.displayName}
        subtitle="Employee Details"
        tags={<InactiveTag isActive={employee?.isActive} />}
      />
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
