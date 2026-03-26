import { Redirect } from "@docusaurus/router";
import useBaseUrl from "@docusaurus/useBaseUrl";
import { JSX } from "react";

export default function Home(): JSX.Element {
  return <Redirect to={useBaseUrl("/docs")} />;
}

