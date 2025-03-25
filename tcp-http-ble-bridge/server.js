const express = require('express');
const net = require('net');
const path = require('path');

const app = express();
const port = 3000; // Port for the Express server
let client; // TCP client reference
let isConnected = false; // Track connection status

// Middleware to parse JSON and URL-encoded data
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Middleware to serve static files from the 'public' folder
app.use(express.static(path.join(__dirname, 'public')));

// Function to attempt TCP connection
function connectToTCPServer(messageToSend) {
    if (!isConnected) {
        console.log('Attempting to connect to TCP server...');
        client = new net.Socket();

        client.connect(50505, '127.0.0.1', () => {
            isConnected = true;
            console.log('Connected to TCP server');
            if (messageToSend) client.write(messageToSend);
        });

        client.on('data', (data) => {
            console.log('Received from TCP server:', data.toString());
        });

        client.on('error', (err) => {
            console.error('TCP connection error:', err.message);
            isConnected = false; // Mark as disconnected and retry
            retryConnection();
        });

        client.on('close', () => {
            console.log('TCP connection closed');
            isConnected = false; // Mark as disconnected and retry
            retryConnection();
        });
    } else if (messageToSend) {
        client.write(messageToSend);
    }
}

// Function to retry connection
function retryConnection() {
    if (!isConnected) {
        setTimeout(() => {
            connectToTCPServer(); // Retry connection
        }, 3000); // Retry every 3 seconds
    }
}

// HTTP GET Endpoint to send a message
app.get('/send', (req, res) => {
    const message = req.query.message || 'Hello from Express!'; // Message to send to TCP server
    connectToTCPServer(message);
    res.send(`Message "${message}" sent to TCP server (or queued if not connected).`);
});

// HTTP POST Endpoint to send a message
app.post('/send', (req, res) => {
    const message = req.body.message || 'Hello from Express (POST)!'; // Message from request body
    connectToTCPServer(message);
    res.send(`Message "${message}" sent to TCP server (or queued if not connected).`);
});

// HTTP GET Endpoint to disconnect TCP Client
app.get('/disconnect', (req, res) => {
    if (client && !client.destroyed) {
        client.destroy(); // Disconnect the client
        isConnected = false;
        console.log('TCP client disconnected');
        res.send('TCP client connection terminated.');
    } else {
        res.send('No active TCP client connection to terminate.');
    }
});

// Start the Express server
app.listen(port, () => {
    console.log(`Express server running at http://localhost:${port}`);
    console.log(`Serving static files from ${path.join(__dirname, 'public')}`);
   retryConnection();
});
