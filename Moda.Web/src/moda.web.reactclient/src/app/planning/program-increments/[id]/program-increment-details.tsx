import { ProgramIncrementDetailsDto } from "@/src/services/moda-api"
import { Col, Descriptions, Row } from "antd"
import { ReactMarkdown } from "react-markdown/lib/react-markdown"

const { Item } = Descriptions

const ProgramIncrementDetails = (programIncrement: ProgramIncrementDetailsDto) => {
    return (
        <>
            <Row>
                <Col xs={24} md={12}>
                    <Descriptions>
                        <Item label="Start">{new Date(programIncrement.start).toLocaleDateString()}</Item>
                        <Item label="End">{new Date(programIncrement.end).toLocaleDateString()}</Item>
                        <Item label="State">{programIncrement.state}</Item>
                    </Descriptions>
                </Col>
                <Col xs={24} md={12}>
                    <Descriptions>
                        <Item label="Description"><ReactMarkdown>{programIncrement.description}</ReactMarkdown></Item>
                    </Descriptions>
                </Col>
            </Row>
        </>
    )
}

export default ProgramIncrementDetails