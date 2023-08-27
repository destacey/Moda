'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { ProgramIncrementObjectiveDetailsDto } from '@/src/services/moda-api'
import { Button, Card, Dropdown, MenuProps, Space } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import ProgramIncrementObjectiveDetails from './program-increment-objective-details'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import useAuth from '@/src/app/components/contexts/auth'
import EditProgramIncrementObjectiveForm from '../../edit-program-increment-objective-form'
import { DownOutlined } from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { BreadcrumbItemType } from 'antd/es/breadcrumb/Breadcrumb'
import DeleteProgramIncrementObjectiveForm from './delete-program-increment-objective-form'
import { authorizePage } from '@/src/app/components/hoc'

const ObjectiveDetailsPage = ({ params }) => {
  useDocumentTitle('PI Objective Details')
  const [activeTab, setActiveTab] = useState('details')
  const [objective, setObjective] =
    useState<ProgramIncrementObjectiveDetailsDto | null>(null)
  const { setBreadcrumbRoute } = useBreadcrumbs()
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const [openDeleteObjectiveForm, setOpenDeleteObjectiveForm] =
    useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage',
  )
  const showActions = canManageObjectives

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <ProgramIncrementObjectiveDetails objective={objective} />,
    },
  ]

  useEffect(() => {
    const breadcrumbRoute: BreadcrumbItemType[] = [
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
      const objectiveDto = await programIncrementsClient.getObjectiveByKey(
        params.key,
        params.objectiveKey,
      )
      setObjective(objectiveDto)

      breadcrumbRoute.push(
        {
          href: `/planning/program-increments/${objectiveDto.programIncrement?.key}`,
          title: objectiveDto.programIncrement?.name,
        },
        {
          title: objectiveDto.name,
        },
      )
      // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
      setBreadcrumbRoute(breadcrumbRoute)
    }

    getObjective()
  }, [params.key, params.objectiveKey, setBreadcrumbRoute, lastRefresh])

  const onUpdateObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    if (wasSaved) {
      // TODO: refresh the PI details and Teams tab only
      setLastRefresh(Date.now())
    }
  }

  const onDeleteObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenDeleteObjectiveForm(false)
    if (wasSaved) {
      // redirect to the PI details page
      window.location.href = `/planning/program-increments/${params.key}`
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
      )
    }
    return items
  }, [canManageObjectives])

  const Actions = () => {
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

  return (
    <>
      <PageTitle
        title={`${objective?.key} - ${objective?.name}`}
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
      {openUpdateObjectiveForm && canManageObjectives && (
        <EditProgramIncrementObjectiveForm
          showForm={openUpdateObjectiveForm}
          objectiveId={objective?.id}
          programIncrementId={objective?.programIncrement?.id}
          onFormSave={() => onUpdateObjectiveFormClosed(true)}
          onFormCancel={() => onUpdateObjectiveFormClosed(false)}
        />
      )}
      {openDeleteObjectiveForm && canManageObjectives && (
        <DeleteProgramIncrementObjectiveForm
          showForm={openDeleteObjectiveForm}
          objective={objective}
          onFormSave={() => onDeleteObjectiveFormClosed(true)}
          onFormCancel={() => onDeleteObjectiveFormClosed(false)}
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
