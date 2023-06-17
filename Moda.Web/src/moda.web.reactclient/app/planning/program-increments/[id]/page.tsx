'use client'

import PageTitle from "@/app/components/common/page-title";
import { ProgramIncrementDetailsDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = () => {
    const [programIncrement, setProgramIncrement] = useState<ProgramIncrementDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={programIncrement?.name ?? 'Test'} subtitle="Program Increment Details" />
        </>
    );
}

export default Page;