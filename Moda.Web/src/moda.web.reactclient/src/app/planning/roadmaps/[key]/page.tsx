'use client'

import { ModaDateRange, PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import {
  useGetRoadmapItemsQuery,
  useGetRoadmapQuery,
} from '@/src/store/features/planning/roadmaps-api'
import {
  notFound,
  usePathname,
  useRouter,
  useSearchParams,
} from 'next/navigation'
import RoadmapDetailsLoading from './loading'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { LockOutlined, UnlockOutlined } from '@ant-design/icons'
import { Descriptions, Divider, MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import {
  CopyRoadmapForm,
  DeleteRoadmapForm,
  EditRoadmapForm,
  RoadmapItemDrawer,
  RoadmapViewManager,
} from '../_components'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import ReorganizeRoadmapActivitiesModal from '../_components/reorganize-roadmap-activities-modal'
import CreateRoadmapActivityForm from '../_components/create-roadmap-activity-form'
import CreateRoadmapTimeboxForm from '../_components/create-roadmap-timebox-form'
import { useGetInternalEmployeeIdQuery } from '@/src/store/features/user-management/profile-api'

const { Item } = Descriptions

const visibilityTitle = (visibility: string, managersInfo: string) => {
  return `This roadmap is set to ${visibility}.\n\nThe roadmap managers are: ${managersInfo}`
}

const RoadmapDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)
  const roadmapKey = Number(key)

  const [openCreateActivityForm, setOpenCreateActivityForm] =
    useState<boolean>(false)
  const [openCreateTimeboxForm, setOpenCreateTimeboxForm] =
    useState<boolean>(false)
  const [openEditRoadmapForm, setOpenEditRoadmapForm] = useState<boolean>(false)
  const [openCopyRoadmapForm, setOpenCopyRoadmapForm] = useState<boolean>(false)
  const [openDeleteRoadmapForm, setOpenDeleteRoadmapForm] =
    useState<boolean>(false)
  const [openReorganizeActivitiesModal, setOpenReorganizeActivitiesModal] =
    useState<boolean>(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Update')
  const canDeleteRoadmap = hasPermissionClaim('Permissions.Roadmaps.Delete')
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')

  const {
    data: roadmapData,
    isLoading,
    error,
    refetch: refetchRoadmap,
  } = useGetRoadmapQuery(roadmapKey.toString())

  useDocumentTitle(`${roadmapData?.name ?? roadmapKey} - Roadmap Details`)

  const {
    data: currentUserInternalEmployeeId,
    error: currentUserInternalEmployeeIdError,
  } = useGetInternalEmployeeIdQuery()

  const {
    data: roadmapItems,
    isFetching: isRoadmapItemsLoading,
    refetch: refetchRoadmapItems,
  } = useGetRoadmapItemsQuery(roadmapData?.id, {
    skip: !roadmapData,
  })

  const searchParams = useSearchParams()
  const timelineEditMode = searchParams.has('editMode')

  const managersInfo = useMemo(() => {
    if (!roadmapData) return 'Unknown'
    return roadmapData.roadmapManagers
      .slice()
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((m) => m.name)
      .join(', ')
  }, [roadmapData])

  const isRoadmapManager = useMemo(() => {
    if (!roadmapData || !currentUserInternalEmployeeId) return false
    return roadmapData.roadmapManagers.some(
      (rm) => rm.id === currentUserInternalEmployeeId,
    )
  }, [roadmapData, currentUserInternalEmployeeId])

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

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []

    // Copy is available to anyone who can view the roadmap and create roadmaps
    if (canCreateRoadmap) {
      items.push({
        key: 'copy',
        label: 'Copy',
        onClick: () => setOpenCopyRoadmapForm(true),
      })
    }

    if (!isRoadmapManager) return items

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
    if (canUpdateRoadmap) {
      items.push(
        {
          key: 'manage-divider',
          type: 'divider',
        },
        {
          key: 'reorganize-activities',
          label: 'Reorganize Activities',
          onClick: () => setOpenReorganizeActivitiesModal(true),
        },
        {
          key: 'create-divider',
          type: 'divider',
        },
        {
          key: 'create-activity',
          label: 'Create Activity',
          onClick: () => setOpenCreateActivityForm(true),
        },
        {
          key: 'create-timebox',
          label: 'Create Timebox',
          onClick: () => setOpenCreateTimeboxForm(true),
        },
      )
    }

    return items
  }, [canCreateRoadmap, canDeleteRoadmap, canUpdateRoadmap, isRoadmapManager])

  const onEditRoadmapFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditRoadmapForm(false)
      if (wasSaved) {
        refetchRoadmap()
      }
    },
    [refetchRoadmap],
  )

  const onCopyRoadmapFormClosed = useCallback(() => {
    setOpenCopyRoadmapForm(false)
  }, [])

  const onDeleteFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteRoadmapForm(false)
      if (wasDeleted) {
        router.push('/planning/roadmaps/')
      }
    },
    [router],
  )

  const onCreateRoadmapActivityFormClosed = useCallback(
    (wasCreated: boolean) => {
      setOpenCreateActivityForm(false)
      if (wasCreated) {
        refetchRoadmapItems()
      }
    },
    [refetchRoadmapItems],
  )

  const onCreateRoadmapTimeboxFormClosed = useCallback(
    (wasCreated: boolean) => {
      setOpenCreateTimeboxForm(false)
      if (wasCreated) {
        refetchRoadmapItems()
      }
    },
    [refetchRoadmapItems],
  )

  const visibilityTag = useMemo(
    () =>
      roadmapData?.visibility?.name === 'Public' ? (
        <UnlockOutlined title={visibilityTitle('Public', managersInfo)} />
      ) : (
        <LockOutlined title={visibilityTitle('Private', managersInfo)} />
      ),
    [managersInfo, roadmapData?.visibility?.name],
  )

  const showDrawer = useCallback(() => {
    setDrawerOpen(true)
  }, [])

  const onDrawerClose = useCallback(() => {
    setDrawerOpen(false)
    setSelectedItemId(null)
  }, [])

  const openRoadmapItemDrawer = useCallback(
    (itemId: string) => {
      setSelectedItemId(itemId)
      showDrawer()
    },
    [showDrawer],
  )

  const onReorganizeActivitiesModalClose = useCallback(() => {
    setOpenReorganizeActivitiesModal(false)
    refetchRoadmapItems()
  }, [refetchRoadmapItems])

  if (isLoading) {
    return <RoadmapDetailsLoading />
  }

  if (!roadmapData) {
    return notFound()
  }

  return (
    <>
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
            <Item>
              <MarkdownRenderer markdown={roadmapData.description} />
            </Item>
          </Descriptions>
        </>
      )}
      <Divider />
      <RoadmapViewManager
        roadmap={roadmapData}
        roadmapItems={roadmapItems}
        isRoadmapItemsLoading={isRoadmapItemsLoading}
        refreshRoadmapItems={refetchRoadmapItems}
        canUpdateRoadmap={canUpdateRoadmap && isRoadmapManager}
        openRoadmapItemDrawer={openRoadmapItemDrawer}
        timelineEditMode={timelineEditMode}
      />
      {openEditRoadmapForm && (
        <EditRoadmapForm
          roadmapKey={roadmapKey}
          showForm={openEditRoadmapForm}
          onFormComplete={() => onEditRoadmapFormClosed(true)}
          onFormCancel={() => onEditRoadmapFormClosed(false)}
        />
      )}
      {openCopyRoadmapForm && (
        <CopyRoadmapForm
          sourceRoadmapId={roadmapData?.id}
          sourceRoadmapName={roadmapData?.name}
          showForm={openCopyRoadmapForm}
          onFormComplete={onCopyRoadmapFormClosed}
          onFormCancel={onCopyRoadmapFormClosed}
        />
      )}
      {openDeleteRoadmapForm && (
        <DeleteRoadmapForm
          roadmap={roadmapData}
          showForm={openDeleteRoadmapForm}
          onFormComplete={() => onDeleteFormClosed(true)}
          onFormCancel={() => onDeleteFormClosed(false)}
        />
      )}
      {openReorganizeActivitiesModal && (
        <ReorganizeRoadmapActivitiesModal
          showModal={openReorganizeActivitiesModal}
          roadmapId={roadmapData?.id}
          roadmapItems={roadmapItems}
          onClose={onReorganizeActivitiesModalClose}
        />
      )}
      {openCreateActivityForm && (
        <CreateRoadmapActivityForm
          showForm={openCreateActivityForm}
          roadmapId={roadmapData?.id}
          onFormComplete={() => onCreateRoadmapActivityFormClosed(true)}
          onFormCancel={() => onCreateRoadmapActivityFormClosed(false)}
        />
      )}
      {openCreateTimeboxForm && (
        <CreateRoadmapTimeboxForm
          showForm={openCreateTimeboxForm}
          roadmapId={roadmapData?.id}
          onFormComplete={() => onCreateRoadmapTimeboxFormClosed(true)}
          onFormCancel={() => onCreateRoadmapTimeboxFormClosed(false)}
        />
      )}
      {roadmapData?.id && selectedItemId && (
        <RoadmapItemDrawer
          roadmapId={roadmapData.id}
          roadmapItemId={selectedItemId}
          drawerOpen={drawerOpen}
          onDrawerClose={onDrawerClose}
          openRoadmapItemDrawer={openRoadmapItemDrawer}
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
