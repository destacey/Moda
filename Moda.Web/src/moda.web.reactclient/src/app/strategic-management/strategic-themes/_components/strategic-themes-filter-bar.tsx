'use client'

import { useGetStateOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { FC } from 'react'
import { PpmFilterBar } from '@/src/app/ppm/_components'

export interface StrategicThemesFilterBarProps {
  selectedStates: number[]
  onToggleState: (stateId: number) => void
}

const StrategicThemesFilterBar: FC<StrategicThemesFilterBarProps> = ({
  selectedStates,
  onToggleState,
}) => {
  const { data: stateOptions, isLoading } = useGetStateOptionsQuery()

  return (
    <PpmFilterBar
      statusOptions={stateOptions}
      selectedStatuses={selectedStates}
      onToggleStatus={onToggleState}
      loading={isLoading}
    />
  )
}

export default StrategicThemesFilterBar
