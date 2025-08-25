import type { NextConfig } from "next";
import { hostname } from "os";
import withFlowbiteReact from "flowbite-react/plugin/nextjs";

const nextConfig: NextConfig = {
	logging: {
		fetches: {
			fullUrl: true,
		},
	},
	images: {
		remotePatterns: [{ protocol: "https", hostname: "cdn.pixabay.com" }],
	},
};

export default withFlowbiteReact(nextConfig);