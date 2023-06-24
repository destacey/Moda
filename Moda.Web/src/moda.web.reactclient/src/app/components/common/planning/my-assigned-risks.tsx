import { useEffect, useState } from "react";
import { getRisksClient } from "@/src/services/clients";
import { Card, List } from "antd";
import Title from "antd/es/typography/Title";
import { RiskListDto } from "@/src/services/moda-api";
import Link from "next/link";

const MyAssignedRisks = () => {
    const [risks, setRisks] = useState<RiskListDto[]>([]);

    useEffect(() => {
        const getRisks = async () => {
            const risksClient = await getRisksClient();
            const risksDto = await risksClient.getMyRisks();
            setRisks(risksDto);
        };

        getRisks();
    }, []);

    const hasAssignedRisks = risks.length > 0;

//     var followUp = risk.FollowUpDate.HasValue
//     ? $" (follow-up: {risk.FollowUpDate?.ToString("d")})"
//     : string.Empty;
// return $"{risk.Summary}{followUp}";

    const riskMessage = (risk: RiskListDto) => {
        if (risk.followUpDate) {
            return `${risk.summary} (follow-up: ${risk.followUpDate})`;
        }
        return risk.summary;
    };

    function RenderContent() {
        if (hasAssignedRisks) {
            return (
                <>
                    <Card size="small" title="My Assigned Risks">
                        <List size="small">
                            {risks.map((r) => (
                                <List.Item key={r.localId}>
                                    <Link href={`/planning/risks/${r.localId}`}>
                                        {riskMessage(r)}
                                    </Link>
                                </List.Item>
                            ))}
                        </List>
                    </Card>
                </>
            );
        }
        return <></>;
    }

    return <RenderContent />;
};

export default MyAssignedRisks;
