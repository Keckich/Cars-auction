export { auth as middleware } from "@/auth"

export const config = {
    matcher: [
        '/session',
        "/((?!api|_next/static|_next/image|favicon.ico).*)",
    ],
    pages: {
        signIn: '/api/auth/signin'
    }
}