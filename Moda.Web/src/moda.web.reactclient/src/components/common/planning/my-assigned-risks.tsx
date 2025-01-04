import { Card, List } from 'antd'
import { RiskListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useGetMyRisks } from '@/src/services/queries/planning-queries'

const { Item } = List

const riskMessage = (risk: RiskListDto) => {
  if (risk.followUpDate) {
    return `${risk.summary} (follow-up: ${risk.followUpDate})`
  }
  return risk.summary
}

const MyAssignedRisks = () => {
  const { data: risks } = useGetMyRisks()

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
