import React, { useEffect, useState } from "react";
import { AppBar, Box, Card, CardContent, Checkbox, Container, FormControl, FormControlLabel, Input, InputLabel, makeStyles, MenuItem, Select, TextField, Toolbar, Typography } from "@material-ui/core";
import { getManagedAlbums, IManagedAlbum, Interval, ISubredditConfig } from "./config";

const useStyles = makeStyles(theme => ({
    formControl: {
        margin: theme.spacing(1),
        minWidth: "150px",
    },
    card: {
        marginBottom: "15px"
    },
}));

export function Config(): JSX.Element {
    const [managedAlbums, setManagedAlbums] = useState<IManagedAlbum[]>();

    useEffect(() => {
        getManagedAlbums().then(setManagedAlbums);
    }, []);

    return (
        <>
            <AppBar>
                <Toolbar>
                    <Typography variant="h6">Managed Albums</Typography>
                </Toolbar>
            </AppBar>
            <Toolbar />
            <Container maxWidth="sm">
                <Box marginTop={2}>
                    {managedAlbums?.map(album =>
                        <ManagedAlbumElement key={album.albumId} managedAlbum={album} />
                    )}
                </Box>
            </Container>
        </>
    );
}

function ManagedAlbumElement({ managedAlbum }: { managedAlbum: IManagedAlbum }): JSX.Element {
    const [albumName, setAlbumName] = useState(managedAlbum.albumName);
    const [refreshSchedule, setRefreshSchedule] = useState(managedAlbum.refreshSchedule);
    const [searchPeriod, setSearchPeriod] = useState(managedAlbum.searchPeriod);
    const [subredditConfigs, setSubredditConfigs] = useState(managedAlbum.subredditConfigs);

    const styles = useStyles();

    const refreshScheduleId = `refreshSchedule-${managedAlbum.id}`;
    const searchPeriodId = `searchPeriod-${managedAlbum.id}`;

    return (
        <Card className={styles.card}>
            <CardContent>
                <TextField className={styles.formControl} id="albumName" label="Name" value={albumName} onChange={e => setAlbumName(e.target.value)}></TextField>
                <br />
                <FormControl className={styles.formControl}>
                    <InputLabel htmlFor={refreshScheduleId}>Refresh Schedule</InputLabel>
                    <Select id={refreshScheduleId} value={refreshSchedule} onChange={e => setRefreshSchedule(e.target.value as Interval)}>
                        <MenuItem value={Interval.Daily}>{Interval.Daily}</MenuItem>
                        <MenuItem value={Interval.Weekly}>{Interval.Weekly}</MenuItem>
                        <MenuItem value={Interval.Monthly}>{Interval.Monthly}</MenuItem>
                    </Select>
                </FormControl>
                <br />
                <FormControl className={styles.formControl}>
                    <InputLabel htmlFor={searchPeriodId}>Search Period</InputLabel>
                    <Select id={searchPeriodId} value={searchPeriod} onChange={e => setSearchPeriod(e.target.value as Interval)}>
                        <MenuItem value={Interval.Daily}>{Interval.Daily}</MenuItem>
                        <MenuItem value={Interval.Weekly}>{Interval.Weekly}</MenuItem>
                        <MenuItem value={Interval.Monthly}>{Interval.Monthly}</MenuItem>
                    </Select>
                </FormControl>
                <br />
                {subredditConfigs?.map(subredditConfig =>
                    <SubredditConfig subredditConfig={subredditConfig} key={subredditConfig.id} />
                )}
            </CardContent>
        </Card>
    )
}

function SubredditConfig({ subredditConfig }: { subredditConfig: ISubredditConfig }): JSX.Element {
    const [subredditName, setSubredditName] = useState(subredditConfig.subredditName);
    const [minScore, setMinScore] = useState(subredditConfig.minScore);
    const [maxFetch, setMaxFetch] = useState(subredditConfig.maxFetch);
    const [minHeight, setMinHeight] = useState(subredditConfig.minHeight);
    const [minAspectRatio, setMinAspectRatio] = useState(subredditConfig.minAspectRatio);
    const [allowNsfw, setAllowNsfw] = useState(subredditConfig.allowNsfw);
    const [id, setId] = useState(subredditConfig.id);

    const styles = useStyles();

    // const minScoreId = `minScore-${subredditConfig.id}`;
    // const maxFetchId = `maxFetch-${subredditConfig.id}`;
    // const minHeightId = `minHeight-${subredditConfig.id}`;
    // const minAspectRatioId = `minAspectRatio-${subredditConfig.id}`;
    const allowNsfwId = `allowNsfw-${subredditConfig.id}`;

    return (
        <Card className={styles.card} elevation={4}>
            <CardContent>
                <TextField className={styles.formControl} label="Subreddit" value={subredditName} onChange={e => setSubredditName(e.target.value)}></TextField>
                <br />
                <TextField className={styles.formControl} label="Min Score" type="number" value={minScore} onChange={e => setMinScore(Number(e.target.value))} />
                <br />
                <TextField className={styles.formControl} label="Max Fetch" type="number" value={maxFetch} onChange={e => setMaxFetch(Number(e.target.value))} />
                <br />
                <TextField className={styles.formControl} label="Min Height" type="number" value={minHeight} onChange={e => setMinHeight(Number(e.target.value))} />
                <br />
                <TextField className={styles.formControl} label="Min Aspect Ratio" type="number" value={minAspectRatio} onChange={e => setMinAspectRatio(Number(e.target.value))} />
                <br />
                {/* <FormControl className={styles.formControl}> */}
                    {/* <InputLabel htmlFor={allowNsfwId}>Allow NSFW</InputLabel> */}
                    <FormControlLabel className={styles.formControl} label="Allow NSFW" control={
                        <Checkbox className={styles.formControl} id={allowNsfwId} value={allowNsfw} onChange={e => setAllowNsfw(e.target.checked)} />
                    } />
                {/* </FormControl> */}

            </CardContent>
        </Card>
    );
}