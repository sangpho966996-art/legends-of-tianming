const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const cors = require('cors');
const helmet = require('helmet');
require('dotenv').config();

const authRoutes = require('./routes/auth');
const playerRoutes = require('./routes/players');
const combatRoutes = require('./routes/combat');
const guildRoutes = require('./routes/guilds');
const economyRoutes = require('./routes/economy');
const worldRoutes = require('./routes/world');

const SocketManager = require('./services/SocketManager');
const GameService = require('./services/GameService');

class GameServer {
    constructor() {
        this.app = express();
        this.server = http.createServer(this.app);
        this.io = socketIo(this.server, {
            cors: {
                origin: process.env.CLIENT_URL || "http://localhost:3000",
                methods: ["GET", "POST"]
            }
        });
        
        this.setupMiddleware();
        this.setupRoutes();
        this.setupSocket();
        this.gameService = new GameService();
        this.socketManager = new SocketManager(this.io, this.gameService);
    }

    setupMiddleware() {
        this.app.use(helmet());
        this.app.use(cors({
            origin: process.env.CLIENT_URL || "http://localhost:3000",
            credentials: true
        }));
        this.app.use(express.json({ limit: '10mb' }));
        this.app.use(express.urlencoded({ extended: true }));
    }

    setupRoutes() {
        this.app.use('/api/v1/auth', authRoutes);
        this.app.use('/api/v1/players', playerRoutes);
        this.app.use('/api/v1/combat', combatRoutes);
        this.app.use('/api/v1/guilds', guildRoutes);
        this.app.use('/api/v1/economy', economyRoutes);
        this.app.use('/api/v1/world', worldRoutes);

        this.app.get('/health', (req, res) => {
            res.json({ 
                status: 'healthy', 
                timestamp: new Date().toISOString(),
                players: this.gameService.getPlayerCount(),
                uptime: process.uptime()
            });
        });

        this.app.get('/', (req, res) => {
            res.json({ 
                message: 'Legends of Tianming Game Server',
                version: '1.0.0',
                status: 'running'
            });
        });
    }

    setupSocket() {
        this.io.on('connection', (socket) => {
            console.log(`Player connected: ${socket.id}`);
            
            socket.on('disconnect', () => {
                console.log(`Player disconnected: ${socket.id}`);
                this.socketManager.handleDisconnect(socket);
            });
        });
    }

    start(port = process.env.PORT || 3001) {
        this.server.listen(port, () => {
            console.log(`🎮 Legends of Tianming Server running on port ${port}`);
            console.log(`🌐 Health check: http://localhost:${port}/health`);
            console.log(`📊 Environment: ${process.env.NODE_ENV || 'development'}`);
        });
    }
}

module.exports = GameServer;
