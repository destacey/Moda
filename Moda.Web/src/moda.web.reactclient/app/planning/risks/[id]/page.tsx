'use client'

import PageTitle from "@/app/components/common/page-title";
import { RiskDetailsDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = ({ params }) => {
    const [risk, setRisk] = useState<RiskDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={risk?.summary ?? `Test ${params.id}` } subtitle="Risk Details" />
        </>
    );
}

export default Page;