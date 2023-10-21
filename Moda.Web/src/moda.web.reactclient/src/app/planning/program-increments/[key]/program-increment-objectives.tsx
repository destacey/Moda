import ProgramIncrementObjectivesGrid from '@/src/app/components/common/planning/program-increment-objectives-grid'
import { useGetProgramIncrementObjectives } from '@/src/services/queries/planning-queries'

interface ProgramIncrementObjectivesProps {
  programIncrementId: string
  objectivesQueryEnabled: boolean
  newObjectivesAllowed: boolean
}

const ProgramIncrementObjectives = ({
  programIncrementId,
  objectivesQueryEnabled,
  newObjectivesAllowed,
}: ProgramIncrementObjectivesProps) => {
  const objectivesQuery = useGetProgramIncrementObjectives(
    programIncrementId,
    objectivesQueryEnabled,
  )

  return (
    <ProgramIncrementObjectivesGrid
      objectivesQuery={objectivesQuery}
      programIncrementId={programIncrementId}
      hideProgramIncrementColumn={true}
      hideTeamColumn={true}
      newObjectivesAllowed={newObjectivesAllowed}
    />
  )
}

export default ProgramIncrementObjectives
