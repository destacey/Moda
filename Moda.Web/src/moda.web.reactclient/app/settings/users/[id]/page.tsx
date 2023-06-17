'use client'

import PageTitle from "@/app/components/common/page-title";
import { UserDetailsDto } from "@/app/services/moda-api";
import { useState } from "react";

const Page = () => {
    const [user, setUser] = useState<UserDetailsDto | null>(null)

    return (
        <>
            <PageTitle title={user?.userName ?? 'Test'} subtitle="User Details" />
        </>
    );
}

export default Page;