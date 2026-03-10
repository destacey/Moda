'use client'

import { LifecyclePhase } from '@/src/components/types'
import { getLifecyclePhaseTagColor } from '@/src/utils'
import { Card, Flex, Select, Skeleton, Space, Tag } from 'antd'
import { BaseOptionType } from 'antd/es/select'
import { FC } from 'react'
import styles from './ppm-filter-bar.module.css'

export interface PpmFilterBarProps {
  statusOptions: { value: number; label: string }[] | undefined
  selectedStatuses: number[]
  onToggleStatus: (statusId: number) => void
  portfolioOptions?: BaseOptionType[] | undefined
  selectedPortfolioId?: string | undefined
  onPortfolioChange?: (portfolioId: string | undefined) => void
  loading?: boolean
}

// Map status names to lifecycle phases for tag coloring
const STATUS_LIFECYCLE_PHASE: Record<string, LifecyclePhase> = {
  Proposed: LifecyclePhase.NotStarted,
  Approved: LifecyclePhase.NotStarted,
  Active: LifecyclePhase.Active,
  'On Hold': LifecyclePhase.Active,
  Completed: LifecyclePhase.Done,
  Cancelled: LifecyclePhase.Done,
}

const hasPortfolioFilter = (
  props: PpmFilterBarProps,
): props is PpmFilterBarProps &
  Required<
    Pick<
      PpmFilterBarProps,
      'portfolioOptions' | 'selectedPortfolioId' | 'onPortfolioChange'
    >
  > => {
  return props.onPortfolioChange !== undefined
}

const PpmFilterBar: FC<PpmFilterBarProps> = (props) => {
  const { statusOptions, selectedStatuses, onToggleStatus, loading } = props

  if (loading) {
    return (
      <Card size="small" className={styles.filterCard}>
        <Skeleton.Input active size="small" className={styles.skeleton} />
      </Card>
    )
  }

  return (
    <Card className={styles.filterCard}>
      <Flex align="center" gap={16} wrap>
        {hasPortfolioFilter(props) && (
          <Space size="middle" align="center">
            <span className={styles.filterLabel}>Portfolio:</span>
            <Select
              placeholder="All Portfolios"
              allowClear
              value={props.selectedPortfolioId}
              onChange={(value) => props.onPortfolioChange(value)}
              options={props.portfolioOptions}
              className={styles.portfolioSelect}
              popupMatchSelectWidth={false}
            />
          </Space>
        )}

        <Space size="small" align="center">
          <span className={styles.filterLabel}>Status:</span>
          {statusOptions?.map((status) => {
            const isSelected = selectedStatuses.includes(status.value)
            const phase =
              STATUS_LIFECYCLE_PHASE[status.label] ?? LifecyclePhase.NotStarted
            const color = isSelected
              ? getLifecyclePhaseTagColor(phase)
              : undefined

            return (
              <Tag
                key={status.value}
                color={isSelected ? color : undefined}
                onClick={() => onToggleStatus(status.value)}
                className={
                  isSelected ? styles.statusTag : styles.statusTagInactive
                }
              >
                {status.label.toUpperCase()}
              </Tag>
            )
          })}
        </Space>
      </Flex>
    </Card>
  )
}

export default PpmFilterBar
