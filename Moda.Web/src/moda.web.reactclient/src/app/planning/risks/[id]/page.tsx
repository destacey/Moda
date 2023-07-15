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
import UpdateRiskForm from '@/src/app/components/common/planning/update-risk-form'

const RiskDetailsPage = ({ params }) => {
  useDocumentTitle('Risk Details')
  const [activeTab, setActiveTab] = useState('details')
  const [risk, setRisk] = useState<RiskDetailsDto | null>(null)
  const { id } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()
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
    const getRisk = async () => {
      const risksClient = await getRisksClient()
      const riskDto = await risksClient.getByLocalId(id)
      setRisk(riskDto)
      setBreadcrumbTitle(riskDto.summary)
    }

    getRisk()
  }, [id, setBreadcrumbTitle, lastRefresh])

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
      <UpdateRiskForm
        showForm={openUpdateRiskForm}
        riskId={risk?.id}
        onFormSave={() => onUpdateRiskFormClosed(true)}
        onFormCancel={() => onUpdateRiskFormClosed(false)}
      />
    </>
  )
}

export default RiskDetailsPage
