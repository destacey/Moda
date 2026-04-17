import { Card, List } from 'antd'
import { RiskListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useGetMyRisksQuery } from '@/src/store/features/planning/risks-api'
import useAuth from '../../contexts/auth'

const { Item } = List

const riskMessage = (risk: RiskListDto) => {
  if (risk.followUpDate) {
    return `${risk.summary} (follow-up: ${risk.followUpDate})`
  }
  return risk.summary
}

const MyAssignedRisks = () => {
  const { user } = useAuth()
  const { data: risks } = useGetMyRisksQuery(user?.username, {
    skip: !user?.username,
  })

  const hasAssignedRisks = risks?.length > 0

  if (!hasAssignedRisks) return null

  return (
    <>
      <Card size="small" title="My Assigned Risks">
        <List size="small">
          {risks.map((r) => (
            <Item key={r.key}>
              <Link href={`/planning/risks/${r.key}`}>{riskMessage(r)}</Link>
            </Item>
          ))}
        </List>
      </Card>
    </>
  )
}

export default MyAssignedRisks
