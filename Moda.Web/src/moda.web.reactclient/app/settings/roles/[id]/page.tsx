'use client'

import PageTitle from "@/app/components/common/page-title";
import { RoleDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = ({ params }) => {
    const [role, setRole] = useState<RoleDto | null>(null)

    return (
        <>
            <PageTitle title={role?.name ?? `Test ${params.id}` } subtitle="Role Details" />
        </>
    );
}

export default Page;