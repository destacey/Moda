'use client'

import { Tag } from 'antd'
import useTheme from '../contexts/theme'
import { FC } from 'react'

interface InactiveTagProps {
  isActive: boolean
}

const InactiveTag: FC<InactiveTagProps> = ({ isActive }) => {
  const { token } = useTheme()

  if (isActive === undefined || isActive === null || isActive) {
    return null
  }

  return <Tag color={token.colorWarning}>Inactive</Tag>
}

export default InactiveTag
