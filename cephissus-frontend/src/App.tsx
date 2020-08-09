import React, { useState, useEffect } from 'react';
import './App.css';
import { auth, AuthResponse } from "./auth";

function App(): JSX.Element {
    const [authResponse, setAuthResponse] = useState<AuthResponse>();

    useEffect(() => {
        auth().then(setAuthResponse);
    }, []);

    if (!authResponse) {
        return <div>Loading...</div>
    }

    if (!authResponse.authenticated) {
        return (
            <div>
                <p>You are logged out</p>
                <a href={authResponse.redirectUrl}>Sign in with Google</a>
            </div>
        );
    }

    const user = authResponse.user;

    return (
        <div>
            <p>Hello, {user.displayName}</p>
        </div>
    );
}

export default App;
