'use client'

import { PageTitle } from '@/src/components/common'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import {
  BreadcrumbItem,
  setBreadcrumbRoute,
  setBreadcrumbTitle,
} from '@/src/store/breadcrumbs'
import {
  useGetSprintBacklogQuery,
  useGetSprintQuery,
} from '@/src/store/features/planning/sprints-api'
import { Card, Descriptions, Divider, Flex, Typography } from 'antd'
import { notFound, usePathname } from 'next/navigation'
import { use, useCallback, useEffect, useState } from 'react'
import SprintDetailsLoading from './loading'
import { SprintBacklogGrid, SprintDetails } from '../_components'
import { IterationStateTag } from '@/src/components/common/planning'
import { IterationState } from '@/src/components/types'
import LinksCard from '@/src/components/common/links/links-card'

const { Title } = Typography

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

    dispatch(setBreadcrumbTitle({ title: sprintKey.toString(), pathname }))
  }, [dispatch, pathname, sprintData, sprintKey])

  // const renderTabContent = useCallback(() => {
  //   switch (activeTab) {
  //     case SprintTabs.Details:
  //       return <SprintDetails sprint={sprintData} backlog={workItemsData} />
  //     case SprintTabs.WorkItems:
  //       return (
  //         <SprintBacklogGrid
  //           workItems={workItemsData}
  //           isLoading={workItemsDataIsLoading}
  //           refetch={refetchWorkItemsData}
  //           hideTeamColumn={true}
  //         />
  //       )
  //     default:
  //       return null
  //   }
  // }, [
  //   activeTab,
  //   refetchWorkItemsData,
  //   sprintData,
  //   workItemsData,
  //   workItemsDataIsLoading,
  // ])

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
        tags={
          <IterationStateTag state={sprintData.state.id as IterationState} />
        }
      />
      <Flex vertical gap="middle">
        <SprintDetails sprint={sprintData} backlog={workItemsData} />
        <Divider size="small" />
        <Flex vertical>
          <Title level={4} style={{ marginBlockStart: '4px' }}>
            Backlog
          </Title>
          <SprintBacklogGrid
            workItems={workItemsData}
            isLoading={workItemsDataIsLoading}
            refetch={refetchWorkItemsData}
            hideTeamColumn={true}
          />
        </Flex>
        <Divider size="small" />
        <LinksCard objectId={sprintData.id} />
      </Flex>
    </>
  )
}

const SprintDetailsPageWithAuthorization = authorizePage(
  SprintDetailsPage,
  'Permission',
  'Permissions.Iterations.View',
)

export default SprintDetailsPageWithAuthorization
