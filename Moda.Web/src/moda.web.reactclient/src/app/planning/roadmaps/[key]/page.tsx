'use client'

import {
  ModaDateRange,
  PageActions,
  PageTitle,
} from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/app/hooks'
import {
  useGetRoadmapItemsQuery,
  useGetRoadmapQuery,
} from '@/src/store/features/planning/roadmaps-api'
import { notFound, usePathname, useRouter } from 'next/navigation'
import RoadmapDetailsLoading from './loading'
import { useEffect, useMemo, useState } from 'react'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { LockOutlined, UnlockOutlined } from '@ant-design/icons'
import { Descriptions, MenuProps, message } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import EditRoadmapForm from '../components/edit-roadmap-form'
import ModaMarkdownDescription from '@/src/app/components/common/moda-markdown-description'
import RoadmapViewManager from './roadmap-view-manager'
import { DeleteRoadmapForm } from '../components'
import CreateRoadmapActivityForm from '../components/create-roadmap-activity-form'

const { Item } = Descriptions

const visibilityTitle = (visibility: string, managersInfo: string) => {
  return `This roadmap is set to ${visibility}.\n\nThe roadmap managers are: ${managersInfo}`
}

const RoadmapDetailsPage = ({ params }) => {
  useDocumentTitle('Roadmap Details')
  const [managersInfo, setManagersInfo] = useState('Unknown')
  const [children, setChildren] = useState([])
  const [openCreateActivityForm, setOpenCreateActivityForm] =
    useState<boolean>(false)
  const [openEditRoadmapForm, setOpenEditRoadmapForm] = useState<boolean>(false)
  const [openDeleteRoadmapForm, setOpenDeleteRoadmapForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')
  const canUpdateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Update')
  const canDeleteRoadmap = hasPermissionClaim('Permissions.Roadmaps.Delete')

  const {
    data: roadmapData,
    isLoading,
    error,
    refetch: refetchRoadmap,
  } = useGetRoadmapQuery(params.key)

  const {
    data: roadmapItems,
    isFetching: isRoadmapItemsLoading,
    refetch: refetchRoadmapItems,
  } = useGetRoadmapItemsQuery(params.key, {
    skip: !roadmapData,
  })

  useEffect(() => {
    if (!roadmapData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/roadmaps`,
        title: 'Roadmaps',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, roadmapData])

  useEffect(() => {
    if (!roadmapData) return
    const managers = roadmapData.roadmapManagers
      .slice()
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((m) => m.name)
      .join(', ')
    setManagersInfo(managers)

    setChildren(roadmapItems)
  }, [roadmapItems, roadmapData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdateRoadmap) {
      items.push({
        key: 'edit',
        label: 'Edit',
        onClick: () => setOpenEditRoadmapForm(true),
      })
    }
    if (canDeleteRoadmap) {
      items.push({
        key: 'delete',
        label: 'Delete',
        onClick: () => setOpenDeleteRoadmapForm(true),
      })
    }
    if (canCreateRoadmap) {
      items.push(
        {
          key: 'divider',
          type: 'divider',
        },
        {
          key: 'create-activity',
          label: 'Create Activity',
          onClick: () => setOpenCreateActivityForm(true),
        },
      )
    }

    return items
  }, [canCreateRoadmap, canDeleteRoadmap, canUpdateRoadmap])

  if (isLoading) {
    return <RoadmapDetailsLoading />
  }

  if (!roadmapData) {
    notFound()
  }

  const onCreateRoadmapFormClosed = (wasCreated: boolean) => {
    setOpenCreateActivityForm(false)
    if (wasCreated) {
      refetchRoadmapItems()
    }
  }

  const onEditRoadmapFormClosed = (wasSaved: boolean) => {
    setOpenEditRoadmapForm(false)
    if (wasSaved) {
      refetchRoadmap()
    }
  }

  const onDeleteFormClosed = (wasDeleted: boolean) => {
    setOpenDeleteRoadmapForm(false)
    if (wasDeleted) {
      router.push('/planning/roadmaps/')
    }
  }

  const visibilityTag =
    roadmapData?.visibility?.name === 'Public' ? (
      <UnlockOutlined title={visibilityTitle('Public', managersInfo)} />
    ) : (
      <LockOutlined title={visibilityTitle('Private', managersInfo)} />
    )

  return (
    <>
      {contextHolder}
      <PageTitle
        title={`${roadmapData?.key} - ${roadmapData?.name}`}
        subtitle="Roadmap Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
        tags={visibilityTag}
      />
      {roadmapData && (
        <>
          <Descriptions>
            <Item label="Dates">
              <ModaDateRange
                dateRange={{ start: roadmapData.start, end: roadmapData.end }}
              />
            </Item>
          </Descriptions>
          <Descriptions>
            {roadmapData.description && (
              <Item>
                <ModaMarkdownDescription content={roadmapData.description} />
              </Item>
            )}
          </Descriptions>
        </>
      )}
      <RoadmapViewManager
        roadmap={roadmapData}
        roadmapItems={children}
        isRoadmapItemsLoading={isRoadmapItemsLoading}
        refreshRoadmapItems={refetchRoadmapItems}
        canUpdateRoadmap={canUpdateRoadmap}
        messageApi={messageApi}
      />
      {openCreateActivityForm && (
        <CreateRoadmapActivityForm
          showForm={openCreateActivityForm}
          roadmapId={roadmapData?.id}
          onFormComplete={() => onCreateRoadmapFormClosed(true)}
          onFormCancel={() => onCreateRoadmapFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openEditRoadmapForm && (
        <EditRoadmapForm
          roadmapId={roadmapData?.id}
          showForm={openEditRoadmapForm}
          onFormComplete={() => onEditRoadmapFormClosed(true)}
          onFormCancel={() => onEditRoadmapFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openDeleteRoadmapForm && (
        <DeleteRoadmapForm
          roadmap={roadmapData}
          showForm={openDeleteRoadmapForm}
          onFormComplete={() => onDeleteFormClosed(true)}
          onFormCancel={() => onDeleteFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const RoadmapDetailsPageWithAuthorization = authorizePage(
  RoadmapDetailsPage,
  'Permission',
  'Permissions.Roadmaps.View',
)

export default RoadmapDetailsPageWithAuthorization
