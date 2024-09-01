'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useDocumentTitle } from '../../hooks/use-document-title'
import useAuth from '../../components/contexts/auth'
import { Button } from 'antd'
import { authorizePage } from '../../components/hoc'
import { useGetRoadmapsQuery } from '@/src/store/features/planning/roadmaps-api'
import RoadmapsGrid from './components/roadmaps-grid'

const RoadmapsPage: React.FC = () => {
  useDocumentTitle('Roadmaps')

  const [openCreateRoadmapForm, setOpenCreateRoadmapForm] =
    useState<boolean>(false)

  const {
    data: roadmapData,
    isLoading,
    error,
    refetch,
  } = useGetRoadmapsQuery(null)

  const { hasClaim } = useAuth()
  const canCreateRoadmap = hasClaim('Permission', 'Permissions.Roadmaps.Create')
  const showActions = canCreateRoadmap

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const refresh = useCallback(async () => {
    refetch
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
      <br />
      <PageTitle title="Roadmaps" actions={showActions && actions()} />
      <RoadmapsGrid
        roadmapsData={roadmapData || []}
        roadmapsLoading={isLoading}
        refreshRoadmaps={refresh}
      />
      {/* {openCreatePlanningIntervalForm && (
        <CreatePlanningIntervalForm
          showForm={openCreatePlanningIntervalForm}
          onFormCreate={() => onCreatePlanningIntervalFormClosed(true)}
          onFormCancel={() => onCreatePlanningIntervalFormClosed(false)}
        />
      )} */}
    </>
  )
}

const RoadmapsPageWithAuthorization = authorizePage(
  RoadmapsPage,
  'Permission',
  'Permissions.Roadmaps.View',
)

export default RoadmapsPageWithAuthorization
