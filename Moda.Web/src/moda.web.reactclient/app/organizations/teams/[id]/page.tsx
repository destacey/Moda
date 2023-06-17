'use client'

import PageTitle from "@/app/components/common/page-title";
import { TeamDetailsDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = () => {
    const [team, setTeam] = useState<TeamDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={team?.name ?? 'Test'} subtitle="Team Details" />
        </>
    );
}

export default Page;