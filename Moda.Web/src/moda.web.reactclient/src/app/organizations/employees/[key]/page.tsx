'use client'

import PageTitle from '@/src/components/common/page-title'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import EmployeeDetails from './employee-details'
import { Card } from 'antd'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { InactiveTag } from '@/src/components/common'
import { useGetEmployeeQuery } from '@/src/store/features/organizations/employee-api'
import EmployeeDetailsLoading from './loading'
import { useMessage } from '@/src/components/contexts/messaging'

enum EmployeeTabs {
  Details = 'details',
}

const EmployeeDetailsPage = (props: { params: Promise<{ key: number }> }) => {
  const { key: employeeKey } = use(props.params)

  const [activeTab, setActiveTab] = useState(EmployeeTabs.Details)

  const messageApi = useMessage()
  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const {
    data: employeeData,
    isLoading,
    error,
  } = useGetEmployeeQuery(employeeKey)

  useDocumentTitle(
    employeeData?.displayName
      ? `${employeeData.displayName} - Employee Details`
      : 'Employee Details',
  )

  const tabs = [
    {
      key: EmployeeTabs.Details,
      tab: 'Details',
    },
  ]

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case EmployeeTabs.Details:
        return <EmployeeDetails employee={employeeData} />
      default:
        return null
    }
  }, [activeTab, employeeData])

  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as EmployeeTabs)
  }, [])

  useEffect(() => {
    dispatch(setBreadcrumbTitle({ title: 'Details', pathname }))
  }, [dispatch, pathname])

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load employee details.')
    }
  }, [error, messageApi])

  if (isLoading) {
    return <EmployeeDetailsLoading />
  }

  if (!employeeData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={employeeData?.displayName}
        subtitle="Employee Details"
        tags={<InactiveTag isActive={employeeData?.isActive} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
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
