"use client";

import { useEffect, useState } from "react";
import PageTitle from "./components/common/page-title";
import { getProgramIncrementsClient } from "../services/clients";
import ActiveProgramIncrements from "./components/common/planning/active-program-increments";

const Page = () => {
    return (
        <>
            <ActiveProgramIncrements />
        </>
    );
};

export default Page;
