export interface ISubredditConfig {
    id: string;
    subredditName: string;
    minScore: number;
    maxFetch: number;
    minAspectRatio: number;
    minHeight: number;
    allowNsfw: boolean;
}

export interface IManagedAlbum {
    id: string;
    albumId: string;
    albumName: string;
    refreshSchedule: Interval;
    searchPeriod: Interval;
    subredditConfigs: ISubredditConfig[];
}

export enum Interval {
    Daily = "Daily",
    Weekly = "Weekly",
    Monthly = "Monthly",
}

interface ConfigResponse {
    managedAlbums: IManagedAlbum[];
}

export function getManagedAlbums(): Promise<IManagedAlbum[]> {
    return fetch("https://localhost:4242/config/", { method: "GET", credentials: "include" })
        .then(r => r.json() as Promise<ConfigResponse>)
        .then(r => r.managedAlbums);
}