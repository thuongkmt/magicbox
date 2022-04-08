const electron = require("electron");
const url = require("url");
const patch = require("path");

const {
    app,
    BrowserWindow
} = electron;

let mainWindow;

app.on("ready", function () {
    mainWindow = new BrowserWindow();
    mainWindow.loadURL(url.format({
        pathname: patch.join(__dirname, 'pages/welcome.html'),
        // pathname: patch.join(__dirname, 'index.html'),
        protocol: 'file:',
        slashes: true
    }));

    mainWindow.maximize();
    mainWindow.setFullScreen(true);

    electron.globalShortcut.register('CommandOrControl+Shift+K', () => {
        electron.BrowserWindow.getFocusedWindow().webContents.openDevTools()
    })
    electron.globalShortcut.register('f5', () => {
        mainWindow.reload();
    })
});

app.on('window-all-closed', app.quit);