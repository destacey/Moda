import { Card, Flex } from 'antd'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import { FC, ReactNode } from 'react'

const { Meta } = Card

export interface ChartCardProps {
  title?: ReactNode
  children: ReactNode
  loading?: boolean
  cardStyle?: React.CSSProperties
  contentStyle?: React.CSSProperties
  tooltip?: string
  /**
   * Where the `tooltip` is anchored. Defaults to `'title'` so chart hover
   * interactions are not blocked by a card-level tooltip.
   */
  tooltipTarget?: 'card' | 'title'
  secondaryValue?: ReactNode
  /**
   * When true, renders without the card's border or hover affordance — for
   * cases where the chart sits inside another card and nested chrome would
   * feel doubled-up.
   */
  embedded?: boolean
}

const ChartCard: FC<ChartCardProps> = ({
  title,
  children,
  loading = false,
  cardStyle,
  contentStyle,
  tooltip,
  tooltipTarget = 'title',
  secondaryValue,
  embedded = false,
}) => {
  const defaultCardStyle = cardStyle ?? { height: '100%' }

  const titleNode =
    tooltip && tooltipTarget === 'title' ? (
      <WaydTooltip title={tooltip} helpCursor>
        <span>{title}</span>
      </WaydTooltip>
    ) : (
      title
    )

  const content = (
    <Flex vertical gap={0} style={contentStyle}>
      {title !== undefined && title !== null && (
        <div
          className="ant-statistic-title"
          style={{
            marginBottom: 0,
            color: 'var(--ant-color-text-description, rgba(0, 0, 0, 0.45))',
            fontSize: 14,
            fontWeight: 'normal',
            lineHeight: '22px',
          }}
        >
          {titleNode}
        </div>
      )}
      {children}
    </Flex>
  )

  const inner = embedded ? (
    <Flex vertical>
      {content}
      {secondaryValue !== undefined && (
        <Flex justify="flex-end">{secondaryValue}</Flex>
      )}
    </Flex>
  ) : (
    <Card style={defaultCardStyle} size="small" loading={loading}>
      {content}
      {secondaryValue !== undefined && (
        <Meta description={<Flex justify="flex-end">{secondaryValue}</Flex>} />
      )}
    </Card>
  )

  return tooltip && tooltipTarget === 'card' ? (
    <WaydTooltip title={tooltip} helpCursor>
      {inner}
    </WaydTooltip>
  ) : (
    inner
  )
}

export default ChartCard
