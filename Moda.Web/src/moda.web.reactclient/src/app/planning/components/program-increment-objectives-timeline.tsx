import * as vis from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
} from '@/src/services/moda-api'
import dayjs from 'dayjs'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ModaEmpty } from '../../components/common'

interface ProgramIncrementObjectivesTimelineProps {
  getObjectives: (
    programIncrementId: string,
    teamId?: string
  ) => Promise<ProgramIncrementObjectiveListDto[]>
  programIncrement: ProgramIncrementDetailsDto
  teamId: string
}

interface TimelineItem extends vis.DataItem {
  id: number
  programIncrementLocalId: number
  title?: string | undefined
  content: string
  start: Date
  end: Date
  group?: string | undefined
  type?: string | undefined
}

// TODO: get this working with the template function
// const TimelineContentTemplate = (objective: ProgramIncrementObjectiveListDto) => {
//   const content = `Status: ${objective.status?.name} | Stretch?: ${objective.isStretch}`
//   const startDate = objective.startDate
//     ? ` | Start: ${
//         objective.startDate
//           ? dayjs(objective.startDate)?.format('M/D/YYYY')
//           : ''
//       }`
//     : null
//   const targetDate = objective.targetDate
//     ? ` | Target: ${
//         objective.targetDate
//           ? dayjs(objective.targetDate)?.format('M/D/YYYY')
//           : ''
//       }`
//     : null
//   const showProgress = objective.status?.name !== 'Not Started'
//   const progressStatus =
//     objective.status?.name === 'Canceled' ? 'exception' : undefined
//   return (
//     <>
//       <Typography.Text>
//         {content}
//         {startDate}
//         {targetDate}
//       </Typography.Text>
//       {showProgress && (
//         <Progress
//           percent={objective.progress}
//           status={progressStatus}
//           size="small"
//         />
//       )}
//     </>
//   )
// }

const ProgramIncrementObjectivesTimeline = ({
  getObjectives,
  programIncrement,
  teamId,
}: ProgramIncrementObjectivesTimelineProps) => {
  const [objectives, setObjectives] = useState<TimelineItem[]>([])

  // TODO: add the ability to export/save as svg or png
  // TODO: update the styles to match the rest of the app.  Especially for dark mode.
  const options: vis.TimelineOptions = useMemo(() => {
    return {
      editable: false,
      orientation: 'top',
      maxHeight: 700,
      minHeight: 200,
      moveable: true,
      showCurrentTime: true,
      verticalScroll: true,
      zoomKey: 'ctrlKey',
      start: dayjs(programIncrement.start).subtract(1, 'week').toDate(),
      end: dayjs(programIncrement.end).add(1, 'week').toDate(),
    }
  }, [programIncrement])

  const loadObjectives = useCallback(
    async (programIncrement: ProgramIncrementDetailsDto, teamId: string) => {
      const objectiveDtos = await getObjectives(programIncrement.id, teamId)
      setObjectives(
        objectiveDtos
          .filter((obj) => obj.status?.name !== 'Canceled')
          .map((obj, index) => {
            return {
              id: obj.localId,
              programIncrementLocalId: obj.programIncrement.localId,
              title: `${obj.name} (${obj.status?.name}) - ${obj.progress}%`,
              content: `${obj.name} (${obj.status?.name})`,
              start: dayjs(obj.startDate ?? programIncrement.start).toDate(),
              end: dayjs(obj.targetDate ?? programIncrement.end).toDate(),
              group: obj.team?.name,
            }
          })
      )
    },
    [getObjectives]
  )

  useEffect(() => {
    loadObjectives(programIncrement, teamId)
  }, [loadObjectives, programIncrement, teamId])

  useEffect(() => {
    if (!objectives || objectives.length === 0) {
      return
    }
    // TODO: add the ability for content to overflow if the text is too long
    const items: TimelineItem[] = [
      {
        id: -1,
        programIncrementLocalId: -1,
        title: 'PI Start',
        content: '',
        start: dayjs(programIncrement.start).toDate(),
        end: dayjs(programIncrement.start).add(1, 'day').toDate(),
        type: 'background',
      },
      {
        id: -2,
        programIncrementLocalId: -2,
        title: 'PI End',
        content: '',
        start: dayjs(programIncrement.end).toDate(),
        end: dayjs(programIncrement.end).add(1, 'day').toDate(),
        type: 'background',
      },
      ...objectives,
    ]

    var container = document.getElementById('timeline-vis')
    const timeline = new vis.Timeline(container, items, options)
  }, [objectives, options, programIncrement.end, programIncrement.start])

  const TimelineChart = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
    }

    return (
      <>
        <div id="timeline-vis"></div>
      </>
    )
  }

  // TODO: add a loading indicator
  return <TimelineChart />
}

export default ProgramIncrementObjectivesTimeline
