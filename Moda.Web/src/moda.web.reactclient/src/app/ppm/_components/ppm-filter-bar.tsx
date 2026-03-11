'use client'

import { Button, Card, Flex, Select, Skeleton, Space } from 'antd'
import { BaseOptionType } from 'antd/es/select'
import { FC } from 'react'
import styles from './ppm-filter-bar.module.css'

export interface PpmFilterBarProps {
  statusOptions: { value: number; label: string }[] | undefined
  selectedStatuses: number[]
  onStatusChange: (statuses: number[]) => void
  portfolioOptions?: BaseOptionType[] | undefined
  selectedPortfolioId?: string | undefined
  onPortfolioChange?: (portfolioId: string | undefined) => void
  loading?: boolean
}

const hasPortfolioFilter = (
  props: PpmFilterBarProps,
): props is PpmFilterBarProps &
  Required<
    Pick<PpmFilterBarProps, 'portfolioOptions' | 'onPortfolioChange'>
  > => {
  return (
    props.onPortfolioChange !== undefined &&
    props.portfolioOptions !== undefined
  )
}

const PpmFilterBar: FC<PpmFilterBarProps> = (props) => {
  const { statusOptions, selectedStatuses, onStatusChange, loading } = props

  if (loading) {
    return (
      <Card size="small" className={styles.filterCard}>
        <Skeleton.Input active size="small" className={styles.skeleton} />
      </Card>
    )
  }

  return (
    <div className={styles.filterCard}>
      <Flex align="center" gap={16} wrap>
        {hasPortfolioFilter(props) && (
          <Space size="middle" align="center">
            <span className={styles.filterLabel}>Portfolio:</span>
            <Select
              placeholder="All Portfolios"
              size="small"
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
          <Flex gap={2}>
            {statusOptions?.map((status) => {
              const isSelected = selectedStatuses.includes(status.value)
              return (
                <Button
                  key={status.value}
                  size="small"
                  className={styles.statusButton}
                  color={isSelected ? 'primary' : 'default'}
                  variant="outlined"
                  onClick={() => {
                    const next = isSelected
                      ? selectedStatuses.filter((s) => s !== status.value)
                      : [...selectedStatuses, status.value]
                    onStatusChange(next)
                  }}
                >
                  {status.label}
                </Button>
              )
            })}
          </Flex>
        </Space>
      </Flex>
    </div>
  )
}

export default PpmFilterBar
