import { ProgramIncrementListDto } from '@/src/services/moda-api'
import { Card, Space } from 'antd'
import Link from 'next/link'

export interface ProgramIncrementCardProps {
  programIncrement: ProgramIncrementListDto
}

const ProgramIncrementCard = ({
  programIncrement,
}: ProgramIncrementCardProps) => {
  const start = new Date(programIncrement.start)
  const end = new Date(programIncrement.end)
  const now = new Date()
  const daysRemaining = Math.ceil(
    (end.getTime() - now.getTime()) / (1000 * 3600 * 24)
  )
  return (
    <Card
      size="small"
      title={programIncrement.name}
      extra={
        <Link href={`/planning/program-increments/${programIncrement.id}`}>
          Details
        </Link>
      }
    >
      <p>
        {start.toLocaleDateString()} - {end.toLocaleDateString()}
      </p>
      <p>{daysRemaining} days remaining</p>
    </Card>
  )
}

export default ProgramIncrementCard
