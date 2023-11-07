import EditRiskForm from '@/src/app/components/common/planning/edit-risk-form'
import { RiskListDto } from '@/src/services/moda-api'
import { EditOutlined } from '@ant-design/icons'
import { Button, List, Space, Tag, Typography } from 'antd'
import Link from 'next/link'
import { useState } from 'react'

export interface RiskListItemProps {
  risk: RiskListDto
  canUpdateRisks: boolean
  refreshRisks: () => void
}

const RiskListItem = ({
  risk,
  canUpdateRisks,
  refreshRisks,
}: RiskListItemProps) => {
  const [openUpdateRiskForm, setOpenUpdateRiskForm] = useState<boolean>(false)

  const title = () => {
    return (
      <Link href={`/planning/risks/${risk.key}`}>
        {risk.key} - {risk.summary}
      </Link>
    )
  }
  const description = () => {
    const content = `Exposure: ${risk.exposure}`
    const assigneeInfo = risk.assignee ? (
      <>
        {' | Assignee: '}
        <Link href={`/organizations/employees/${risk.assignee.key}`}>
          {risk.assignee.name}
        </Link>
      </>
    ) : null

    return (
      <Space wrap>
        <Tag>{risk.category}</Tag>
        <Typography.Text>
          {content}
          {assigneeInfo}
        </Typography.Text>
      </Space>
    )
  }

  const onEditRiskFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRiskForm(false)
    if (wasSaved) {
      refreshRisks()
    }
  }

  return (
    <>
      <List.Item key={risk.key}>
        <List.Item.Meta title={title()} description={description()} />
        {canUpdateRisks && (
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => setOpenUpdateRiskForm(true)}
          />
        )}
      </List.Item>
      {openUpdateRiskForm && (
        <EditRiskForm
          showForm={openUpdateRiskForm}
          riskId={risk.id}
          onFormSave={() => onEditRiskFormClosed(true)}
          onFormCancel={() => onEditRiskFormClosed(false)}
        />
      )}
    </>
  )
}

export default RiskListItem
