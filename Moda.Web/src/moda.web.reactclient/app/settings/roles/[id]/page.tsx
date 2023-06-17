'use client'

import PageTitle from "@/app/components/common/page-title";
import { RoleDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = () => {
    const [role, setRole] = useState<RoleDto | null>(null)

    return (
        <>
            <PageTitle title={role?.name ?? 'Test'} subtitle="Role Details" />
        </>
    );
}

export default Page;