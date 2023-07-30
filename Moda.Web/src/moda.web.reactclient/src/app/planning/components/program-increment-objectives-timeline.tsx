import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
} from '@/src/services/moda-api'
import { ApexOptions } from 'apexcharts'
import dayjs from 'dayjs'
import { use, useCallback, useEffect, useState } from 'react'
import ReactApexChart from 'react-apexcharts'
import { ModaEmpty } from '../../components/common'

interface ProgramIncrementObjectivesTimelineProps {
  getObjectives: (
    programIncrementId: string,
    teamId?: string
  ) => Promise<ProgramIncrementObjectiveListDto[]>
  programIncrement: ProgramIncrementDetailsDto
  teamId: string
}

const ProgramIncrementObjectivesTimeline = ({
  getObjectives,
  programIncrement,
  teamId,
}: ProgramIncrementObjectivesTimelineProps) => {
  const [objectives, setObjectives] = useState<
    ProgramIncrementObjectiveListDto[]
  >([])
  const [chartSeries, setChartSeries] = useState<ApexAxisChartSeries>([])
  const [chartHeight, setChartHeight] = useState<number>(500)

  const chartOptions: ApexOptions = {
    chart: {
      toolbar: {
        autoSelected: 'pan',
      },
    },
    plotOptions: {
      bar: {
        horizontal: true,
        distributed: true,
        dataLabels: {
          hideOverflowingLabels: false,
        },
      },
    },
    dataLabels: {
      enabled: true,
      formatter: function (val, opts) {
        var label = opts.w.globals.labels[opts.dataPointIndex]
        return label
      },
      style: {
        colors: ['#f5f5f5', '#fff'],
      },
    },
    xaxis: {
      type: 'datetime',
      min: programIncrement
        ? dayjs(programIncrement.start).valueOf()
        : undefined,
      max: programIncrement ? dayjs(programIncrement.end).valueOf() : undefined,
    },
    yaxis: {
      show: false,
    },
    grid: {
      row: {
        colors: ['#f5f5f5', '#fff'],
        opacity: 1,
      },
    },
  }

  const loadObjectives = useCallback(
    async (programIncrementId: string, teamId: string) => {
      const objectiveDtos = await getObjectives(programIncrementId, teamId)
      const rowHeight = objectiveDtos.length <= 10 ? 75 : 50
      setChartHeight(objectiveDtos.length * rowHeight + 100)
      setObjectives(
        objectiveDtos.filter((obj) => obj.status?.name !== 'Canceled')
      )
    },
    [getObjectives]
  )

  useEffect(() => {
    loadObjectives(programIncrement?.id, teamId)
  }, [loadObjectives, programIncrement, teamId])

  useEffect(() => {
    if (!objectives || objectives.length === 0) {
      setChartSeries([])
      return
    }

    const seriesData = objectives.map((objective) => {
      return {
        x: objective.name,
        y: [
          dayjs(objective.startDate ?? programIncrement.start).valueOf(),
          dayjs(objective.targetDate ?? programIncrement.end).valueOf(),
        ],
      }
    })
    setChartSeries([{ data: seriesData }])
  }, [objectives, programIncrement.end, programIncrement.start])

  const TimelineChart = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
    }

    return (
      <div id="chart">
        <ReactApexChart
          options={chartOptions}
          series={chartSeries}
          type="rangeBar"
          height={chartHeight}
        />
      </div>
    )
  }

  return <TimelineChart />
}

export default ProgramIncrementObjectivesTimeline
