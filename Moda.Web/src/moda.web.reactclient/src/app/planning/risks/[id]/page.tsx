'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getRisksClient } from '@/src/services/clients'
import { RiskDetailsDto } from '@/src/services/moda-api'
import { createElement, useEffect, useState } from 'react'
import RiskDetails from './risk-details'
import { Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'

const RiskDetailsPage = ({ params }) => {
  useDocumentTitle('Risk Details')
  const [activeTab, setActiveTab] = useState('details')
  const [risk, setRisk] = useState<RiskDetailsDto | null>(null)
  const { id } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()

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
  }, [id, setBreadcrumbTitle])

  return (
    <>
      <PageTitle
        title={`${risk?.localId} - ${risk?.summary}`}
        subtitle="Risk Details"
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
    </>
  )
}

export default RiskDetailsPage
