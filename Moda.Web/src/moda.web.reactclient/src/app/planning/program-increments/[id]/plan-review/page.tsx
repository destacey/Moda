'use client'

import { PageTitle } from '@/src/app/components/common'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { useDocumentTitle } from '@/src/app/hooks'
import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementTeamResponse,
} from '@/src/services/moda-api'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useEffect, useState } from 'react'
import { Card } from 'antd'
import Link from 'next/link'

const ProgramIncrementPlanReviewPage = ({ params }) => {
  useDocumentTitle('PI Plan Review')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto>()
  const [teams, setTeams] = useState<ProgramIncrementTeamResponse[]>([])
  const [activeTab, setActiveTab] = useState<string>()
  const { setBreadcrumbRoute } = useBreadcrumbs()

  useEffect(() => {
    const breadcrumbRoute: ItemType[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/program-increments`,
        title: 'Program Increments',
      },
    ]

    const getProgramIncrement = async () => {
      const programIncrementsClient = await getProgramIncrementsClient()
      const programIncrementDto = await programIncrementsClient.getByLocalId(
        params.id
      )
      setProgramIncrement(programIncrementDto)

      breadcrumbRoute.push(
        {
          href: `/planning/program-increments/${programIncrement?.localId}`,
          title: programIncrement?.name,
        },
        {
          title: 'Plan Review',
        }
      )
      // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
      setBreadcrumbRoute(breadcrumbRoute)

      var teams = await programIncrementsClient.getTeams(programIncrementDto.id)
      setTeams(
        teams
          .sort((a, b) => a.code.localeCompare(b.code))
          .filter((t) => t.type === 'Team')
      )
      setActiveTab(teams[0].localId.toString())
    }

    getProgramIncrement()
  }, [
    params.id,
    programIncrement?.localId,
    programIncrement?.name,
    setBreadcrumbRoute,
  ])

  const tabs = teams.map((team) => {
    return {
      key: team.localId.toString(),
      tab: team.code,
      content: <div>{team.code}</div>,
    }
  })

  const Actions = () => {
    return (
      <>
        <Link
          href={`/planning/program-increments/${programIncrement?.localId}`}
        >
          PI Details
        </Link>
      </>
    )
  }

  return (
    <>
      <PageTitle
        title={programIncrement?.name}
        subtitle="PI Plan Review"
        actions={<Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {/* <Row>
        <Col xs={24} sm={24} md={12}>
          Objectives
        </Col>
        <Col xs={24} sm={24} md={12}>
          Risks
        </Col>
      </Row> */}
    </>
  )
}

export default ProgramIncrementPlanReviewPage
