'use client'

import React, { FC } from 'react'
import { Flex, Grid } from 'antd'

const { useBreakpoint } = Grid

interface ResponsiveFlexProps {
  children: React.ReactNode
  gap?: string
  align?: string
}

const ResponsiveFlex: FC<ResponsiveFlexProps> = ({
  children,
  gap = 'middle',
  align = 'start',
}: ResponsiveFlexProps) => {
  const screens = useBreakpoint()
  const isSmallScreen = !screens.md

  return (
    <Flex gap={gap} align={align} vertical={isSmallScreen}>
      {children}
    </Flex>
  )
}

export default ResponsiveFlex
