'use client'

import { LifecycleNavigationDto } from '@/src/services/moda-api'
import { FC, memo } from 'react'
import { Tag } from 'antd'
import { LifecyclePhase } from '../types'
import { getLifecyclePhaseTagColor } from '@/src/utils'

export interface LifecycleStatusTagProps {
  status: LifecycleNavigationDto
}

const LifecycleStatusTag: FC<LifecycleStatusTagProps> = ({ status }) => {
  const phase =
    LifecyclePhase[status.lifecyclePhase as keyof typeof LifecyclePhase]
  const color = getLifecyclePhaseTagColor(phase)

  return <Tag color={color}>{status.name}</Tag>
}

export default memo(LifecycleStatusTag)
