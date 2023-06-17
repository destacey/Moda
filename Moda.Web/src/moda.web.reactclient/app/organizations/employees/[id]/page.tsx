'use client'

import PageTitle from "@/app/components/common/page-title";
import { EmployeeDetailsDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = () => {
    const [employee, setEmployee] = useState<EmployeeDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={employee?.fullName ?? 'Test'} subtitle="Employee Details" />
        </>
    );
}

export default Page;