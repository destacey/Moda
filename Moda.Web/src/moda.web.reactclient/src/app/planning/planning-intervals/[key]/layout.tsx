'use client'

import { Layout, Menu, MenuProps } from 'antd'
import Link from 'next/link'
import { useMemo, useState } from 'react'

const { Content } = Layout

const PlanninIntervalLayout = ({
  params,
  children,
}: {
  params: any
  children: React.ReactNode
}) => {
  const items: MenuProps['items'] = useMemo(
    () => [
      {
        label: (
          <Link href={`/planning/planning-intervals/${params.key}`}>
            PI Details
          </Link>
        ),
        key: 'pi-details',
      },
      {
        label: (
          <Link href={`/planning/planning-intervals/${params.key}/plan-review`}>
            Plan Review
          </Link>
        ),
        key: 'pi-plan-review',
      },
      //   {
      //     label: 'Objectives',
      //     key: 'pi-objectives',
      //   },
      //   {
      //     label: 'Risks',
      //     key: 'pi-risks',
      //   },
      {
        label: 'Reports',
        key: 'pi-reports',
        children: [
          {
            label: (
              <Link
                href={`/planning/planning-intervals/${params.key}/objectives/health-report`}
              >
                Health Report
              </Link>
            ),
            key: 'pi-reports-health-report',
          },
        ],
      },
    ],
    [params.key],
  )

  return (
    <>
      <Menu
        selectable={false}
        style={{
          marginBottom: 12,
          borderColor: 'transparent',
        }}
        mode="horizontal"
        items={items}
      />
      {children}
    </>
  )
}

export default PlanninIntervalLayout
