'use client'

import PageTitle from '@/src/components/common/page-title'
import { FC, useEffect, useState } from 'react'
import { useDocumentTitle } from '../../../hooks/use-document-title'
import useAuth from '../../../components/contexts/auth'
import { Button } from 'antd'
import { authorizePage } from '../../../components/hoc'
import {
  ROADMAP_STATE,
  useGetRoadmapsQuery,
} from '@/src/store/features/planning/roadmaps-api'
import {
  CreateRoadmapForm,
  RoadmapsFilterBar,
  RoadmapsGrid,
} from './_components'
import { useMessage } from '@/src/components/contexts/messaging'

const DEFAULT_STATES = [ROADMAP_STATE.Active]

const RoadmapsPage: FC = () => {
  useDocumentTitle('Roadmaps')

  const [openCreateRoadmapForm, setOpenCreateRoadmapForm] =
    useState<boolean>(false)
  const [selectedStates, setSelectedStates] = useState<number[]>(DEFAULT_STATES)

  const messageApi = useMessage()

  const {
    data: roadmapData,
    isLoading,
    error,
    refetch,
  } = useGetRoadmapsQuery({
    state: selectedStates.length > 0 ? selectedStates : undefined,
  })

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load roadmaps.')
    }
  }, [error, messageApi])

  const { hasPermissionClaim } = useAuth()
  const canCreateRoadmap = hasPermissionClaim('Permissions.Roadmaps.Create')
  const showActions = canCreateRoadmap

  const handleStateChange = (states: number[]) => {
    setSelectedStates(states)
  }

  const refresh = async () => {
    refetch()
  }

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
      <RoadmapsFilterBar
        selectedStates={selectedStates}
        onStateChange={handleStateChange}
      />
      <RoadmapsGrid
        roadmapsData={roadmapData || []}
        roadmapsLoading={isLoading}
        refreshRoadmaps={refresh}
      />
      {openCreateRoadmapForm && (
        <CreateRoadmapForm
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
