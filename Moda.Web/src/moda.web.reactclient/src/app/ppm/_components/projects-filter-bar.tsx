'use client'

import { useGetProjectStatusOptionsQuery } from '@/src/store/features/ppm/projects-api'
import { useGetPortfolioOptionsQuery } from '@/src/store/features/ppm/portfolios-api'
import { FC } from 'react'
import PpmFilterBar from './ppm-filter-bar'

export interface ProjectsFilterBarProps {
  selectedStatuses: number[]
  onStatusChange: (statuses: number[]) => void
  selectedPortfolioId?: string | undefined
  onPortfolioChange?: (portfolioId: string | undefined) => void
  showPortfolioFilter?: boolean
}

const ProjectsFilterBar: FC<ProjectsFilterBarProps> = ({
  selectedStatuses,
  onStatusChange,
  selectedPortfolioId,
  onPortfolioChange,
  showPortfolioFilter = true,
}) => {
  const { data: statusOptions, isLoading: statusOptionsLoading } =
    useGetProjectStatusOptionsQuery()

  const { data: portfolioOptions, isLoading: portfolioOptionsLoading } =
    useGetPortfolioOptionsQuery(undefined, { skip: !showPortfolioFilter })

  const loading =
    statusOptionsLoading || (showPortfolioFilter && portfolioOptionsLoading)

  return (
    <PpmFilterBar
      statusOptions={statusOptions}
      selectedStatuses={selectedStatuses}
      onStatusChange={onStatusChange}
      portfolioOptions={showPortfolioFilter ? portfolioOptions : undefined}
      selectedPortfolioId={showPortfolioFilter ? selectedPortfolioId : undefined}
      onPortfolioChange={showPortfolioFilter ? onPortfolioChange : undefined}
      loading={loading}
    />
  )
}

export default ProjectsFilterBar
