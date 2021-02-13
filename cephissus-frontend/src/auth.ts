export interface User {
    id: string;
    displayName: string;
    profilePic: string;
    email: string
}

export type AuthResponse = {
    authenticated: true;
    user: User;
    redirectUrl: null;
} | {
    authenticated: false;
    redirectUrl: string;
    user: null;
};

export function auth(): Promise<AuthResponse> {
    return fetch("https://localhost:4242/auth", { method: "POST", credentials: "include" })
        .then(r => r.json() as Promise<AuthResponse>);
}
