'use client'

import { PageTitle } from '@/src/components/common'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import {
  useGetSprintBacklogQuery,
  useGetSprintQuery,
} from '@/src/store/features/planning/sprints-api'
import { Card, Descriptions, Flex } from 'antd'
import { notFound, usePathname } from 'next/navigation'
import { use, useCallback, useEffect, useState } from 'react'
import SprintDetailsLoading from './loading'
import { SprintBacklogGrid, SprintDetails } from '../_components'

enum SprintTabs {
  Details = 'details',
  WorkItems = 'workItems',
}

const tabs = [
  {
    key: SprintTabs.Details,
    label: 'Details',
  },
  {
    key: SprintTabs.WorkItems,
    tab: 'Work Items',
  },
]

const SprintDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)
  const sprintKey = Number(key)

  const [activeTab, setActiveTab] = useState(SprintTabs.Details)
  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const { data: sprintData, isLoading } = useGetSprintQuery(sprintKey, {
    skip: !sprintKey,
  })

  const {
    data: workItemsData,
    isLoading: workItemsDataIsLoading,
    refetch: refetchWorkItemsData,
  } = useGetSprintBacklogQuery(sprintKey, {
    skip: !sprintKey,
  })

  useDocumentTitle(`${sprintData?.name ?? sprintKey} - Sprint Details`)

  useEffect(() => {
    if (!sprintData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/sprints`,
        title: 'Sprints',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, sprintData])

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case SprintTabs.Details:
        return <SprintDetails sprint={sprintData} />
      case SprintTabs.WorkItems:
        return (
          <SprintBacklogGrid
            workItems={workItemsData}
            isLoading={workItemsDataIsLoading}
            refetch={refetchWorkItemsData}
            hideTeamColumn={true}
          />
        )
      default:
        return null
    }
  }, [
    activeTab,
    refetchWorkItemsData,
    sprintData,
    workItemsData,
    workItemsDataIsLoading,
  ])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as SprintTabs)
  }, [])

  if (isLoading) {
    return <SprintDetailsLoading />
  }

  if (!sprintData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${sprintKey} - ${sprintData.name}`}
        subtitle="Sprint Details"
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
      </Card>
    </>
  )
}

const SprintDetailsPageWithAuthorization = authorizePage(
  SprintDetailsPage,
  'Permission',
  'Permissions.Iterations.View',
)

export default SprintDetailsPageWithAuthorization
