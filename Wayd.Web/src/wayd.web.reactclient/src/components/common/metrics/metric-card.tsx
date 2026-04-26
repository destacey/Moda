import { Card, Flex, Statistic, StatisticProps } from 'antd'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import { FC, ReactNode } from 'react'

const { Meta } = Card

export interface MetricCardProps extends Omit<StatisticProps, 'valueStyle'> {
  cardStyle?: React.CSSProperties
  statisticStyle?: React.CSSProperties
  tooltip?: string
  /**
   * Where the `tooltip` is anchored. Defaults to `'card'` — the entire card
   * is the hover target. Use `'title'` when the metric has its own child
   * tooltips (e.g. icon-stat secondaries) so hovers don't double up.
   */
  tooltipTarget?: 'card' | 'title'
  secondaryValue?: ReactNode
  // Support both old valueStyle (for backwards compatibility) and new styles.content
  valueStyle?: React.CSSProperties
  /**
   * When true, renders without the card's border or hover affordance — for
   * cases where the metric sits inside another card and the nested chrome
   * would feel doubled-up.
   */
  embedded?: boolean
}

const MetricCard: FC<MetricCardProps> = ({
  cardStyle,
  statisticStyle,
  tooltip,
  tooltipTarget = 'card',
  secondaryValue,
  valueStyle,
  styles,
  embedded = false,
  title,
  ...statisticProps
}) => {
  const defaultCardStyle = cardStyle ?? { height: '100%' }
  const defaultStatisticStyle = statisticStyle ?? { whiteSpace: 'nowrap' }

  // Migrate deprecated valueStyle to new styles.content format
  const statisticStyles = valueStyle
    ? { ...styles, content: valueStyle }
    : styles

  const titleNode =
    tooltip && tooltipTarget === 'title' ? (
      <WaydTooltip title={tooltip}>
        <span>{title}</span>
      </WaydTooltip>
    ) : (
      title
    )

  // Embedded mode: skip the Card wrapper entirely — the metric is nested in
  // another card already, so the body padding / background of an inner card
  // reads as visual noise even with `variant="borderless"`.
  const inner = embedded ? (
    <Flex vertical>
      <Statistic
        {...statisticProps}
        title={titleNode}
        style={defaultStatisticStyle}
        styles={statisticStyles}
      />
      {secondaryValue !== undefined && (
        <Flex justify="flex-end">{secondaryValue}</Flex>
      )}
    </Flex>
  ) : (
    <Card style={defaultCardStyle} size="small" hoverable>
      <Statistic
        {...statisticProps}
        title={titleNode}
        style={defaultStatisticStyle}
        styles={statisticStyles}
      />
      {secondaryValue !== undefined && (
        <Meta description={<Flex justify="flex-end">{secondaryValue}</Flex>} />
      )}
    </Card>
  )

  return tooltip && tooltipTarget === 'card' ? (
    <WaydTooltip title={tooltip}>{inner}</WaydTooltip>
  ) : (
    inner
  )
}

export default MetricCard
