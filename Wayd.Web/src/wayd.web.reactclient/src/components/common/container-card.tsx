'use client'

import { FC, ReactNode } from 'react'
import useTheme from '../contexts/theme'
import { Badge, Card, Flex } from 'antd'

interface ContainerCardProps {
  title: string
  children: ReactNode
  count?: number
  actions?: ReactNode
}

const ContainerCard: FC<ContainerCardProps> = (props) => {
  const { title, children, count, actions } = props

  const { badgeColor } = useTheme()

  const showBadge = count !== undefined && count > 0
  const cardTitle = (
    <Flex align="center" gap="small">
      {title}
      {showBadge && (
        <Badge color={badgeColor} size="small" count={count} title="" />
      )}
    </Flex>
  )

  return (
    <Card
      size="small"
      title={cardTitle}
      extra={actions}
      style={{ margin: '24px 0 0 0' }}
    >
      {children}
    </Card>
  )
}

export default ContainerCard
