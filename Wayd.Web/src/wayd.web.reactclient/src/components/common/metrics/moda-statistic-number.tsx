import StatisticNumber from 'antd/lib/statistic/Number'
import { valueType } from 'antd/lib/statistic/utils'
import { FC } from 'react'

export interface ModaStatisticNumberProps {
  value?: valueType
  precision?: number
}

export const ModaStatisticNumber: FC<ModaStatisticNumberProps> = ({
  value,
  precision,
}) => {
  if (value === null || value === undefined) return null

  // TODO: the precision prop doesn't round the number correctly, it just cuts it off.

  return (
    <StatisticNumber
      value={value}
      precision={precision}
      decimalSeparator="."
      groupSeparator=","
    />
  )
}

export default ModaStatisticNumber
