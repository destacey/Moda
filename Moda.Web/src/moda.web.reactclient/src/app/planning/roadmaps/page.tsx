'use client'

import PageTitle from '@/src/components/common/page-title'
import { useCallback, useEffect, useState } from 'react'
import { useDocumentTitle } from '../../../hooks/use-document-title'
import useAuth from '../../../components/contexts/auth'
import { Button } from 'antd'
import { authorizePage } from '../../../components/hoc'
import { useGetRoadmapsQuery } from '@/src/store/features/planning/roadmaps-api'
import { CreateRoadmapForm, RoadmapsGrid } from './_components'
import { useMessage } from '@/src/components/contexts/messaging'

const RoadmapsPage: React.FC = () => {
  useDocumentTitle('Roadmaps')

  const [openCreateRoadmapForm, setOpenCreateRoadmapForm] =
    useState<boolean>(false)
  const messageApi = useMessage()

  const { data: roadmapData, isLoading, error, refetch } = useGetRoadmapsQuery()

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
      <PageTitle title="Roadmaps" actions={showActions && actions()} />
      <RoadmapsGrid
        roadmapsData={roadmapData || []}
        roadmapsLoading={isLoading}
        refreshRoadmaps={refresh}
      />
      {openCreateRoadmapForm && (
        <CreateRoadmapForm
          showForm={openCreateRoadmapForm}
          onFormComplete={() => onCreateRoadmapFormClosed(true)}
          onFormCancel={() => onCreateRoadmapFormClosed(false)}
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
