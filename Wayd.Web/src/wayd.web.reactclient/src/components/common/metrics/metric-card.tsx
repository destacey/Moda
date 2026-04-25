import { Card, Flex, Statistic, StatisticProps } from 'antd'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import { FC, ReactNode } from 'react'

const { Meta } = Card

export interface MetricCardProps extends Omit<StatisticProps, 'valueStyle'> {
  cardStyle?: React.CSSProperties
  statisticStyle?: React.CSSProperties
  tooltip?: string
  secondaryValue?: ReactNode
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

  return tooltip ? <WaydTooltip title={tooltip}>{card}</WaydTooltip> : card
}

export default MetricCard
