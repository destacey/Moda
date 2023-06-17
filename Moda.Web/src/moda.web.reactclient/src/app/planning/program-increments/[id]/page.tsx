'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { ProgramIncrementDetailsDto } from "@/src/services/moda-api";
import { useState } from "react";

const Page = ({ params }) => {
    const [programIncrement, setProgramIncrement] = useState<ProgramIncrementDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={programIncrement?.name ?? `Test ${params.id}`} subtitle="Program Increment Details" />
        </>
    );
}

export default Page;