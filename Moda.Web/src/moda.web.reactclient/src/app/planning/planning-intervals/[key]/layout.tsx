'use client'

import { useGetPlanningIntervalByKey } from '@/src/services/queries/planning-queries'
import { Menu, MenuProps } from 'antd'
import Link from 'next/link'
import { useMemo } from 'react'

const PlanninIntervalLayout = ({
  params,
  children,
}: {
  params: any
  children: React.ReactNode
}) => {
  const { data: planningIntervalData } = useGetPlanningIntervalByKey(params.key)

  const items: MenuProps['items'] = useMemo(
    () => [
      {
        label: `Planning Interval: ${planningIntervalData?.name}`,
        key: 'pi-name',
        type: 'text',
        disabled: true,
      },
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
      {
        label: (
          <Link href={`/planning/planning-intervals/${params.key}/objectives`}>
            Objectives
          </Link>
        ),
        key: 'pi-objectives',
      },
      {
        label: (
          <Link href={`/planning/planning-intervals/${params.key}/risks`}>
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
    [planningIntervalData?.name, params.key],
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
