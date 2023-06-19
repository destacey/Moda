'use client'

import PageTitle from "@/src/app/components/common/page-title";
import ModaGrid from "../../components/common/moda-grid";
import { useEffect, useState } from "react";
import { BackgroundJobDto } from "@/src/services/moda-api";
import { getBackgroundJobsClient } from "@/src/services/clients";
import { withAuthorization } from "../../components/hoc";

// TODO: check permissions

const columnDefs = [
  { field: 'id' },
  { field: 'action' },
  { field: 'status' },
  { field: 'type' },
  { field: 'namespace' },
  { field: 'startedAt', headerName: 'Start (UTC)' }
]

const Page = () => {
  const [backgroundJobs, setBackgroundJobs] = useState<BackgroundJobDto[]>([])

  useEffect(() => {
    const getRunningJobs = async () => {
      const backgroundJobsClient = await getBackgroundJobsClient()
      const jobDtos = await backgroundJobsClient.getRunningJobs()
      setBackgroundJobs(jobDtos)
    }

    getRunningJobs()
  }, [])

  return (
    <>
      <PageTitle title="Background Jobs" />

      <ModaGrid columnDefs={columnDefs}
        rowData={backgroundJobs} />
    </>
  );
}

const PageWithAuthorization = withAuthorization(Page, "Permission", "Permissions.BackgroundJobs.View")

export default PageWithAuthorization;