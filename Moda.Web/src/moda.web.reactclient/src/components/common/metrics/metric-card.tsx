import { Card, Flex, Statistic, StatisticProps, Tooltip } from 'antd'
import { FC } from 'react'

const { Meta } = Card

export interface MetricCardProps extends StatisticProps {
  cardStyle?: React.CSSProperties
  statisticStyle?: React.CSSProperties
  tooltip?: string
  secondaryValue?: string | number
}

const MetricCard: FC<MetricCardProps> = ({
  cardStyle,
  statisticStyle,
  tooltip,
  secondaryValue,
  ...statisticProps
}) => {
  const defaultCardStyle = cardStyle ?? { height: '100%' }
  const defaultStatisticStyle = statisticStyle ?? { whiteSpace: 'nowrap' }

  const card = (
    <Card style={defaultCardStyle} size="small" hoverable>
      <Statistic {...statisticProps} style={defaultStatisticStyle} />
      {secondaryValue !== undefined && (
        <Meta description={<Flex justify="flex-end">{secondaryValue}</Flex>} />
      )}
    </Card>
  )

  return tooltip ? <Tooltip title={tooltip}>{card}</Tooltip> : card
}

export default MetricCard
