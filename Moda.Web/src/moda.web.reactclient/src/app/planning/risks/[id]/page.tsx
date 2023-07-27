'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getRisksClient } from '@/src/services/clients'
import { RiskDetailsDto } from '@/src/services/moda-api'
import { createElement, useEffect, useState } from 'react'
import RiskDetails from './risk-details'
import { Button, Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import useAuth from '@/src/app/components/contexts/auth'
import UpdateRiskForm from '@/src/app/components/common/planning/edit-risk-form'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'

const RiskDetailsPage = ({ params }) => {
  useDocumentTitle('Risk Details')
  const [activeTab, setActiveTab] = useState('details')
  const [risk, setRisk] = useState<RiskDetailsDto | null>(null)
  const { id } = params
  const { setBreadcrumbRoute } = useBreadcrumbs()
  const [openUpdateRiskForm, setOpenUpdateRiskForm] = useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canUpdateRisks = hasClaim('Permission', 'Permissions.Risks.Update')
  const showActions = canUpdateRisks

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: createElement(RiskDetails, risk),
    },
  ]

  useEffect(() => {
    const breadcrumbRoute: ItemType[] = [
      {
        title: 'Organizations',
      },
      {
        href: `/organizations/teams`,
        title: 'Teams',
      },
    ]

    const getRisk = async () => {
      const risksClient = await getRisksClient()
      const riskDto = await risksClient.getByLocalId(id)
      setRisk(riskDto)

      const teamRoute =
        riskDto.team?.type === 'Team' ? 'teams' : 'team-of-teams'

      breadcrumbRoute.push(
        {
          href: `/organizations/${teamRoute}/${riskDto.team?.localId}`,
          title: riskDto.team?.name,
        },
        {
          title: riskDto.summary,
        }
      )
      // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
      setBreadcrumbRoute(breadcrumbRoute)
    }

    getRisk()
  }, [id, lastRefresh, setBreadcrumbRoute])

  const onUpdateRiskFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRiskForm(false)
    if (wasSaved) {
      setLastRefresh(Date.now())
    }
  }

  const Actions = () => {
    return (
      <>
        {canUpdateRisks && (
          <Button onClick={() => setOpenUpdateRiskForm(true)}>Edit</Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle
        title={`${risk?.localId} - ${risk?.summary}`}
        subtitle="Risk Details"
        actions={showActions && <Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {canUpdateRisks && (
        <UpdateRiskForm
          showForm={openUpdateRiskForm}
          riskId={risk?.id}
          onFormSave={() => onUpdateRiskFormClosed(true)}
          onFormCancel={() => onUpdateRiskFormClosed(false)}
        />
      )}
    </>
  )
}

export default RiskDetailsPage
