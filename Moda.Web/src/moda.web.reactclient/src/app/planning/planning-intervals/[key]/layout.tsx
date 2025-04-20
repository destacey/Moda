'use client'

import { useGetPlanningInterval } from '@/src/services/queries/planning-queries'
import { Menu } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import Link from 'next/link'
import { useMemo, use } from 'react'

const PlanningIntervalLayout = (props: {
  params: Promise<{ key: string }>
  children: React.ReactNode
}) => {
  const { key } = use(props.params)

  const { children } = props

  const { data: planningIntervalData } = useGetPlanningInterval(key)

  const items = useMemo(
    () =>
      [
        {
          label: `Planning Interval: ${planningIntervalData?.name}`,
          key: 'pi-name',
          type: 'text',
          disabled: true,
        },
        {
          label: (
            <Link href={`/planning/planning-intervals/${key}`}>PI Details</Link>
          ),
          key: 'pi-details',
        },
        {
          label: (
            <Link href={`/planning/planning-intervals/${key}/plan-review`}>
              Plan Review
            </Link>
          ),
          key: 'pi-plan-review',
        },
        {
          label: (
            <Link href={`/planning/planning-intervals/${key}/objectives`}>
              Objectives
            </Link>
          ),
          key: 'pi-objectives',
        },
        {
          label: (
            <Link href={`/planning/planning-intervals/${key}/risks`}>
              Risks
            </Link>
          ),
          key: 'pi-risks',
        },
        {
          label: 'Reports',
          key: 'pi-reports',
          children: [
            {
              label: (
                <Link
                  href={`/planning/planning-intervals/${key}/objectives/health-report`}
                >
                  Health Report
                </Link>
              ),
              key: 'pi-reports-health-report',
            },
          ],
        },
      ] as ItemType[],
    [planningIntervalData?.name, key],
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

export default PlanningIntervalLayout
