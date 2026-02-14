'use client'

import { FC, useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  DatePicker,
  Flex,
  InputNumber,
  Select,
  Space,
  Tooltip,
  Typography,
} from 'antd'
import dayjs, { Dayjs } from 'dayjs'
import utc from 'dayjs/plugin/utc'
import { useGetTeamWorkItemsQuery } from '@/src/store/features/organizations/team-api'
import { WorkStatusCategory, WorkItemListDto } from '@/src/services/moda-api'
import { useDebounce } from '@/src/hooks'
import { CycleTimeAnalysisChart, WorkItemsGrid } from '.'
import {
  applyBalancedPercentileFilter,
  CycleTimeOutlierMethod,
  applyForecastingPercentileFilter,
  getCycleTimeWorkItems,
  normalizePercentile,
  sortCycleTimeWorkItems,
} from './cycle-time-report.filtering'
import { InfoCircleOutlined } from '@ant-design/icons'
import { AgGridReact } from 'ag-grid-react'

const { Title, Text } = Typography

dayjs.extend(utc)

export interface CycleTimeReportProps {
  teamCode: string
}

export const CycleTimeReport: FC<CycleTimeReportProps> = ({ teamCode }) => {
  const gridRef = useRef<AgGridReact<WorkItemListDto>>(null)
  const filterAnimationFrameRef = useRef<number | null>(null)
  const defaultDoneFrom = useMemo(() => {
    return dayjs().utc().subtract(90, 'days').startOf('day')
  }, [])
  const doneFromPresets = useMemo(() => {
    return [30, 60, 90, 120, 180].map((days) => ({
      label: `${days} Days`,
      value: dayjs().utc().subtract(days, 'days').startOf('day'),
    }))
  }, [])

  const [selectedDate, setSelectedDate] = useState<Dayjs>(defaultDoneFrom)
  const [chartWorkItems, setChartWorkItems] = useState<WorkItemListDto[]>([])
  const [percentileInputValue, setPercentileInputValue] = useState<number>(100)
  const [method, setMethod] = useState<CycleTimeOutlierMethod>('Balanced')
  const percentile = useDebounce(percentileInputValue, 500)

  const doneFrom = useMemo(() => {
    return selectedDate.toISOString()
  }, [selectedDate])

  const {
    data: workItemsData,
    isLoading,
    error,
    refetch,
  } = useGetTeamWorkItemsQuery({
    idOrCode: teamCode,
    statusCategories: [WorkStatusCategory.Done],
    doneFrom,
    doneTo: null,
  })

  const cycleTimeItems = useMemo(() => {
    return getCycleTimeWorkItems(workItemsData)
  }, [workItemsData])

  const normalizedPercentile = useMemo(() => {
    return normalizePercentile(percentile)
  }, [percentile])

  const sortedCycleTimeItems = useMemo(() => {
    if (normalizedPercentile === 1 || normalizedPercentile === 0) {
      return cycleTimeItems
    }
    return sortCycleTimeWorkItems(cycleTimeItems)
  }, [cycleTimeItems, normalizedPercentile])

  const filteredWorkItems = useMemo(() => {
    if (cycleTimeItems.length === 0) {
      return []
    }

    if (normalizedPercentile === 1) {
      return cycleTimeItems
    }
    if (normalizedPercentile === 0) {
      return []
    }

    if (method === 'Balanced') {
      return applyBalancedPercentileFilter(
        sortedCycleTimeItems,
        normalizedPercentile,
      )
    }

    return applyForecastingPercentileFilter(
      sortedCycleTimeItems,
      normalizedPercentile,
    )
  }, [
    cycleTimeItems,
    method,
    normalizedPercentile,
    sortedCycleTimeItems,
  ])

  const onFilterChanged = useCallback(() => {
    if (filterAnimationFrameRef.current !== null) {
      return
    }
    filterAnimationFrameRef.current = window.requestAnimationFrame(() => {
      filterAnimationFrameRef.current = null

      if (gridRef.current?.api) {
        const displayedRows: WorkItemListDto[] = []
        gridRef.current.api.forEachNodeAfterFilterAndSort((node) => {
          if (node.data) {
            displayedRows.push(node.data)
          }
        })
        setChartWorkItems(displayedRows)
      }
    })
  }, [])

  // Initialize chart data when filtered work items change
  useEffect(() => {
    setChartWorkItems(filteredWorkItems)
  }, [filteredWorkItems])

  useEffect(() => {
    return () => {
      if (filterAnimationFrameRef.current !== null) {
        window.cancelAnimationFrame(filterAnimationFrameRef.current)
      }
    }
  }, [])

  if (error) {
    return <div>Error loading work items</div>
  }

  return (
    <Flex vertical>
      <Flex justify="space-between" align="start" wrap>
        <Space>
          <Title level={4} style={{ marginTop: 0 }}>
            Cycle Time Report
          </Title>
          <Tooltip
            title={
              <>
                Shows cycle time analysis for team work items completed since
                the selected date. Work items that were never activated and that
                went straight to Done are excluded.
                <br />
                <br />
                The chart updates dynamically based on grid filters.
              </>
            }
          >
            <InfoCircleOutlined />
          </Tooltip>
        </Space>
        <Flex gap="16px" wrap>
          <Space>
            <Tooltip title="Date represents the beginning of the day in UTC">
              <Text>Done From:</Text>
            </Tooltip>
            <DatePicker
              value={selectedDate}
              onChange={(date) =>
                date && setSelectedDate(date.utc().startOf('day'))
              }
              presets={doneFromPresets}
              format="YYYY-MM-DD"
              allowClear={false}
            />
          </Space>
          <Space>
            <Tooltip title="Percentage of work items included in the calculation after outliers are removed.">
              <Text>Percentile:</Text>
            </Tooltip>
            <InputNumber
              min={0}
              max={100}
              value={percentileInputValue}
              onChange={(value) => setPercentileInputValue(value ?? 100)}
              style={{ width: 90 }}
              suffix="%"
            />
          </Space>
          <Space>
            <Tooltip
              title={
                <>
                  Determines how outliers are identified and removed from cycle
                  time calculations.
                  <br />
                  <br />
                  Balanced: Provides a statistically balanced view of cycle time
                  and prevents extreme values from skewing averages.
                  <br />
                  <br />
                  Forecasting: Removes the slowest outliers from the dataset.
                </>
              }
            >
              <Text>Method:</Text>
            </Tooltip>
            <Select<CycleTimeOutlierMethod>
              value={method}
              onChange={setMethod}
              options={[
                { value: 'Balanced', label: 'Balanced' },
                { value: 'Forecasting', label: 'Forecasting' },
              ]}
              style={{ width: 120 }}
            />
          </Space>
        </Flex>
      </Flex>
      {workItemsData !== undefined && (
        <CycleTimeAnalysisChart
          workItems={chartWorkItems}
          isLoading={isLoading}
        />
      )}
      <WorkItemsGrid
        ref={gridRef}
        workItems={filteredWorkItems}
        isLoading={isLoading}
        refetch={refetch}
        showStats={true}
        onFilterChanged={onFilterChanged}
      />
    </Flex>
  )
}
