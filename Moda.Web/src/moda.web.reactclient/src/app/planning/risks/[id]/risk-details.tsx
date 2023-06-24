import { RiskDetailsDto } from "@/src/services/moda-api";
import { Col, Descriptions, Row } from "antd";
import Link from "next/link";
import { ReactMarkdown } from "react-markdown/lib/react-markdown";

const { Item } = Descriptions;

const RiskDetails = (risk: RiskDetailsDto) => {
    const teamLink =
        risk.team?.type === "Team"
            ? `/organizations/teams/${risk.team?.localId}`
            : `/organizations/team-of-teams/${risk.team?.localId}`;

    const followUpDate = risk.followUpDate
        ? new Date(risk.followUpDate)?.toLocaleDateString()
        : null;
    return (
        <>
            <Row>
                <Col xs={24} md={10}>
                    <Descriptions layout="vertical">
                        <Item label="Description">
                            <ReactMarkdown>{risk.description}</ReactMarkdown>
                        </Item>
                    </Descriptions>
                    <Descriptions layout="vertical">
                        <Item label="Response">
                            <ReactMarkdown>{risk.response}</ReactMarkdown>
                        </Item>
                    </Descriptions>
                </Col>
                <Col xs={24} md={14}>
                    <Descriptions>
                        <Item label="Status">{risk.status?.name}</Item>
                        <Item label="Team">
                            <Link href={teamLink}>{risk.team?.name}</Link>
                        </Item>
                        <Item label="Category">{risk.category?.name}</Item>
                        <Item label="Follow-Up Date">{followUpDate}</Item>
                        <Item label="Assignee">
                            <Link
                                href={`/organizations/employees/${risk.assignee?.localId}`}
                            >
                                {risk.assignee?.name}
                            </Link>
                        </Item>
                        <Item label="exposure">{risk.exposure?.name}</Item>
                        <Item label="Impact">{risk.impact?.name}</Item>
                        <Item label="Likelihood">{risk.likelihood?.name}</Item>
                    </Descriptions>
                </Col>
            </Row>
            <Descriptions>
                <Item label="Reported By">
                    <Link
                        href={`/organizations/employees/${risk.reportedBy?.localId}`}
                    >
                        {risk.reportedBy?.name}
                    </Link>
                </Item>
                <Item label="Reported On">
                    {new Date(risk.reportedOn).toLocaleDateString()}
                </Item>
            </Descriptions>
        </>
    );
};

export default RiskDetails;
