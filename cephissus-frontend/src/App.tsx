import { createMuiTheme, CssBaseline, ThemeProvider } from '@material-ui/core';
import { purple } from '@material-ui/core/colors';
import React, { useState, useEffect } from 'react';
import './App.css';
import { auth, AuthResponse } from "./auth";
import { Config } from './config/ConfigElement';

const theme = createMuiTheme({
    palette: {
        type: "dark",
        primary: purple,
    }
});

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
        <ThemeProvider theme={theme} >
            <CssBaseline />
            <Config />
        </ThemeProvider>
    );
}

export default App;
