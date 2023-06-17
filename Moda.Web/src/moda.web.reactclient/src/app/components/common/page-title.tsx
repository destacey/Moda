import { Typography } from "antd";

const { Title, Text } = Typography;

export interface PageTitleProps {
  title: string;
  subtitle?: string;
}

const PageTitle = ({ title, subtitle }: PageTitleProps) => {
  return (
    <div style={{marginBottom:'12px'}}>
      <Title level={2} style={{marginTop:'8px', marginBottom:'0px'}}>{title}</Title>
      {subtitle && <Text style={{fontWeight: "bold"}}>{subtitle}</Text>}
    </div>
  )
}

export default PageTitle;