import React, { useState, useEffect } from 'react';
import './App.css';
import { UserManager, UserManagerSettings, User } from "oidc-client";

const config: UserManagerSettings = {
    authority: "https://accounts.google.com",
    client_id: "458544218315-uj4rolp7u08fctqjsgs9ghsgglu3vlg5.apps.googleusercontent.com",
    response_type: "code",
    response_mode: "fragment",
    scope: "https://www.googleapis.com/auth/photoslibrary",
    redirect_uri: "https://localhost/temp",
};

const userManager = new UserManager(config);

function logIn(): void {
    userManager.signinRedirect();
}

function App(): JSX.Element {
    const [loggedIn, setLoggedIn] = useState<boolean | null>(null);

    useEffect(() => {
        userManager.getUser().then(u => {
            console.log("getUser callback", { user: u });
            setLoggedIn(!!u);
        });
    }, []);
    return (
        <div>
            {loggedIn == null && <span>loading...</span>}
            {loggedIn === true && <span>logged in</span>}
            {loggedIn === false &&
                <div>
                    <h2>you logged out, son</h2>
                    <button onClick={() => logIn()}>log in with google</button>
                </div>
            }
        </div>
    );
}

export default App;
