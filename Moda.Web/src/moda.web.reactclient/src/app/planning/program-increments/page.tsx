"use client";

import ModaGrid from "@/src/app/components/common/moda-grid";
import PageTitle from "@/src/app/components/common/page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { ProgramIncrementListDto } from "@/src/services/moda-api";
import Link from "next/link";
import { useCallback, useMemo, useState } from "react";
import { useDocumentTitle } from "../../hooks/use-document-title";

const ProgramIncrementLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/planning/program-increments/${data.localId}`}>
            {value}
        </Link>
    );
};

const ProgramIncrementListPage = () => {
    useDocumentTitle('Program Increments')
    const [programIncrements, setProgramIncrements] = useState<
        ProgramIncrementListDto[]
    >([]);

    // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
    const columnDefs = useMemo(
        () => [
            { field: "localId", headerName: "#", width: 90 },
            { field: "name", cellRenderer: ProgramIncrementLinkCellRenderer },
            { field: "state", width: 125 },
            { field: "start", valueGetter: (params) => new Date(params.data.start).toLocaleDateString() },
            { field: "end", valueGetter: (params) => new Date(params.data.end).toLocaleDateString() },
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

export default ProgramIncrementListPage;
