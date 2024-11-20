'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card, MenuProps } from 'antd'
import { useMemo, useState } from 'react'
import PlanningIntervalObjectiveDetails from './planning-interval-objective-details'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import EditPlanningIntervalObjectiveForm from '../../../components/edit-planning-interval-objective-form'
import { ItemType } from 'antd/es/menu/interface'
import DeletePlanningIntervalObjectiveForm from './delete-planning-interval-objective-form'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetPlanningIntervalObjectiveByKey } from '@/src/services/queries/planning-queries'
import { notFound, useRouter } from 'next/navigation'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import PlanningIntervalObjectiveDetailsLoading from './loading'
import CreateHealthCheckForm from '@/src/app/components/common/health-check/create-health-check-form'
import { SystemContext } from '@/src/app/components/constants'
import HealthCheckTag from '@/src/app/components/common/health-check/health-check-tag'
import { beginHealthCheckCreate } from '@/src/store/features/health-check-slice'
import Link from 'next/link'
import { PageActions } from '@/src/app/components/common'

const ObjectiveDetailsPage = ({ params }) => {
  useDocumentTitle('PI Objective Details')

  const [activeTab, setActiveTab] = useState('details')
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const [openDeleteObjectiveForm, setOpenDeleteObjectiveForm] =
    useState<boolean>(false)

  const {
    data: objectiveData,
    isLoading,
    isFetching,
    refetch: refetchObjective,
  } = useGetPlanningIntervalObjectiveByKey(params.key, params.objectiveKey)

  const router = useRouter()
  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.PlanningIntervalObjectives.Manage',
  )
  const canCreateHealthChecks =
    !!canManageObjectives &&
    hasClaim('Permission', 'Permissions.HealthChecks.Create')
  const showActions = canManageObjectives

  const dispatch = useAppDispatch()
  const editingObjectiveId = useAppSelector(
    (state) => state.healthCheck.createContext.objectId,
  )

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: (
        <PlanningIntervalObjectiveDetails
          objective={objectiveData}
          canManageObjectives={canManageObjectives}
        />
      ),
    },
  ]

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
      router.push(`/planning/planning-intervals/${params.key}`)
    }
  }

  const onCreateHealthCheckFormClosed = (wasSaved: boolean) => {
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
          onClick: () =>
            dispatch(
              beginHealthCheckCreate({
                objectId: objectiveData?.id,
                contextId: SystemContext.PlanningPlanningIntervalObjective,
              }),
            ),
        },
      )
    }
    items.push({
      key: 'healthReport',
      label: (
        <Link
          href={`/planning/planning-intervals/${params.key}/objectives/${params.objectiveKey}/health-report`}
        >
          Health Report
        </Link>
      ),
    })

    return items
  }, [
    canCreateHealthChecks,
    canManageObjectives,
    dispatch,
    objectiveData?.id,
    params.key,
    params.objectiveKey,
  ])

  if (isLoading) {
    return <PlanningIntervalObjectiveDetailsLoading />
  }

  if (!isLoading && !isFetching && !objectiveData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={`${objectiveData?.key} - ${objectiveData?.name}`}
        subtitle="PI Objective Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
        tags={<HealthCheckTag healthCheck={objectiveData?.healthCheck} />}
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
        <EditPlanningIntervalObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={objectiveData?.id}
          planningIntervalId={objectiveData?.planningInterval?.id}
          onFormSave={() => onUpdateObjectiveFormClosed(true)}
          onFormCancel={() => onUpdateObjectiveFormClosed(false)}
        />
      )}
      {openDeleteObjectiveForm && (
        <DeletePlanningIntervalObjectiveForm
          showForm={openDeleteObjectiveForm}
          objective={objectiveData}
          onFormSave={() => onDeleteObjectiveFormClosed(true)}
          onFormCancel={() => onDeleteObjectiveFormClosed(false)}
        />
      )}
      {editingObjectiveId == objectiveData?.id && (
        <CreateHealthCheckForm onClose={onCreateHealthCheckFormClosed} />
      )}
    </>
  )
}

const ObjectiveDetailsPageWithAuthorization = authorizePage(
  ObjectiveDetailsPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default ObjectiveDetailsPageWithAuthorization
