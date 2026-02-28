'use client'

import { EstimationScaleValueDto } from '@/src/services/moda-api'
import { Button, Flex } from 'antd'
import { FC } from 'react'

export interface EstimationCardDeckProps {
  values: EstimationScaleValueDto[]
  selectedValue?: string
  onSelect: (value: string) => void
  disabled?: boolean
}

const EstimationCardDeck: FC<EstimationCardDeckProps> = ({
  values,
  selectedValue,
  onSelect,
  disabled = false,
}) => {
  return (
    <Flex wrap gap={8}>
      {values.map((v) => (
        <Button
          key={v.value}
          type={selectedValue === v.value ? 'primary' : 'default'}
          size="large"
          onClick={() => onSelect(v.value)}
          disabled={disabled}
          style={{ minWidth: 56, height: 72, fontSize: 20, fontWeight: 600 }}
        >
          {v.value}
        </Button>
      ))}
    </Flex>
  )
}

export default EstimationCardDeck
