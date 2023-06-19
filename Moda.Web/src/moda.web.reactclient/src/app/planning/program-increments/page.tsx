'use client'

import ModaGrid from "@/src/app/components/common/moda-grid";
import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementListDto } from "@/src/services/moda-api";
import { useEffect, useMemo, useState } from "react";

const Page = () => {
  const [programIncrements, setProgramIncrements] = useState<ProgramIncrementListDto[]>([])

  const columnDefs = useMemo(() => [
    { field: 'id', hide: true },
    { field: 'localId', headerName: '#', width: 75 },
    { field: 'name' },
    { field: 'state', width: 125 },
    { field: 'start' },
    { field: 'end' }
  ], []);

  useEffect(() => {
    const getProgramIncrements = async () => {
      const programIncrementClient = await getProgramIncrementsClient()
      const programIncrementDtos = await programIncrementClient.getList()
      setProgramIncrements(programIncrementDtos) // TODO: add sorting: by state: active, future, completed, then by start date
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