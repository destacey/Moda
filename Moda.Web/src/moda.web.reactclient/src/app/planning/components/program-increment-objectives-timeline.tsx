import vis from 'vis-timeline/dist/vis-timeline-graph2d.esm'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
} from '@/src/services/moda-api'
import dayjs from 'dayjs'
import { useCallback, useEffect, useState } from 'react'
import { ModaEmpty } from '../../components/common'

interface ProgramIncrementObjectivesTimelineProps {
  getObjectives: (
    programIncrementId: string,
    teamId?: string
  ) => Promise<ProgramIncrementObjectiveListDto[]>
  programIncrement: ProgramIncrementDetailsDto
  teamId: string
}

interface TimelineItem {
  id: number
  programIncrementLocalId: number
  content: string
  start: Date
  end: Date
  group?: string | undefined
  type?: string | undefined
}

// TODO: add a link within the content to the objective details page
const ItemTemplate = (item) => {
  return <>{item.content}</>
}

const ProgramIncrementObjectivesTimeline = ({
  getObjectives,
  programIncrement,
  teamId,
}: ProgramIncrementObjectivesTimelineProps) => {
  const [objectives, setObjectives] = useState<TimelineItem[]>([])

  // TODO: add the ability to export/save as svg or png
  // TODO: update the styles to match the rest of the app.  Especially for dark mode.
  const options = useCallback((start: Date, end: Date, template) => {
    return {
      orientation: 'top',
      showCurrentTime: true,
      minHeight: 200,
      maxHeight: 700,
      verticalScroll: true,
      zoomKey: 'ctrlKey',
      start: dayjs(start).subtract(1, 'week').toDate(),
      end: dayjs(end).add(1, 'week').toDate(),
      editable: false,
      teamplate: template,
    }
  }, [])

  const loadObjectives = useCallback(
    async (programIncrementId: string, teamId: string) => {
      const objectiveDtos = await getObjectives(programIncrementId, teamId)
      setObjectives(
        objectiveDtos
          .filter((obj) => obj.status?.name !== 'Canceled')
          .map((obj, index) => {
            return {
              id: obj.localId,
              programIncrementLocalId: obj.programIncrement.localId,
              content: obj.name,
              start: dayjs(obj.startDate ?? programIncrement.start).toDate(),
              end: dayjs(obj.targetDate ?? programIncrement.end).toDate(),
              group: obj.team?.name,
            }
          })
      )
    },
    [getObjectives, programIncrement.end, programIncrement.start]
  )

  useEffect(() => {
    loadObjectives(programIncrement?.id, teamId)
  }, [loadObjectives, programIncrement, teamId])

  useEffect(() => {
    if (!objectives || objectives.length === 0) {
      return
    }
    // TODO: add the ability for content to overflow if the text is too long
    const items = [
      {
        id: -1,
        content: '',
        start: dayjs(programIncrement.start).toDate(),
        end: dayjs(programIncrement.start).add(1, 'day').toDate(),
        type: 'background',
      },
      {
        id: -2,
        content: '',
        start: dayjs(programIncrement.end).toDate(),
        end: dayjs(programIncrement.end).add(1, 'day').toDate(),
        type: 'background',
      },
      ...objectives,
    ]

    var container = document.getElementById('timeline-vis')
    const timeline = new vis.Timeline(
      container,
      items,
      options(programIncrement.start, programIncrement.end, ItemTemplate)
    )
  }, [objectives, options, programIncrement.end, programIncrement.start])

  const TimelineChart = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
    }

    // TODO: add a loading indicator
    return (
      <>
        <div id="timeline-vis"></div>
      </>
    )
  }

  return <TimelineChart />
}

export default ProgramIncrementObjectivesTimeline
