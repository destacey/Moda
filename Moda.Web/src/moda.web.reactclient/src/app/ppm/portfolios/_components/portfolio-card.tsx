import { MarkdownRenderer } from '@/src/components/common/markdown'
import { ProjectPortfolioListDto } from '@/src/services/moda-api'
import { Card, Descriptions, Tag, Typography } from 'antd'
import { useRouter } from 'next/navigation'

const { Item: DiscriptionItem } = Descriptions
const { Text } = Typography

export interface PortfolioCardProps {
  portfolio: ProjectPortfolioListDto
}

const PortfolioCard: React.FC<PortfolioCardProps> = (
  props: PortfolioCardProps,
) => {
  const router = useRouter()

  return (
    <Card
      title={props.portfolio.name}
      size="small"
      style={{ width: '100%' }}
      hoverable
      onClick={() => router.push(`/ppm/portfolios/${props.portfolio.key}`)}
    >
      <Descriptions column={1} size="small">
        <DiscriptionItem label="Key">{props.portfolio.key}</DiscriptionItem>
        <DiscriptionItem>
          <MarkdownRenderer markdown={props.portfolio.description} />
        </DiscriptionItem>
      </Descriptions>
      {props.portfolio?.status.name && (
        <Tag>{props.portfolio?.status.name}</Tag>
      )}
    </Card>
  )
}

export default PortfolioCard
