'use client'

import { FC, ReactNode, useMemo } from 'react'
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

  const cardTitle = useMemo(() => {
    const showBadge = count !== undefined && count > 0
    return (
      <Flex align="center" gap={8}>
        {title}
        {showBadge && <Badge color={badgeColor} size="small" count={count} />}
      </Flex>
    )
  }, [title, count, badgeColor])

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
