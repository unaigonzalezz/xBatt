// global websocket, used to communicate from/to Stream Deck software
var websocket = null,
    uuid = null,
    inInfo = null,
    actionInfo = {},
    settingsModel = {
        ControllerNumber: 1 // Default value
    };

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    actionInfo = JSON.parse(inActionInfo);
    inInfo = JSON.parse(inInfo);
    websocket = new WebSocket('ws://localhost:' + inPort);

    // Initialize values
    if (actionInfo.payload.settings.settingsModel) {
        settingsModel.ControllerNumber = actionInfo.payload.settings.settingsModel.ControllerNumber || 1;
    }

    document.getElementById('controllerValue').value = settingsModel.ControllerNumber;

    websocket.onopen = function () {
        var json = { event: inRegisterEvent, uuid: inUUID };
        // Register property inspector to Stream Deck
        websocket.send(JSON.stringify(json));
    };

    websocket.onmessage = function (evt) {
        var jsonObj = JSON.parse(evt.data);
        var sdEvent = jsonObj['event'];
        switch (sdEvent) {
            case "didReceiveSettings":
                if (jsonObj.payload.settings.settingsModel && jsonObj.payload.settings.settingsModel.ControllerNumber !== undefined) {
                    settingsModel.ControllerNumber = jsonObj.payload.settings.settingsModel.ControllerNumber;
                    document.getElementById('controllerValue').value = settingsModel.ControllerNumber;
                }
                break;
            default:
                break;
        }
    };
}

const setSettings = (value, param) => {
    const controllerValue = parseInt(value);

    if (controllerValue < 1 || controllerValue > 4 || isNaN(controllerValue)) {
        document.getElementById('error-message').style.display = 'block';
        settingsModel.ControllerNumber = 1; // Valor por defecto si no es válido
    } else {
        document.getElementById('error-message').style.display = 'none';
        settingsModel.ControllerNumber = controllerValue;
    }

    if (websocket) {
        var json = {
            "event": "setSettings",
            "context": uuid,
            "payload": {
                "settingsModel": settingsModel
            }
        };
        websocket.send(JSON.stringify(json));
    }
};
