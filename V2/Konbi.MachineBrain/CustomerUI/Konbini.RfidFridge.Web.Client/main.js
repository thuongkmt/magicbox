const electron = require("electron");
const url = require("url");
const patch = require("path");

const {
    app,
    BrowserWindow
} = electron;

let mainWindow;

app.on("ready", function () {
    var electronScreen = electron.screen;
    let displays = electronScreen.getAllDisplays()
    let externalDisplay = displays.find((display) => {
        return display.bounds.x !== 0 || display.bounds.y !== 0
    })

    // if (externalDisplay) {
    //     mainWindow = new BrowserWindow({
    //         x: externalDisplay.bounds.x + 50,
    //         y: externalDisplay.bounds.y + 50
    //     });
    // } else {
    //     mainWindow = new BrowserWindow();
    // }

    mainWindow = new BrowserWindow();


    mainWindow.loadURL(url.format({
        // pathname: patch.join(__dirname, 'pages/welcome.html'),
        // pathname: patch.join(__dirname, 'index.html'),
        //pathname: patch.join(__dirname, 'index.html'),
        pathname: patch.join(__dirname, 'loading.html'),
        protocol: 'file:',
        slashes: true
    }));


    // app.dock.hide();
    mainWindow.setAlwaysOnTop(true, "floating");
    mainWindow.setVisibleOnAllWorkspaces(true);
    mainWindow.setFullScreenable(true);
    mainWindow.maximize();
    mainWindow.setFullScreen(true);

    electron.globalShortcut.register('CommandOrControl+Shift+K', () => {
        electron.BrowserWindow.getFocusedWindow().webContents.openDevTools()
    })
    electron.globalShortcut.register('f5', () => {
        mainWindow.reload();
    })
    electron.globalShortcut.register('f6', () => {
        mainWindow.setAlwaysOnTop(false, "floating");
    })
    electron.globalShortcut.register('f7', () => {
        mainWindow.setAlwaysOnTop(true, "floating");
    })
});

app.on('window-all-closed', app.quit);