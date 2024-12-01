'use client'

import { usePathname } from 'next/navigation'
import { authorizePage } from '../../components/hoc'
import { useAppDispatch, useDocumentTitle } from '../../hooks'
import { disableBreadcrumb } from '@/src/store/breadcrumbs'
import { useEffect, useState } from 'react'
import { PageTitle } from '../../components/common'
import { useGetFunctionalOrganizationChartQuery } from '@/src/store/features/organizations/team-api'
import {
  OrganizationalUnitDto,
  FunctionalOrganizationChartDto,
} from '@/src/services/moda-api'
import { Spin } from 'antd'
import {
  ModaOrganizationChart,
  OrganizationChartEdgeData,
  OrganizationChartGraphData,
  OrganizationChartNodeData,
} from '../../components/common/organization-chart'
import { OrganizationalChartTeamNode } from '../components'

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
  const [data, setData] = useState<TeamOrganizationGraphData>()

  useDocumentTitle('Functional Org Chart')

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const {
    data: orgChartData,
    isLoading,
    isError,
  } = useGetFunctionalOrganizationChartQuery(null)

  useEffect(() => {
    dispatch(disableBreadcrumb(pathname))
  }, [dispatch, pathname])

  useEffect(() => {
    if (orgChartData) {
      const graphData = transformOrganizationToGraph(orgChartData)
      setData(graphData)
    }
  }, [orgChartData])

  if (isError) {
    return (
      <>
        <br />
        <PageTitle title="Functional Org Chart" />
        <div>Error loading org chart. Please try again later.</div>
      </>
    )
  }

  return (
    <>
      <br />
      <PageTitle title="Functional Org Chart" />
      <Spin
        spinning={isLoading}
        tip="Loading functional org chart..."
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
