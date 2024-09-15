'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useEffect, useState } from 'react'
import { useDocumentTitle } from '../../hooks/use-document-title'
import useAuth from '../../components/contexts/auth'
import { Button, message } from 'antd'
import { authorizePage } from '../../components/hoc'
import { useGetRoadmapsQuery } from '@/src/store/features/planning/roadmaps-api'
import { CreateRoadmapForm, RoadmapsGrid } from './components'

const RoadmapsPage: React.FC = () => {
  useDocumentTitle('Roadmaps')

  const [openCreateRoadmapForm, setOpenCreateRoadmapForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const {
    data: roadmapData,
    isLoading,
    error,
    refetch,
  } = useGetRoadmapsQuery(null)

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')
  const showActions = canCreateRoadmap

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onCreateRoadmapFormClosed = (wasCreated: boolean) => {
    setOpenCreateRoadmapForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  const actions = () => {
    return (
      <>
        {canCreateRoadmap && (
          <Button onClick={() => setOpenCreateRoadmapForm(true)}>
            Create Roadmap
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      {contextHolder}
      <PageTitle title="Roadmaps" actions={showActions && actions()} />
      <RoadmapsGrid
        roadmapsData={roadmapData || []}
        roadmapsLoading={isLoading}
        refreshRoadmaps={refresh}
        messageApi={messageApi}
      />
      {openCreateRoadmapForm && (
        <CreateRoadmapForm
          showForm={openCreateRoadmapForm}
          onFormComplete={() => onCreateRoadmapFormClosed(true)}
          onFormCancel={() => onCreateRoadmapFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const RoadmapsPageWithAuthorization = authorizePage(
  RoadmapsPage,
  'Permission',
  'Permissions.Roadmaps.View',
)

export default RoadmapsPageWithAuthorization
