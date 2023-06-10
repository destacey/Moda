'use client'

import { Breadcrumb } from "antd";
import { Content } from "antd/es/layout/layout";

export default function Page() {
  return (
    <>
      {/* <Breadcrumb style={{ padding: '0 24px 24px' }} separator='>' style={{ margin: '16px 0' }} items={[{ title: 'Home', href: '/' }, { title: 'Teams' }]} /> */}
      <Content>
        <h1>Welcome to Teams</h1>
      </Content>
    </>
  );
}
