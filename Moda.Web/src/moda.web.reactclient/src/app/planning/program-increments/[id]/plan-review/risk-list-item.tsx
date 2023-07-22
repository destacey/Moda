import { RiskListDto } from '@/src/services/moda-api'
import { List, Typography } from 'antd'
import Link from 'next/link'

export interface RiskListItemProps {
  risk: RiskListDto
}

const RiskListItem = ({ risk }: RiskListItemProps) => {
  const title = () => {
    return (
      <Link href={`/planning/risks/${risk.localId}`}>
        {risk.localId} - {risk.summary}
      </Link>
    )
  }
  const description = () => {
    const content = `Category: ${risk.category} | Exposure: ${risk.exposure}`
    const assigneeInfo = risk.assignee ? (
      <>
        {' | Assignee: '}
        <Link href={`/organizations/employees/${risk.assignee.localId}`}>
          {risk.assignee.name}
        </Link>
      </>
    ) : null

    return (
      <Typography.Text>
        {content}
        {assigneeInfo}
      </Typography.Text>
    )
  }

  return (
    <List.Item key={risk.localId}>
      <List.Item.Meta title={title()} description={description()} />
    </List.Item>
  )
}

export default RiskListItem
