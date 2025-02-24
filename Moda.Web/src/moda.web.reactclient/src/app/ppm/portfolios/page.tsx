'use client'

import { PageTitle } from '@/src/components/common'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { useGetPortfoliosQuery } from '@/src/store/features/ppm/portfolios-api'
import { AppstoreOutlined, MenuOutlined } from '@ant-design/icons'
import { Button, message } from 'antd'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { useEffect, useMemo, useState } from 'react'
import {
  CreatePortfolioForm,
  PortfoliosCardGrid,
  PortfoliosGrid,
} from './_components'
import useAuth from '@/src/components/contexts/auth'

enum Views {
  Cards,
  List,
}

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: Views.Cards,
    icon: <AppstoreOutlined alt="Cards" title="Cards" />,
  },
  {
    value: Views.List,
    icon: <MenuOutlined alt="List" title="List" />,
  },
]

const PortfoliosPage: React.FC = () => {
  useDocumentTitle('Portfolios')
  const [currentView, setCurrentView] = useState<string | number>(Views.Cards)
  const [openCreatePortfolioForm, setOpenCreatePortfolioForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasPermissionClaim } = useAuth()
  const canCreatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Create',
  )
  const showActions = canCreatePortfolio

  const {
    data: portfolioData,
    isLoading,
    error,
    refetch,
  } = useGetPortfoliosQuery(null)

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load portfolios.')
    }
  }, [error, messageApi])

  const viewSelector = useMemo(
    () => (
      <Segmented
        options={viewSelectorOptions}
        value={currentView}
        onChange={setCurrentView}
      />
    ),
    [currentView],
  )

  const actions = useMemo(() => {
    if (!showActions) return null
    return (
      <>
        {canCreatePortfolio && (
          <Button onClick={() => setOpenCreatePortfolioForm(true)}>
            Create Portfolio
          </Button>
        )}
      </>
    )
  }, [canCreatePortfolio, showActions])

  const onCreatePortfolioFormClosed = (wasCreated: boolean) => {
    setOpenCreatePortfolioForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  return (
    <>
      {contextHolder}
      <PageTitle title="Portfolios" actions={actions} />
      {currentView === Views.Cards ? (
        <PortfoliosCardGrid
          portfolios={portfolioData}
          viewSelector={viewSelector}
          isLoading={isLoading}
          messageApi={messageApi}
        />
      ) : (
        <PortfoliosGrid
          portfolios={portfolioData}
          viewSelector={viewSelector}
          isLoading={isLoading}
          refetch={refetch}
          messageApi={messageApi}
        />
      )}
      {openCreatePortfolioForm && (
        <CreatePortfolioForm
          showForm={openCreatePortfolioForm}
          onFormComplete={() => onCreatePortfolioFormClosed(true)}
          onFormCancel={() => onCreatePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const PortfoliosPageWithAuthorization = authorizePage(
  PortfoliosPage,
  'Permission',
  'Permissions.ProjectPortfolios.View',
)

export default PortfoliosPageWithAuthorization
