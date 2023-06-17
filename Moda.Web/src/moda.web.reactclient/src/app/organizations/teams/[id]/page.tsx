'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { TeamDetailsDto } from "@/src/services/moda-api";
import { useState } from "react";

const Page = ({ params }) => {
    const [team, setTeam] = useState<TeamDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={team?.name ?? `Test ${params.id}` } subtitle="Team Details" />
        </>
    );
}

export default Page;