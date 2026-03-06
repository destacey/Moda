'use client'

import { usePathname } from 'next/navigation'
import { authorizePage } from '../../../components/hoc'
import { useAppDispatch, useDocumentTitle } from '../../../hooks'
import { disableBreadcrumb } from '@/src/store/breadcrumbs'
import { useEffect, useMemo, useState } from 'react'
import { PageTitle } from '../../../components/common'
import { useGetFunctionalOrganizationChartQuery } from '@/src/store/features/organizations/team-api'
import {
  OrganizationalUnitDto,
  FunctionalOrganizationChartDto,
} from '@/src/services/moda-api'
import { DatePicker, Spin } from 'antd'
import {
  ModaOrganizationChart,
  OrganizationChartEdgeData,
  OrganizationChartGraphData,
  OrganizationChartNodeData,
} from '../../../components/common/organization-chart'
import { OrganizationalChartTeamNode } from '../_components'
import dayjs from 'dayjs'

export type OrganizationalUnitWithoutId = Omit<
  OrganizationalUnitDto,
  'id' | 'children'
>
export type OrganizationalUnitRecord = Record<
  string,
  OrganizationalUnitWithoutId
>

// Type alias for your specific use case
type TeamOrganizationGraphData = OrganizationChartGraphData<
  OrganizationalUnitWithoutId,
  Record<string, any>,
  Record<string, any>
>

function transformOrganizationToGraph(
  orgData: FunctionalOrganizationChartDto,
): TeamOrganizationGraphData {
  if (!orgData.organization) {
    return { nodes: [], edges: [] }
  }

  const nodes: OrganizationChartNodeData<OrganizationalUnitWithoutId>[] = []
  const edges: OrganizationChartEdgeData[] = []

  function processUnit(unit: OrganizationalUnitDto) {
    if (!unit.id) return

    const nodeData: OrganizationChartNodeData<OrganizationalUnitWithoutId> = {
      id: unit.id,
      data: {
        key: unit.key,
        name: unit.name,
        code: unit.code,
        type: unit.type,
        level: unit.level,
        path: unit.path,
      },
    }
    nodes.push(nodeData)

    if (unit.children && unit.children.length > 0) {
      unit.children.forEach((child) => {
        if (child.id) {
          const edge: OrganizationChartEdgeData = {
            id: `${unit.id}-${child.id}`,
            source: unit.id!,
            target: child.id,
          }
          edges.push(edge)
          processUnit(child)
        }
      })
    }
  }

  orgData.organization.forEach(processUnit)
  return { nodes, edges }
}

const FunctionalOrgChartPage: React.FC = () => {
  const [asOfDate, setAsOfDate] = useState<dayjs.Dayjs | null>(dayjs())

  useDocumentTitle('Functional Org Chart')

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const {
    data: orgChartData,
    isLoading,
    isError,
  } = useGetFunctionalOrganizationChartQuery(asOfDate?.toISOString(), {
    skip: !asOfDate,
  })

  const data = useMemo<TeamOrganizationGraphData>(() => {
    return orgChartData ? transformOrganizationToGraph(orgChartData) : undefined
  }, [orgChartData])

  useEffect(() => {
    dispatch(disableBreadcrumb(pathname))
  }, [dispatch, pathname])

  if (isError) {
    return (
      <>
        <br />
        <PageTitle title="Functional Org Chart" />
        <div>Error loading org chart. Please try again later.</div>
      </>
    )
  }

  const actions = () => {
    return (
      <>
        <DatePicker
          value={asOfDate}
          placeholder="As of date"
          onChange={setAsOfDate}
          title="The as of date for the functional org chart."
        />
      </>
    )
  }

  return (
    <>
      <br />
      <PageTitle title="Functional Org Chart" actions={actions()} />
      <Spin
        spinning={isLoading}
        description="Loading functional org chart..."
        size="large"
      >
        {!isLoading && (
          <ModaOrganizationChart
            data={data}
            NodeComponent={OrganizationalChartTeamNode}
          />
        )}
      </Spin>
    </>
  )
}

const FunctionalOrgChartPageWithAuthorization = authorizePage(
  FunctionalOrgChartPage,
  'Permission',
  'Permissions.Teams.View',
)

export default FunctionalOrgChartPageWithAuthorization
