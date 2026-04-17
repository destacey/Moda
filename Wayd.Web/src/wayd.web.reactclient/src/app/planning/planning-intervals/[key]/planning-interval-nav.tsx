'use client'

import { useGetPlanningIntervalQuery } from '@/src/store/features/planning/planning-interval-api'
import { Menu } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import Link from 'next/link'

const PlanningIntervalNav = ({ piKey }: { piKey: number }) => {
  const { data: planningIntervalData } = useGetPlanningIntervalQuery(piKey)

  const items: ItemType[] = [
    {
      label: `Planning Interval: ${planningIntervalData?.name}`,
      key: 'pi-name',
      disabled: true,
    },
    {
      label: (
        <Link href={`/planning/planning-intervals/${piKey}`}>
          PI Details
        </Link>
      ),
      key: 'pi-details',
    },
    {
      label: (
        <Link href={`/planning/planning-intervals/${piKey}/plan-review`}>
          Plan Review
        </Link>
      ),
      key: 'pi-plan-review',
    },
    {
      label: (
        <Link href={`/planning/planning-intervals/${piKey}/objectives`}>
          Objectives
        </Link>
      ),
      key: 'pi-objectives',
    },
    {
      label: (
        <Link href={`/planning/planning-intervals/${piKey}/risks`}>
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
              href={`/planning/planning-intervals/${piKey}/objectives/health-report`}
            >
              Health Report
            </Link>
          ),
          key: 'pi-reports-health-report',
        },
      ],
    },
  ]

  return (
    <Menu
      selectable={false}
      style={{
        marginBottom: 12,
        borderColor: 'transparent',
      }}
      mode="horizontal"
      items={items}
    />
  )
}

export default PlanningIntervalNav
