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
        .send("StartMeasurementFixedOps")
        .catch(function (err) {
            console.error(err.toString());
        });
    event.preventDefault();
});
document.getElementById("clearMeasurementsButton").addEventListener("click", function (event) {
    clearMeasurements();
})
document.getElementById("saveMeasurementsButton").addEventListener("click", function (event) {
    connectionMeasurementHub.invoke("SaveMeasurements").catch(function (err) { console.error(err.toString()) });
})

// function to clear meassurements
function clearMeasurements() {
    myChart.data.labels.length = 0; // empty chart label array
    myChart.data.datasets.forEach(function (dataset) {
        dataset.data.length = 0;
    });
    myChart.update();
    connectionMeasurementHub.invoke("ClearMeasurements").catch(function (err) { console.error(err.toString()) });
}



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


var videoUrlsForm = document.getElementById("videoUrlsForm");

// add event listener to videoUrlsForm to prevent default behaviour and validate input
videoUrlsForm.addEventListener('submit', function (event) {

    event.preventDefault();


    event.stopPropagation();


    saveVideoUrl();
}, false)

// add event listener to startAnalyticsButton to start analytics
document.getElementById("startAnalyticsButton").addEventListener("click", async function (event) {
    // Disable the "Start Analytics" button
    this.disabled = true;


    // Disable all buttons and inputs
    disableUI();

    // Disable all ways of user interaction with the dash player
    disablePlayerInteraction();

    // Enable the "Stop Analytics" button
    stopAnalyticsButton.disabled = false;
    // Iterate over the set of unique URLs and start analytics for each
    await startAnalyticsForAllVideos();

    //disable stop analytics button
    stopAnalyticsButton.disabled = true;
});
async function startAnalyticsForAllVideos() {
    // If there are no videos, use the current video
    if (uniqueURLs.size === 0) {
        await startPlaybackWithAllSettings();
        return;
    }
    // Iterate over the set of unique URLs and start analytics for each
    for (const videoURL of uniqueURLs) {
        await changeVideo(videoURL);

        await startAnalyticsForVideo(videoURL);
    }

    
}
async function startAnalyticsForVideo(videoURL) {

    await startPlaybackWithAllSettings();
}

async function changeVideo(url) {
    const manifestLoadedPromise = new Promise(resolve => {
        function manifestLoadedHandler() {
            dashjsPlayer.off(dashjs.MediaPlayer.events.BUFFER_LOADED, manifestLoadedHandler);
            resolve();
        }
        dashjsPlayer.on(dashjs.MediaPlayer.events.BUFFER_LOADED, manifestLoadedHandler);
    });
    dashjsPlayer.reset();
    var videoElement = document.querySelector(".videoContainer video");
    dashjsPlayer.attachView(videoElement);
    dashjsPlayer.attachSource(url);
    await manifestLoadedPromise;

}

function disableUI() {
    var allButtons = document.querySelectorAll('button');
    allButtons.forEach(function (button) {
        button.disabled = true;
    });

    var allInputs = document.querySelectorAll('input');
    allInputs.forEach(function (input) {
        input.disabled = true;
    });

    // disable navbar
    document.getElementById("navbarMain").style.pointerEvents = 'none';
    document.getElementById("mainNavbarWrapper").style.cursor = 'not-allowed';
}

// Utility function to disable all ways of user interaction with the dash player
function disablePlayerInteraction() {
    document.getElementById("videoController").style.pointerEvents = 'none';
    document.getElementById("videoControllerWrapper").style.cursor = 'not-allowed';
}

// when stopAnalyticsButton is clicked, enable all buttons of the ui except for the stopAnalyticsButton
stopAnalyticsButton.addEventListener("click", function (event) {
    dashjsPlayer.seek(0);
    enableUI();
    stopPlayback();
    connectionMeasurementHub.invoke("StopMeasurement").catch(function (err) { console.error(err.toString()) });

});

// method to enable all ui clickable elements
function enableUI() {
    var allButtons = document.querySelectorAll('button');
    allButtons.forEach(function (button) {
        button.disabled = false;
    });

    var allInputs = document.querySelectorAll('input');
    allInputs.forEach(function (input) {
        input.disabled = false;
    });


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
        dashjsPlayer.seek(0);
        startAnalyticsMeasurement();
        startPlayback();

        // Wait for the PLAYBACK_ENDED event
        await playbackEndedPromise;
    } catch (error) {
        console.error("An error occurred during playback:", error);
    } finally {
        // Stop playback regardless of success or failure
        stopPlayback();
        // Stop analytics measurement
        connectionMeasurementHub
            .invoke("StopMeasurement")
            .catch(function (err) {
                console.error(err.toString());
            });
    }
}

// method to start analytics measurement
function startAnalyticsMeasurement() {
    // invoke clear measurements
    clearMeasurements();
    connectionMeasurementHub
        .send("StartMeasurementUntilEnd")
        .catch(function (err) {
            console.error(err.toString());
        })
        .then(function () {
            console.log("measurement started");
        });
}
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

var uniqueURLs = new Set();

// Function to save video URL
function saveVideoUrl() {
    var videoURLInput = document.getElementById('videoUrl');
    var videoURL = videoURLInput.value;

    if (isValidURL(videoURL)) {
        addURLToList(videoURL);
        videoURLInput.value = '';
    } else {
        videoURLInput.setCustomValidity('Please enter a valid URL');
        videoURLInput.reportValidity();
    }
}

// Function to check if a string is a valid URL
function isValidURL(url) {
    try {
        new URL(url);
        return true;
    } catch (error) {
        return false;
    }
}

// Function to add URL to the list
function addURLToList(url) {
    var urlList = document.getElementById('urlList');

    if (uniqueURLs.has(url)) {
        alert('This URL is already in the list.');
        return;
    }

    var listItem = document.createElement('li');
    listItem.className = 'list-group-item d-flex justify-content-between align-items-center';

    var deleteButton = document.createElement('button');
    deleteButton.textContent = 'Delete';
    deleteButton.className = 'btn btn-danger btn-sm';
    deleteButton.onclick = function () {
        urlList.removeChild(listItem);
        uniqueURLs.delete(url);
        checkDisplay();
    };

    var textSpan = document.createElement('span');
    textSpan.textContent = url;
    textSpan.style.marginRight = '10px';

    listItem.appendChild(textSpan);
    listItem.appendChild(deleteButton);

    urlList.appendChild(listItem);

    uniqueURLs.add(url);

    checkDisplay();
}

// Function to check and toggle the display of the list container
function checkDisplay() {
    var videoUrlList = document.getElementById('videoUrlList');
    var displayValue = uniqueURLs.size > 0 ? 'block' : 'none';
    videoUrlList.style.display = displayValue;
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
                fastSwitchEnabled: true, /* enables buffer replacement when switching bitra
                
                tes for faster switching */
            },
            scheduling: {
                scheduleWhilePaused: false, /* stops the player from loading segments while paused */
            }
        }
    });
    player.on(dashjs.MediaPlayer.events["PLAYBACK_STARTED"], playbackStarted);
    player.on(dashjs.MediaPlayer.events["PLAYBACK_PAUSED"], playbackPaused);
}
