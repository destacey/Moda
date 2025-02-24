'use client'

import { ProjectPortfolioListDto } from '@/src/services/moda-api'
import { Flex, List } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { ReactElement, useMemo } from 'react'
import PortfolioCard from './portfolio-card'
import { ModaEmpty } from '@/src/components/common'

const { Item: ListItem } = List

export interface PortfoliosCardGridProps {
  portfolios: ProjectPortfolioListDto[]
  viewSelector: ReactElement
  isLoading: boolean
  messageApi: MessageInstance
}

const gridConfig = {
  gutter: 16,
  xs: 1,
  sm: 1,
  md: 2,
  lg: 2,
  xl: 3,
  xxl: 4,
}

const PortfoliosCardGrid: React.FC<PortfoliosCardGridProps> = (
  props: PortfoliosCardGridProps,
) => {
  const sortedPortfolios = useMemo<ProjectPortfolioListDto[]>(() => {
    if (!props.portfolios || props.portfolios.length === 0) {
      return []
    }
    const activePortfolios = props.portfolios.filter(
      (portfolio) =>
        portfolio.status.name === 'Active' ||
        portfolio.status.name === 'On Hold',
    )
    return activePortfolios.sort((a, b) => a.name.localeCompare(b.name))
  }, [props.portfolios])

  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        {props.viewSelector}
      </Flex>
      <List
        grid={gridConfig}
        loading={{
          spinning: props.isLoading,
          tip: 'Loading portfolios...',
          size: 'large',
        }}
        locale={{
          emptyText: <ModaEmpty message="No active portfolios found" />,
        }}
        dataSource={sortedPortfolios}
        renderItem={(item: ProjectPortfolioListDto) => (
          <ListItem>
            <PortfolioCard portfolio={item} />
          </ListItem>
        )}
      />
    </>
  )
}

export default PortfoliosCardGrid
