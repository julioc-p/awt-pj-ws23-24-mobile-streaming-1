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

});

// when stopAnalyticsButton is clicked, enable all buttons of the ui except for the stopAnalyticsButton
stopAnalyticsButton.addEventListener("click", function (event) {
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
});


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
    var url = "https://media.axprod.net/TestVectors/v7-Clear/Manifest_MultiPeriod.mpd";
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
