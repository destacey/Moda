'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useState } from 'react'
import RiskDetails from './risk-details'
import { Button, Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import EditRiskForm from '@/src/app/components/common/planning/edit-risk-form'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useGetRisk } from '@/src/services/queries/planning-queries'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'

const RiskDetailsPage = ({ params }) => {
  useDocumentTitle('Risk Details')
  const {
    data: riskData,
    isLoading,
    isFetching,
    refetch,
  } = useGetRisk(params.key)
  const [activeTab, setActiveTab] = useState('details')
  const [openUpdateRiskForm, setOpenUpdateRiskForm] = useState<boolean>(false)
  const dispatch = useAppDispatch()
  const pathname = usePathname()

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

  if (!isLoading && !isFetching && !riskData) {
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
        title={`${riskData?.key} - ${riskData?.summary}`}
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
          riskId={riskData?.id}
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
