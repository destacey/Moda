import { RiskListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Empty, List } from 'antd'
import RiskListItem from './risk-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreateRiskForm from '@/src/app/components/common/planning/create-risk-form'

export interface TeamRisksListCardProps {
  risks: RiskListDto[]
  programIncrementId: string
  teamId: string
}

const TeamRisksListCard = ({
  risks,
  programIncrementId,
  teamId,
}: TeamRisksListCardProps) => {
  const [openCreateRiskForm, setOpenCreateRiskForm] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canCreateRisks = hasClaim('Permission', 'Permissions.Risks.Create')

  const cardTitle = () => {
    let title = `Risks`
    if (risks?.length > 0) {
      title += ` (${risks.length})`
    }
    return title
  }

  const RisksList = () => {
    if (!risks || risks.length === 0) {
      return <ModaEmpty message="No risks" />
    }

    const sortedRisks = risks.sort((a, b) => {
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
        renderItem={(risk) => <RiskListItem risk={risk} />}
      />
    )
  }

  const onCreateRiskFormClosed = (wasCreated: boolean) => {
    setOpenCreateRiskForm(false)
    if (wasCreated) {
      //loadRisks()
    }
  }

  return (
    <>
      <Card
        size="small"
        title={cardTitle()}
        extra={
          <Button
            type="text"
            icon={<PlusOutlined />}
            onClick={() => setOpenCreateRiskForm(true)}
          />
        }
      >
        <RisksList />
      </Card>
      <CreateRiskForm
        createForTeamId={teamId}
        showForm={openCreateRiskForm}
        onFormCreate={() => onCreateRiskFormClosed(true)}
        onFormCancel={() => onCreateRiskFormClosed(false)}
      />
    </>
  )
}

export default TeamRisksListCard
