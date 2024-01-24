"use strict";

// build connection to server
const connectionMeasurementHub = new signalR.HubConnectionBuilder().withUrl("/measurementHub").build();
const connectionPlaybackHub = new signalR.HubConnectionBuilder().withUrl("/playbackHub").build();

// chart config
const ctx = document.getElementById('myChart');
const myChart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: [],
        datasets: [
            {
                label: 'Total Power',
                data: [],
                borderColor: 'blue'
            }
        ]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            title: {
                display: true,
                text: 'Power Consumption'
            }
        },
        scales: {
            x: {
                title: {
                    display: true,
                    text: 'Time (s)'
                },
            },
            y: {
                title: {
                    display: true,
                    text: 'Power Consumption (W)',
                },
                min: 0,
            }
        }
    }
})

// player variable
var dashjsPlayer;

// disable startMeasurementButton before connection to server is started
const startMeasurementButton = document.getElementById("startMeasurementButton");
startMeasurementButton.disabled = true;

// disable stop analytics button before
const stopAnalyticsButton = document.getElementById("stopAnalyticsButton");
stopAnalyticsButton.disabled = true;

// when event "ReceiveMeasurement" occurs it updates the powerConsumption text field and adds the value to the dataList text field
connectionMeasurementHub.on("ReceiveMeasurement", function (totalPower, timeSinceStart) {
    // console.log(totalPower);
    myChart.data.labels.push(timeSinceStart);
    // console.log(myChart.data.labels);
    // console.log(myChart.data.datasets);
    myChart.data.datasets.forEach(function (dataset) {
        dataset.data.push(totalPower);
    });
    myChart.update();
});

// start connection to measurementHub and if successful enable startMeasurementButton
connectionMeasurementHub.start().then(function () {
    startMeasurementButton.disabled = false;

}).catch(function (err) {
    return console.error(err.toString());
});

// start connection to playbackHub
connectionPlaybackHub.start().catch(function (err) {
    return console.error(err.toString());
})

// bind buttons on webpage to class functions on backend
startMeasurementButton.addEventListener("click", function (event) {
    connectionMeasurementHub
        .send("StartMeasurement")
        .catch(function (err) {
            console.error(err.toString());
        });
    event.preventDefault();
});
document.getElementById("clearMeasurementsButton").addEventListener("click", function (event) {
    myChart.data.labels.length = 0; // empty chart label array
    myChart.data.datasets.forEach(function (dataset) {
        dataset.data.length = 0;
    });
    myChart.update();
    connectionMeasurementHub.invoke("ClearMeasurements").catch(function (err) { console.error(err.toString()) });
})
document.getElementById("saveMeasurementsButton").addEventListener("click", function (event) {
    connectionMeasurementHub.invoke("SaveMeasurements").catch(function (err) { console.error(err.toString()) });
})



var numberOfMeasurementsForm = document.getElementById("numberOfMeasurementsForm");
numberOfMeasurementsForm.addEventListener('submit', function (event) {
    event.preventDefault();
    // if (!numberOfMeasurementsForm.checkValidity()) {
    event.stopPropagation();
    // }
    numberOfMeasurementsForm.classList.add('was-validated');
    if (numberOfMeasurementsForm.checkValidity()) {
        var inputField = document.getElementById("numberOfMeasurementsInput");
        connectionMeasurementHub.invoke("SaveNumberOfMeasurements", Number(inputField.value));
    }
}, false)

document.getElementById("startAnalyticsButton").addEventListener("click", function (event) {
    // disable all buttons of the ui except for the stopAnalyticsButton
    document.getElementById("startAnalyticsButton").disabled = true;
    document.getElementById("startAnalyticsButton").disabled = true;
    document.getElementById("startMeasurementButton").disabled = true;
    document.getElementById("clearMeasurementsButton").disabled = true;
    document.getElementById("saveMeasurementsButton").disabled = true;
    document.getElementById("numberOfMeasurementsInput").disabled = true;
    document.getElementById("settingsButton").disabled = true;

    // disable all ways of user to interact with dash player, just keep playing videos
    document.getElementById("videoController").style.pointerEvents = 'none';
    document.getElementById("videoControllerWrapper").style.cursor = 'not-allowed';

    // disable navbar
    document.getElementById("navbarMain").style.pointerEvents = 'none';
    document.getElementById("mainNavbarWrapper").style.cursor = 'not-allowed';

    stopAnalyticsButton.disabled = false;
    startPlaybackWithAllSettings()

});

// when stopAnalyticsButton is clicked, enable all buttons of the ui except for the stopAnalyticsButton
stopAnalyticsButton.addEventListener("click", function (event) {
    enableUI();
    stopPlayback();

});

// method to enable all ui clickable elements
function enableUI() {
    document.getElementById("startAnalyticsButton").disabled = false;
    document.getElementById("startMeasurementButton").disabled = false;
    document.getElementById("clearMeasurementsButton").disabled = false;
    document.getElementById("saveMeasurementsButton").disabled = false;
    document.getElementById("numberOfMeasurementsInput").disabled = false;
    document.getElementById("settingsButton").disabled = false;

    // enable all ways of user to interact with dash player, just keep playing videos
    document.getElementById("videoController").style.pointerEvents = 'auto';
    document.getElementById("videoControllerWrapper").style.cursor = 'auto';

    // enable navbar
    document.getElementById("navbarMain").style.pointerEvents = 'auto';
    document.getElementById("mainNavbarWrapper").style.cursor = 'auto';

    stopAnalyticsButton.disabled = true;
}


// method to start playback of with all the different settings in the mpd file. the different resolutions, bitrates, etc.
async function startPlaybackWithAllSettings() {
    const adaptationSets = dashjsPlayer.getTracksFor("video");
    const abrConfig = {
        'streaming': {
            'abr': {
                'autoSwitchBitrate': {
                    'video': false
                }
            }
        }
    };

    //console.log(adaptationSets);

    for (const adaptationSet of adaptationSets) {
        await playAllRepresentations(adaptationSet, abrConfig);
    }

    enableUI();
}

async function playAllRepresentations(adaptationSet, abrConfig) {
    const representations = adaptationSet.bitrateList;
    dashjsPlayer.setCurrentTrack(adaptationSet);

    //console.log(representations);


    for (var j = 0; j < representations.length; j++) {
        //console.log(representations[j]);
        await playRepresentation(j, abrConfig);
    }
}

async function playRepresentation(representationIndex, abrConfig) {
    
    try {
        // Use a promise to wait for the PLAYBACK_ENDED event
        const playbackEndedPromise = new Promise(resolve => {
            function playbackEndedHandler() {
                dashjsPlayer.off(dashjs.MediaPlayer.events.PLAYBACK_ENDED, playbackEndedHandler);
                resolve();
            }
            dashjsPlayer.on(dashjs.MediaPlayer.events.PLAYBACK_ENDED, playbackEndedHandler);
        });

        // Update settings and start playback
        dashjsPlayer.updateSettings(abrConfig);
        dashjsPlayer.setQualityFor("video", representationIndex, true);
        startPlayback();

        // Wait for the PLAYBACK_ENDED event
        await playbackEndedPromise;
    } catch (error) {
        console.error("An error occurred during playback:", error);
    } finally {
        // Stop playback regardless of success or failure
        stopPlayback();
    }
}

// Utility function to simulate asynchronous behavior (sleep)
//function sleep(ms) {
//    return new Promise(resolve => setTimeout(resolve, ms));
//}



// method to start playback
function startPlayback() {
    dashjsPlayer.play();
}

// method to stop playback
function stopPlayback() {
    dashjsPlayer.pause();
}


// helper functions to send changes in playback state to server
function playbackStarted() {
    connectionPlaybackHub
        .invoke("StartPlayback")
        .catch(function (err) {
            console.error(err.toString());
        })
        .then(function () {
            console.log("playback started");
        });
}
function playbackPaused() {
    connectionPlaybackHub
        .invoke("StopPlayback")
        .catch(function (err) {
            console.error(err.toString());
        }).then(function () {
            console.log("playback paused");
        });
}

// init function to run on pageload 
function init() {
    // https://media.axprod.net/TestVectors/v7-Clear/Manifest_MultiPeriod.mpd
    var url = "https://dash.akamaized.net/akamai/bbb_30fps/bbb_30fps.mpd";
    var videoElement = document.querySelector(".videoContainer video");
    var player = dashjs.MediaPlayer().create();
    dashjsPlayer = player;

    player.initialize(videoElement, url, false);
    var controlbar = new ControlBar(player);
    controlbar.initialize();
    player.updateSettings({
        streaming: {
            buffer: {
                fastSwitchEnabled: true, /* enables buffer replacement when switching bitrates for faster switching */
            },
            scheduling: {
                scheduleWhilePaused: false, /* stops the player from loading segments while paused */
            }
        }
    });
    player.on(dashjs.MediaPlayer.events["PLAYBACK_STARTED"], playbackStarted);
    player.on(dashjs.MediaPlayer.events["PLAYBACK_PAUSED"], playbackPaused);
}
