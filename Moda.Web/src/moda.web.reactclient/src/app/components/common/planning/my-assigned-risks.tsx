import { Card, List } from 'antd'
import { RiskListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useGetMyRisks } from '@/src/services/queries/planning-queries'

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
            <List.Item key={r.localId}>
              <Link href={`/planning/risks/${r.localId}`}>
                {riskMessage(r)}
              </Link>
            </List.Item>
          ))}
        </List>
      </Card>
    </>
  )
}

export default MyAssignedRisks
