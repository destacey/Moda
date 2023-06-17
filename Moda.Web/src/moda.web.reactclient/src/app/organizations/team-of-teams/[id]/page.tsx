'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { TeamOfTeamsDetailsDto } from "@/src/services/moda-api";
import { useState } from "react";

const Page = ({ params }) => {
    const [team, setTeam] = useState<TeamOfTeamsDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={team?.name ?? `Test ${params.id}` } subtitle="Team of Teams Details" />
        </>
    );
}

export default Page;