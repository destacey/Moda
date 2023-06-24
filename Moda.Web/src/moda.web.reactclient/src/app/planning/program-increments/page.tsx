"use client";

import ModaGrid from "@/src/app/components/common/moda-grid";
import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementListDto } from "@/src/services/moda-api";
import Link from "next/link";
import { useCallback, useMemo, useState } from "react";

const ProgramIncrementLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/planning/program-increments/${data.localId}`}>
            {value}
        </Link>
    );
};

const Page = () => {
    const [programIncrements, setProgramIncrements] = useState<
        ProgramIncrementListDto[]
    >([]);

    const columnDefs = useMemo(
        () => [
            { field: "localId", headerName: "#", width: 90 },
            { field: "name", cellRenderer: ProgramIncrementLinkCellRenderer },
            { field: "state", width: 125 },
            { field: "start" },
            { field: "end" },
        ],
        []
    );

    const getProgramIncrements = useCallback(async () => {
        const programIncrementClient = await getProgramIncrementsClient();
        const programIncrementDtos = await programIncrementClient.getList();
        setProgramIncrements(programIncrementDtos); // TODO: add sorting: by state: active, future, completed, then by start date
    }, []);

    return (
        <>
            <PageTitle title="Program Increments" />
            <ModaGrid
                columnDefs={columnDefs}
                rowData={programIncrements}
                loadData={getProgramIncrements}
            />
        </>
    );
};

export default Page;
