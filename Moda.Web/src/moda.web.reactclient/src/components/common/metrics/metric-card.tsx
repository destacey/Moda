import { Card, Flex, Statistic, StatisticProps, Tooltip } from 'antd'
import { FC } from 'react'

const { Meta } = Card

export interface MetricCardProps extends Omit<StatisticProps, 'valueStyle'> {
  cardStyle?: React.CSSProperties
  statisticStyle?: React.CSSProperties
  tooltip?: string
  secondaryValue?: string | number
  // Support both old valueStyle (for backwards compatibility) and new styles.content
  valueStyle?: React.CSSProperties
}

const MetricCard: FC<MetricCardProps> = ({
  cardStyle,
  statisticStyle,
  tooltip,
  secondaryValue,
  valueStyle,
  styles,
  ...statisticProps
}) => {
  const defaultCardStyle = cardStyle ?? { height: '100%' }
  const defaultStatisticStyle = statisticStyle ?? { whiteSpace: 'nowrap' }

  // Migrate deprecated valueStyle to new styles.content format
  const statisticStyles = valueStyle
    ? { ...styles, content: valueStyle }
    : styles

  const card = (
    <Card style={defaultCardStyle} size="small" hoverable>
      <Statistic
        {...statisticProps}
        style={defaultStatisticStyle}
        styles={statisticStyles}
      />
      {secondaryValue !== undefined && (
        <Meta description={<Flex justify="flex-end">{secondaryValue}</Flex>} />
      )}
    </Card>
  )

  return tooltip ? <Tooltip title={tooltip}>{card}</Tooltip> : card
}

export default MetricCard
