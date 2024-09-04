'use client'

import { PageActions, PageTitle } from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/app/hooks'
import { useGetRoadmapQuery } from '@/src/store/features/planning/roadmaps-api'
import { notFound, usePathname } from 'next/navigation'
import RoadmapDetailsLoading from './loading'
import { useEffect, useMemo, useState } from 'react'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { LockOutlined, UnlockOutlined } from '@ant-design/icons'
import { MenuProps, message } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import EditRoadmapForm from '../components/edit-roadmap-form'

const RoadmapDetailsPage = ({ params }) => {
  useDocumentTitle('Roadmap Details')
  const [openEditRoadmapForm, setOpenEditRoadmapForm] = useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const { hasPermissionClaim } = useAuth()
  const canUpdateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Update')

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
    error && console.error(error)
  }, [error])

  const onEditRoadmapFormClosed = (wasSaved: boolean) => {
    setOpenEditRoadmapForm(false)
    if (wasSaved) {
      refetchRoadmap()
    }
  }

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdateRoadmap) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          onClick: () => setOpenEditRoadmapForm(true),
        },
        // {
        //   key: 'delete',
        //   label: 'Delete',
        //   onClick: () => setOpenDeleteObjectiveForm(true),
        // },
      )
    }

    return items
  }, [canUpdateRoadmap])

  if (isLoading) {
    return <RoadmapDetailsLoading />
  }

  if (!isLoading && !roadmapData) {
    notFound()
  }

  const visibilityTag =
    roadmapData?.visibility?.name === 'Public' ? (
      <UnlockOutlined title="Public" />
    ) : (
      <LockOutlined title="Private" />
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
      {openEditRoadmapForm && (
        <EditRoadmapForm
          roadmapId={roadmapData?.id}
          showForm={openEditRoadmapForm}
          onFormComplete={() => onEditRoadmapFormClosed(true)}
          onFormCancel={() => onEditRoadmapFormClosed(false)}
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
