'use client'

import PageTitle from '@/src/components/common/page-title'
import { use, useEffect, useState } from 'react'
import RiskDetails from './risk-details'
import { Button, Card } from 'antd'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import useAuth from '@/src/components/contexts/auth'
import EditRiskForm from '@/src/components/common/planning/edit-risk-form'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { useGetRiskQuery } from '@/src/store/features/planning/risks-api'

const RiskDetailsPage = (props: { params: Promise<{ key: number }> }) => {
  const { key: riskKey } = use(props.params)

  useDocumentTitle('Risk Details')

  const [activeTab, setActiveTab] = useState('details')
  const [openUpdateRiskForm, setOpenUpdateRiskForm] = useState<boolean>(false)
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const { data: riskData, isLoading, error, refetch } = useGetRiskQuery(riskKey)

  const { hasClaim } = useAuth()
  const canUpdateRisks = hasClaim('Permission', 'Permissions.Risks.Update')
  const showActions = canUpdateRisks

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <RiskDetails risk={riskData} />,
    },
  ]

  useEffect(() => {
    if (!riskData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Organizations',
      },
      {
        href: `/organizations/teams`,
        title: 'Teams',
      },
    ]

    const teamRoute = riskData.team?.type === 'Team' ? 'teams' : 'team-of-teams'

    breadcrumbRoute.push(
      {
        href: `/organizations/${teamRoute}/${riskData.team?.key}`,
        title: riskData.team?.name,
      },
      {
        title: riskData.summary,
      },
    )
    // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [riskData, dispatch, pathname])

  const onUpdateRiskFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRiskForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  if (!isLoading && !riskData) {
    notFound()
  }

  const actions = () => {
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
        title={`${riskKey} - ${riskData?.summary}`}
        subtitle="Risk Details"
        actions={showActions && actions()}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openUpdateRiskForm && (
        <EditRiskForm
          showForm={openUpdateRiskForm}
          riskKey={riskKey}
          onFormSave={() => onUpdateRiskFormClosed(true)}
          onFormCancel={() => onUpdateRiskFormClosed(false)}
        />
      )}
    </>
  )
}

const RiskDetailsPageWithAuthorization = authorizePage(
  RiskDetailsPage,
  'Permission',
  'Permissions.Risks.View',
)

export default RiskDetailsPageWithAuthorization
