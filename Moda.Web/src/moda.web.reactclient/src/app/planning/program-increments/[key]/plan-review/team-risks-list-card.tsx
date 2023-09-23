'use client'

import { RiskListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Badge, Button, Card, List, Space } from 'antd'
import RiskListItem from './risk-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreateRiskForm from '@/src/app/components/common/planning/create-risk-form'
import { UseQueryResult } from 'react-query'
import useTheme from '@/src/app/components/contexts/theme'

export interface TeamRisksListCardProps {
  riskQuery: UseQueryResult<RiskListDto[], unknown>
  teamId: string
}

const TeamRisksListCard = ({ riskQuery, teamId }: TeamRisksListCardProps) => {
  const [openCreateRiskForm, setOpenCreateRiskForm] = useState<boolean>(false)
  const theme = useTheme()

  const { hasClaim } = useAuth()
  const canCreateRisks = hasClaim('Permission', 'Permissions.Risks.Create')
  const canUpdateRisks = hasClaim('Permission', 'Permissions.Risks.Update')

  const refreshRisks = useCallback(() => {
    riskQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const CardTitle = () => {
    const count = riskQuery?.data?.length ?? 0
    const showBadge = count > 0
    return (
      <Space>
        {'Risks'}
        {showBadge && (
          <Badge color={theme.badgeColor} size="small" count={count} />
        )}
      </Space>
    )
  }

  const RisksList = () => {
    if (!riskQuery?.data || riskQuery?.data.length === 0) {
      return <ModaEmpty message="No risks" />
    }

    const sortedRisks = riskQuery?.data.sort((a, b) => {
      const categoryOrder = ['Owned', 'Accepted', 'Mitigated', 'Resolved']
      const aIndex = categoryOrder.indexOf(a.category)
      const bIndex = categoryOrder.indexOf(b.category)
      if (aIndex !== bIndex) {
        return aIndex - bIndex
      } else {
        const exposureOrder = ['High', 'Medium', 'Low']
        const aExposureIndex = exposureOrder.indexOf(a.exposure)
        const bExposureIndex = exposureOrder.indexOf(b.exposure)
        return aExposureIndex - bExposureIndex
      }
    })

    return (
      <List
        size="small"
        dataSource={sortedRisks}
        renderItem={(risk) => (
          <RiskListItem
            risk={risk}
            canUpdateRisks={canUpdateRisks}
            refreshRisks={refreshRisks}
          />
        )}
      />
    )
  }

  const onCreateRiskFormClosed = (wasCreated: boolean) => {
    setOpenCreateRiskForm(false)
    if (wasCreated) {
      refreshRisks()
    }
  }

  return (
    <>
      <Card
        size="small"
        title={<CardTitle />}
        extra={
          canCreateRisks && (
            <Button
              type="text"
              icon={<PlusOutlined />}
              onClick={() => setOpenCreateRiskForm(true)}
            />
          )
        }
      >
        <RisksList />
      </Card>
      {openCreateRiskForm && (
        <CreateRiskForm
          createForTeamId={teamId}
          showForm={openCreateRiskForm}
          onFormCreate={() => onCreateRiskFormClosed(true)}
          onFormCancel={() => onCreateRiskFormClosed(false)}
        />
      )}
    </>
  )
}

export default TeamRisksListCard
