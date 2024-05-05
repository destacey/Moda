'use client'

import { RiskListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Badge, Button, Card, List, Space } from 'antd'
import RiskListItem from './risk-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useMemo, useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreateRiskForm from '@/src/app/components/common/planning/create-risk-form'
import { UseQueryResult } from 'react-query'
import useTheme from '@/src/app/components/contexts/theme'

export interface TeamRisksListCardProps {
  riskQuery: UseQueryResult<RiskListDto[], unknown>
  teamId: string
}

const categoryOrder = ['Owned', 'Accepted', 'Mitigated', 'Resolved']
const exposureOrder = ['High', 'Medium', 'Low']

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

  const cardTitle = useMemo(() => {
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
  }, [riskQuery?.data?.length, theme.badgeColor])

  const risksList = useMemo(() => {
    const sortedRisks = riskQuery?.data?.sort((a, b) => {
      const aIndex = categoryOrder.indexOf(a.category)
      const bIndex = categoryOrder.indexOf(b.category)
      if (aIndex !== bIndex) {
        return aIndex - bIndex
      } else {
        const aExposureIndex = exposureOrder.indexOf(a.exposure)
        const bExposureIndex = exposureOrder.indexOf(b.exposure)
        return aExposureIndex - bExposureIndex
      }
    })

    return (
      <List
        size="small"
        dataSource={sortedRisks}
        locale={{
          emptyText: <ModaEmpty message="No risks" />,
        }}
        renderItem={(risk) => (
          <RiskListItem
            risk={risk}
            canUpdateRisks={canUpdateRisks}
            refreshRisks={refreshRisks}
          />
        )}
      />
    )
  }, [canUpdateRisks, refreshRisks, riskQuery?.data])

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
        title={cardTitle}
        extra={
          canCreateRisks && (
            <Button
              type="text"
              icon={<PlusOutlined />}
              onClick={() => setOpenCreateRiskForm(true)}
            />
          )
        }
        styles={{ body: { padding: 4 } }}
      >
        {risksList}
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
