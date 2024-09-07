'use client'

import { PageActions, PageTitle } from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/app/hooks'
import { useGetRoadmapQuery } from '@/src/store/features/planning/roadmaps-api'
import { notFound, usePathname, useRouter } from 'next/navigation'
import RoadmapDetailsLoading from './loading'
import { useEffect, useMemo, useState } from 'react'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { LockOutlined, UnlockOutlined } from '@ant-design/icons'
import { Descriptions, List, MenuProps, message } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import EditRoadmapForm from '../components/edit-roadmap-form'
import ModaMarkdownDescription from '@/src/app/components/common/moda-markdown-description'
import DeleteRoadmapForm from '../components/delete-roadmap-form'
import CreateRoadmapForm from '../components/create-roadmap-form'

const { Item } = Descriptions
const { Item: ListItem } = List

const visibilityTitle = (visibility: string, managersInfo: string) => {
  return `This roadmap is set to ${visibility}.\n\nThe roadmap managers are: ${managersInfo}`
}

const RoadmapDetailsPage = ({ params }) => {
  useDocumentTitle('Roadmap Details')
  const [managersInfo, setManagersInfo] = useState('Unknown')
  const [children, setChildren] = useState([])
  const [openCreateRoadmapForm, setOpenCreateRoadmapForm] =
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
    isFetching,
    error,
    refetch: refetchRoadmap,
  } = useGetRoadmapQuery(params.key)

  useEffect(() => {
    dispatch(setBreadcrumbTitle({ title: 'Details', pathname }))
  }, [dispatch, pathname])

  useEffect(() => {
    if (!roadmapData) return
    const managers = roadmapData.managers
      .slice()
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((m) => m.name)
      .join(', ')
    setManagersInfo(managers)

    const children = roadmapData.children
      .slice()
      .sort((a, b) => a.order - b.order)
    setChildren(children)
  }, [roadmapData])

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
          key: 'create-child',
          label: 'Create Child Roadmap',
          onClick: () => setOpenCreateRoadmapForm(true),
        },
      )
    }

    return items
  }, [canCreateRoadmap, canDeleteRoadmap, canUpdateRoadmap])

  if (isLoading) {
    return <RoadmapDetailsLoading />
  }

  if (!isLoading && !roadmapData) {
    notFound()
  }

  const onCreateRoadmapFormClosed = (wasCreated: boolean) => {
    setOpenCreateRoadmapForm(false)
    if (wasCreated) {
      refetchRoadmap()
    }
  }

  const onEditRoadmapFormClosed = (wasSaved: boolean) => {
    setOpenEditRoadmapForm(false)
    if (wasSaved) {
      refetchRoadmap()
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
      {roadmapData?.description && (
        <Descriptions>
          <Item>
            <ModaMarkdownDescription content={roadmapData?.description} />
          </Item>
        </Descriptions>
      )}
      <List
        dataSource={children}
        renderItem={(item) => (
          <ListItem>
            <ListItem.Meta
              title={`${item.roadmap.key} - ${item.roadmap.name} (${item.order})`}
              description={`${item.roadmap.start} - ${item.roadmap.end}`}
            />
          </ListItem>
        )}
      />
      {openCreateRoadmapForm && (
        <CreateRoadmapForm
          showForm={openCreateRoadmapForm}
          parentRoadmapId={roadmapData?.id}
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
          onFormComplete={() => router.push('/planning/roadmaps/')}
          onFormCancel={() => setOpenDeleteRoadmapForm(false)}
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
