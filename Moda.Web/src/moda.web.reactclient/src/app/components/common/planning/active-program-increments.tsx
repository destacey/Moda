import { useEffect, useState } from "react";
import PageTitle from "../page-title";
import { getProgramIncrementsClient } from "@/src/services/clients";
import { Col, Row } from "antd";
import ProgramIncrementCard from "./program-increment-card";

const ActiveProgramIncrements = () => {
    const [activeProgramIncrements, setActiveProgramIncrements] = useState([]);

    useEffect(() => {
        const loadActiveProgramIncrements = async () => {
            const programIncrementClient = await getProgramIncrementsClient();
            const programIncrementDtos = await programIncrementClient.getList();
            const activeProgramIncrements = programIncrementDtos.filter(
                (pi) => pi.state === "Active"
            );
            setActiveProgramIncrements(activeProgramIncrements);
        };

        loadActiveProgramIncrements();
    }, []);

    const hasActiveProgramIncrements = activeProgramIncrements.length > 0;

    function RenderContent() {
        if (hasActiveProgramIncrements) {
            let i = 0;
            return (
                <>
                    <PageTitle title="Active Program Increments" />
                    <Row>
                            {activeProgramIncrements.map((pi) => (
                                <Col key={i++} xs={24} sm={12} md={6} lg={4} >
                                    <ProgramIncrementCard
                                        programIncrement={pi}
                                    />
                                </Col>
                            ))}
                    </Row>
                </>
            );
        }
        return <></>;
    }

    return <RenderContent />;
};

export default ActiveProgramIncrements;
