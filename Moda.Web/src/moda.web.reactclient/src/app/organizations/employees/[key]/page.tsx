'use client'

import PageTitle from '@/src/components/common/page-title'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import EmployeeDetails from './employee-details'
import { Card, MenuProps } from 'antd'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { useAppDispatch } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { InactiveTag, PageActions } from '@/src/components/common'
import { useGetEmployeeQuery } from '@/src/store/features/organizations/employee-api'
import EmployeeDetailsLoading from './loading'
import { useMessage } from '@/src/components/contexts/messaging'
import useAuth from '@/src/components/contexts/auth'
import { ItemType } from 'antd/es/menu/interface'
import DeleteEmployeeForm from '../_components/delete-employee-form'

enum EmployeeTabs {
  Details = 'details',
}

const tabs = [
  {
    key: EmployeeTabs.Details,
    tab: 'Details',
  },
]

const EmployeeDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)
  const employeeKey = Number(key)

  const [activeTab, setActiveTab] = useState(EmployeeTabs.Details)
  const [openDeleteEmployeeForm, setOpenDeleteEmployeeForm] =
    useState<boolean>(false)

  const messageApi = useMessage()
  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canDeleteEmployee = hasPermissionClaim('Permissions.Employees.Delete')

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
      messageApi.error('Failed to load employee details.')
    }
  }, [error, messageApi])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canDeleteEmployee) {
      items.push({
        key: 'delete',
        label: 'Delete',
        onClick: () => setOpenDeleteEmployeeForm(true),
      })
    }

    return items
  }, [canDeleteEmployee])

  const onDeleteFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteEmployeeForm(false)
      if (wasDeleted) {
        router.push('/organizations/employees/')
      }
    },
    [router],
  )

  if (isLoading) {
    return <EmployeeDetailsLoading />
  }

  if (!employeeData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${employeeData?.key} - ${employeeData?.displayName}`}
        subtitle="Employee Details"
        tags={<InactiveTag isActive={employeeData?.isActive} />}
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
      </Card>

      {/* Delete Employee Form */}
      {openDeleteEmployeeForm && (
        <DeleteEmployeeForm
          employeeKey={employeeData.key}
          onFormComplete={() => {
            onDeleteFormClosed(true)
          }}
          onFormCancel={() => {
            onDeleteFormClosed(false)
          }}
        />
      )}
    </>
  )
}

const EmployeeDetailsPageWithAuthorization = authorizePage(
  EmployeeDetailsPage,
  'Permission',
  'Permissions.Employees.View',
)

export default EmployeeDetailsPageWithAuthorization
