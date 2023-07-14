'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { ProgramIncrementObjectiveDetailsDto } from '@/src/services/moda-api'
import { Button, Card } from 'antd'
import { createElement, useEffect, useState } from 'react'
import ProgramIncrementObjectiveDetails from './program-increment-objective-details'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import useAuth from '@/src/app/components/contexts/auth'
import UpdateProgramIncrementObjectiveForm from '../../update-program-increment-objective-form'

const ObjectiveDetailsPage = ({ params }) => {
  useDocumentTitle('PI Objective Details')
  const [activeTab, setActiveTab] = useState('details')
  const [objective, setObjective] =
    useState<ProgramIncrementObjectiveDetailsDto | null>(null)
  const { setBreadcrumbRoute } = useBreadcrumbs()
  const [openUpdateObjectiveModal, setOpenUpdateObjectiveModal] =
    useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage'
  )
  const showActions = canManageObjectives

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: createElement(ProgramIncrementObjectiveDetails, objective),
    },
  ]

  useEffect(() => {
    const objectiveRoute: ItemType[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/program-increments`,
        title: 'Program Increments',
      },
    ]

    const getObjective = async () => {
      const programIncrementsClient = await getProgramIncrementsClient()
      const objectiveDto = await programIncrementsClient.getObjectiveByLocalId(
        params.id,
        params.objectiveId
      )
      setObjective(objectiveDto)

      objectiveRoute.push(
        {
          href: `/planning/program-increments/${objectiveDto.programIncrement?.localId}`,
          title: objectiveDto.programIncrement?.name,
        },
        {
          title: objectiveDto.name,
        }
      )
      // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
      setBreadcrumbRoute(objectiveRoute)
    }

    getObjective()
  }, [params.id, params.objectiveId, setBreadcrumbRoute, lastRefresh])

  const onUpdateObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveModal(false)
    if (wasSaved) {
      // TODO: refresh the PI details and Teams tab only
      setLastRefresh(Date.now())
    }
  }

  const Actions = () => {
    return (
      <>
        {canManageObjectives && (
          <Button onClick={() => setOpenUpdateObjectiveModal(true)}>
            Edit
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle
        title={`${objective?.localId} - ${objective?.name}`}
        subtitle="PI Objective Details"
        actions={showActions && <Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      <UpdateProgramIncrementObjectiveForm
        showForm={openUpdateObjectiveModal}
        objectiveId={objective?.id}
        programIncrementId={objective?.programIncrement?.id}
        onFormSave={() => onUpdateObjectiveFormClosed(true)}
        onFormCancel={() => onUpdateObjectiveFormClosed(false)}
      />
    </>
  )
}

export default ObjectiveDetailsPage
