'use client'

import ModaGrid from "@/src/app/components/common/moda-grid";
import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementListDto } from "@/src/services/moda-api";
import { useEffect, useState } from "react";

const columnDefs = [
  { field: 'id', headerName: 'Full Id', hide: true },
  { field: 'localId', headerName: 'Id', width: 75 },
  { field: 'name' },
  { field: 'state', width: 125 },
  { field: 'start' },
  { field: 'end' }
]

const Page = () => {
  const [programIncrements, setProgramIncrements] = useState<ProgramIncrementListDto[]>([])

  useEffect(() => {
    const getProgramIncrements = async () => {
      const programIncrementClient = await getProgramIncrementsClient()
      const programIncrements = await programIncrementClient.getList()
      setProgramIncrements(programIncrements) // TODO: add sorting: by state: active, future, completed, then by start date
    }

    getProgramIncrements()
  }, [])

  return (
    <>
      <PageTitle title="Program Increments" />

      <ModaGrid columnDefs={columnDefs}
        rowData={programIncrements} />
    </>
  );
}

export default Page;