'use client'

import PageTitle from "@/app/components/common/page-title";
import { EmployeeDetailsDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = ({ params }) => {
    const [employee, setEmployee] = useState<EmployeeDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={employee?.fullName ?? `Test ${params.id}` } subtitle="Employee Details" />
        </>
    );
}

export default Page;