'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Button, Card, Dropdown, MenuProps, Space } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import ProgramIncrementObjectiveDetails from './program-increment-objective-details'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import EditProgramIncrementObjectiveForm from '../../edit-program-increment-objective-form'
import { DownOutlined } from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import DeleteProgramIncrementObjectiveForm from './delete-program-increment-objective-form'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetProgramIncrementObjectiveByKey } from '@/src/services/queries/planning-queries'
import { notFound, useRouter, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import ProgramIncrementObjectiveDetailsLoading from './loading'
import CreateHealthCheckForm from '@/src/app/components/common/health-check/create-health-check-form'
import { SystemContext } from '@/src/app/components/constants'

const ObjectiveDetailsPage = ({ params }) => {
  useDocumentTitle('PI Objective Details')

  const {
    data: objectiveData,
    isLoading,
    isFetching,
    refetch: refetchObjective,
  } = useGetProgramIncrementObjectiveByKey(params.key, params.objectiveKey)

  const [activeTab, setActiveTab] = useState('details')
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const [openDeleteObjectiveForm, setOpenDeleteObjectiveForm] =
    useState<boolean>(false)
  const [openCreateHealthCheckForm, setOpenCreateHealthCheckForm] =
    useState<boolean>(false)

  const router = useRouter()
  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage',
  )
  const canCreateHealthChecks =
    !!canManageObjectives &&
    hasClaim('Permission', 'Permissions.HealthChecks.Create')
  const showActions = canManageObjectives

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <ProgramIncrementObjectiveDetails objective={objectiveData} />,
    },
  ]

  useEffect(() => {
    if (!objectiveData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/program-increments`,
        title: 'Program Increments',
      },
    ]

    breadcrumbRoute.push(
      {
        href: `/planning/program-increments/${objectiveData.programIncrement?.key}`,
        title: objectiveData.programIncrement?.name,
      },
      {
        title: objectiveData.name,
      },
    )
    // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
    dispatch(
      setBreadcrumbRoute({
        pathname,
        route: breadcrumbRoute,
      }),
    )
  }, [dispatch, objectiveData, pathname])

  const onUpdateObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    if (wasSaved) {
      refetchObjective()
    }
  }

  const onDeleteObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenDeleteObjectiveForm(false)
    if (wasSaved) {
      // redirect to the PI details page
      router.push(`/planning/program-increments/${params.key}`)
    }
  }

  const onCreateHealthCheckFormClosed = (wasSaved: boolean) => {
    setOpenCreateHealthCheckForm(false)
    if (wasSaved) {
      refetchObjective()
    }
  }

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canManageObjectives) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          onClick: () => setOpenUpdateObjectiveForm(true),
        },
        {
          key: 'delete',
          label: 'Delete',
          onClick: () => setOpenDeleteObjectiveForm(true),
        },
        {
          key: 'divider',
          type: 'divider',
        },
        {
          key: 'createHealthCheck',
          label: 'Create Health Check',
          disabled: !canCreateHealthChecks,
          onClick: () => setOpenCreateHealthCheckForm(true),
        },
      )
    }
    return items
  }, [canCreateHealthChecks, canManageObjectives])

  const actions = () => {
    return (
      <>
        {canManageObjectives && (
          <Dropdown menu={{ items: actionsMenuItems }}>
            <Button>
              <Space>
                Actions
                <DownOutlined />
              </Space>
            </Button>
          </Dropdown>
        )}
      </>
    )
  }

  if (isLoading) {
    return <ProgramIncrementObjectiveDetailsLoading />
  }

  if (!isLoading && !isFetching && !objectiveData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={`${objectiveData?.key} - ${objectiveData?.name}`}
        subtitle="PI Objective Details"
        actions={showActions && actions()}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openUpdateObjectiveForm && (
        <EditProgramIncrementObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={objectiveData?.id}
          programIncrementId={objectiveData?.programIncrement?.id}
          onFormSave={() => onUpdateObjectiveFormClosed(true)}
          onFormCancel={() => onUpdateObjectiveFormClosed(false)}
        />
      )}
      {openDeleteObjectiveForm && (
        <DeleteProgramIncrementObjectiveForm
          showForm={openDeleteObjectiveForm}
          objective={objectiveData}
          onFormSave={() => onDeleteObjectiveFormClosed(true)}
          onFormCancel={() => onDeleteObjectiveFormClosed(false)}
        />
      )}
      {openCreateHealthCheckForm && (
        <CreateHealthCheckForm
          showForm={openCreateHealthCheckForm}
          objectId={objectiveData?.id}
          context={SystemContext.PlanningProgramIncrementObjective}
          onFormCreate={() => onCreateHealthCheckFormClosed(true)}
          onFormCancel={() => onCreateHealthCheckFormClosed(false)}
        />
      )}
    </>
  )
}

const ObjectiveDetailsPageWithAuthorization = authorizePage(
  ObjectiveDetailsPage,
  'Permission',
  'Permissions.ProgramIncrements.View',
)

export default ObjectiveDetailsPageWithAuthorization
