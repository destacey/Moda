'use client'

import { PageTitle } from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/app/hooks'
import { useGetRoadmapQuery } from '@/src/store/features/planning/roadmaps-api'
import { notFound, usePathname } from 'next/navigation'
import RoadmapDetailsLoading from './loading'
import { useEffect } from 'react'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'

const RoadmapDetailsPage = ({ params }) => {
  useDocumentTitle('Roadmap Details')

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePlanningInterval = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

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

  if (isLoading) {
    return <RoadmapDetailsLoading />
  }

  if (!isLoading && !roadmapData) {
    notFound()
  }
  return (
    <>
      <PageTitle
        title={roadmapData.name}
        subtitle="Roadmap Details"
        //actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <p>Key: {params.key}</p>
    </>
  )
}

const RoadmapDetailsPageWithAuthorization = authorizePage(
  RoadmapDetailsPage,
  'Permission',
  'Permissions.Roadmaps.View',
)

export default RoadmapDetailsPageWithAuthorization
