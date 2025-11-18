'use client'

import { IconMenu, PageTitle } from '@/src/components/common'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import {
  useGetSprintBacklogQuery,
  useGetSprintQuery,
} from '@/src/store/features/planning/sprints-api'
import { Divider, Flex, Space, Typography } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import SprintDetailsLoading from './loading'
import { SprintBacklogGrid, SprintDetails } from '../_components'
import { IterationStateTag } from '@/src/components/common/planning'
import { IterationState } from '@/src/components/types'
import LinksCard from '@/src/components/common/links/links-card'
import { useGetTeamSprintsQuery } from '@/src/store/features/organizations/team-api'
import { SwapOutlined } from '@ant-design/icons'

const { Title } = Typography

const SprintDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)
  const sprintKey = Number(key)

  const pathname = usePathname()
  const router = useRouter()
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

  const { data: teamSprints } = useGetTeamSprintsQuery(sprintData?.team.id, {
    skip: !sprintData?.team.id,
  })

  useEffect(() => {
    if (!sprintData) return

    dispatch(setBreadcrumbTitle({ title: sprintKey.toString(), pathname }))
  }, [dispatch, pathname, sprintData, sprintKey])

  const handleSprintChange = useCallback(
    (value: string | number) => {
      router.push(`/planning/sprints/${value}`)
    },
    [router],
  )

  const sprintsItems = useMemo(() => {
    if (!teamSprints) return []

    return [...teamSprints]
      .sort((a, b) => new Date(b.start).getTime() - new Date(a.start).getTime())
      .map((option) => ({
        label: option.name,
        extra: option.state.name,
        value: option.key,
      }))
  }, [teamSprints])

  const switchSprints = useMemo(() => {
    if (!sprintsItems.length) return null

    return (
      <IconMenu
        icon={<SwapOutlined />}
        tooltip="Switch to another team sprint"
        items={sprintsItems}
        selectedKeys={[sprintKey.toString()]}
        onChange={handleSprintChange}
      />
    )
  }, [sprintsItems, sprintKey, handleSprintChange])

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
          <Space>
            {switchSprints}
            <IterationStateTag state={sprintData.state.id as IterationState} />
          </Space>
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
